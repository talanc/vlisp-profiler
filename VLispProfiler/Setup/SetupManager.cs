using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace VLispProfiler.Setup
{
    public interface ISetupManager
    {
        IEnumerable<SetupInstance> GetSetups();
        void InstallSetup(SetupInstance setup);
        void UninstallSetup(SetupInstance setup);
    }

    public class SetupManager //: ISetupManager
    {
        private readonly string currentDir;
        private readonly string userProfilePath;
        private readonly string profLspPath;

        public SetupManager(string currentDir, string userProfilePath)
        {
            this.currentDir = currentDir;
            this.userProfilePath = userProfilePath;

            profLspPath = Path.Combine(currentDir, "prof.lsp");
        }

        public IEnumerable<SetupInstance> GetSetups()
        {
            var setups = new List<SetupInstance>();

            var autocadKey = Registry.CurrentUser.OpenSubKey(@"Software\Autodesk\AutoCAD");
            if (autocadKey == null)
                return setups;

            var autocadSubKeys = autocadKey.GetSubKeyNames();

            foreach (var releaseName in autocadSubKeys)
            {
                var releaseKey = autocadKey.OpenSubKey(releaseName);
                if (releaseKey.SubKeyCount != 1)
                    continue;
                var versionName = releaseKey.GetSubKeyNames().First();
                var versionKey = releaseKey.OpenSubKey(versionName);
                var profilesKey = versionKey.OpenSubKey("Profiles");
                var profilesSubKeyNames = profilesKey.GetSubKeyNames();

                foreach (var profileName in profilesSubKeyNames)
                {
                    var setup = new SetupInstance();
                    setup.ReleaseName = releaseName;
                    setup.VersionName = versionName;
                    setup.ProfileName = profileName;

                    setups.Add(setup);
                }
            }

            return setups;
        }

        public IEnumerable<SetupInstance> FilterSetups(IEnumerable<SetupInstance> setups, IEnumerable<string> filters)
        {
            if (filters.Any(s => s.Equals("all", StringComparison.InvariantCultureIgnoreCase)))
                return setups;

            return setups.Where(setup =>
                filters.Any(s => s.Equals(setup.FriendlyName, StringComparison.InvariantCultureIgnoreCase)) ||
                filters.Any(s => s.Equals(setup.ReleaseName, StringComparison.InvariantCultureIgnoreCase)) ||
                filters.Any(s => s.Equals(NameHelper.GetReleaseYear(setup.ReleaseName), StringComparison.InvariantCultureIgnoreCase)) ||
                filters.Any(s => s.Equals(setup.ProfileName, StringComparison.InvariantCultureIgnoreCase))
            );
        }

        public void InstallSetup(SetupInstance setup)
        {
            if (!File.Exists(profLspPath))
                throw new FileNotFoundException($"Setup requires prof.lsp present in '{currentDir}'", profLspPath);

            AddAcadPath(setup, currentDir);
            AddStartupFile(setup, profLspPath);
        }

        public void UninstallSetup(SetupInstance setup)
        {
            RemoveAcadPath(setup, currentDir);
            RemoveStartupFile(setup, profLspPath);
        }

        private void AddAcadPath(SetupInstance setup, string path)
        {
            var acadPath = GetAcadPath(path);

            using (var generalKey = OpenProfileSubKey(setup, "General", writable: true))
            {
                var acadValue = (string)generalKey.GetValue("ACAD", null, RegistryValueOptions.DoNotExpandEnvironmentNames);

                // already present
                if (acadValue.IndexOf(acadPath, StringComparison.InvariantCultureIgnoreCase) != -1)
                    return;

                var newAcadValue = acadValue;
                if (!newAcadValue.EndsWith(";")) newAcadValue += ";";
                newAcadValue += acadPath + ";";

                generalKey.SetValue("ACAD", newAcadValue, RegistryValueKind.ExpandString);
            }
        }

        private void RemoveAcadPath(SetupInstance setup, string path)
        {
            var acadPath = GetAcadPath(path);

            using (var generalKey = OpenProfileSubKey(setup, "General", writable: true))
            {
                var acadValue = (string)generalKey.GetValue("ACAD", null, RegistryValueOptions.DoNotExpandEnvironmentNames);

                var findAcadPath = acadPath + ";";
                var i = acadValue.IndexOf(findAcadPath, StringComparison.InvariantCultureIgnoreCase);

                // not present
                if (i == -1)
                    return;

                var newAcadValue = acadValue.Substring(0, i) + acadValue.Substring(i + findAcadPath.Length);

                generalKey.SetValue("ACAD", newAcadValue, RegistryValueKind.ExpandString);
            }
        }

        private void AddStartupFile(SetupInstance setup, string lspPath)
        {
            var acadLspPath = GetAcadPath(lspPath);

            using (var startupKey = OpenProfileSubKey(setup, @"Dialogs\Appload\Startup", writable: true))
            {
                var numStartup = int.Parse((string)startupKey.GetValue("NumStartup"));

                // find if already present
                for (var i = 1; i <= numStartup; i++)
                {
                    var startupValue = (string)startupKey.GetValue($"{i}Startup", null, RegistryValueOptions.DoNotExpandEnvironmentNames);

                    if (acadLspPath.Equals(startupValue, StringComparison.InvariantCultureIgnoreCase))
                        return;
                }

                // add
                numStartup++;
                startupKey.SetValue($"{numStartup}Startup", acadLspPath, RegistryValueKind.ExpandString);
                startupKey.SetValue("NumStartup", numStartup.ToString(), RegistryValueKind.String);
            }
        }

        private void RemoveStartupFile(SetupInstance setup, string lspPath)
        {
            var acadLspPath = GetAcadPath(lspPath);

            using (var startupKey = OpenProfileSubKey(setup, @"Dialogs\Appload\Startup", writable: true))
            {
                var numStartup = int.Parse((string)startupKey.GetValue("NumStartup"));

                // find path
                var idx = -1;
                for (var i = 1; i <= numStartup; i++)
                {
                    var startupValue = (string)startupKey.GetValue($"{i}Startup", null, RegistryValueOptions.DoNotExpandEnvironmentNames);

                    if (acadLspPath.Equals(startupValue, StringComparison.InvariantCultureIgnoreCase))
                    {
                        idx = i;
                        break;
                    }
                }

                // not present
                if (idx == -1)
                    return;

                for (var i = idx; i <= numStartup - 1; i++)
                {
                    var nextValue = (string)startupKey.GetValue($"{i + 1}Startup", null, RegistryValueOptions.DoNotExpandEnvironmentNames);

                    startupKey.SetValue($"{i}Startup", nextValue, RegistryValueKind.ExpandString);
                }

                // remove last value
                startupKey.DeleteValue($"{numStartup}Startup");

                // resize NumStartup
                startupKey.SetValue("NumStartup", (numStartup - 1).ToString(), RegistryValueKind.String);
            }
        }

        private RegistryKey OpenProfileSubKey(SetupInstance setup, string path, bool writable)
        {
            var name = $@"Software\Autodesk\AutoCAD\{setup.ReleaseName}\{setup.VersionName}\Profiles\{setup.ProfileName}\{path}";
            var subKey = Registry.CurrentUser.OpenSubKey(name, writable);
            if (subKey == null) throw new InvalidOperationException($"Could not open registry key: {name}");
            return subKey;
        }

        private string GetAcadPath(string path)
        {
            if (path.IndexOf(userProfilePath, StringComparison.InvariantCultureIgnoreCase) == 0)
                return "%USERPROFILE%" + path.Substring(userProfilePath.Length).ToLowerInvariant();
            return path.ToLowerInvariant();
        }
    }

    public class SetupInstance
    {
        public string ReleaseName { get; set; }
        public string VersionName { get; set; }
        public string ProfileName { get; set; }

        public string FriendlyName => NameHelper.GetFriendlyName(ReleaseName);

        public string FriendNameWithProfile => NameHelper.GetFriendlyNameWithProfile(ReleaseName, ProfileName);
    }
}
