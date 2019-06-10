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
        private readonly string currentLocation;

        public SetupManager(string currentLocation)
        {
            this.currentLocation = currentLocation;
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
                    //var profileKey = profilesKey.OpenSubKey(profileName);

                    //var startupKey = profileKey.OpenSubKey(@"Dialogs\Appload\Startup");
                    //var numStartupValue = (string)startupKey.GetValue("NumStartup");
                    //var numStartupInt = int.Parse(numStartupValue);

                    //var profPath = default(string);
                    //var startupActive = false;
                    //for (var i = 1; i <= numStartupInt; i++)
                    //{
                    //    var startupValue = (string)startupKey.GetValue($"{i}Startup");
                    //    if (startupValue.EndsWith("prof.lsp", StringComparison.InvariantCultureIgnoreCase))
                    //    {
                    //        profPath = startupValue;
                    //        startupActive = true;
                    //        break;
                    //    }
                    //}

                    //var dirActive = false;
                    //if (profPath != null)
                    //{
                    //    var profDir = Path.GetDirectoryName(profPath);
                    //    var generalKey = profileKey.OpenSubKey("General");
                    //    var acadValue = (string)generalKey.GetValue("ACAD");
                    //    var acadPaths = acadValue.Split(';');
                    //    foreach (var acadPath in acadPaths)
                    //    {
                    //        if (profDir.Equals(acadPath, StringComparison.InvariantCultureIgnoreCase))
                    //        {
                    //            dirActive = true;
                    //            break;
                    //        }
                    //    }
                    //}

                    var setup = new SetupInstance();
                    setup.ReleaseName = releaseName;
                    setup.VersionName = versionName;
                    setup.ProfileName = profileName;

                    setups.Add(setup);
                }

            }

            return setups;
        }

        public void InstallSetup(SetupInstance setup)
        {
            throw new NotImplementedException();
        }

        public void UninstallSetup(SetupInstance setup)
        {
            throw new NotImplementedException();
        }
    }

    public class SetupInstance
    {
        public string ReleaseName { get; set; }
        public string VersionName { get; set; }
        public string ProfileName { get; set; }

        public string FriendlyName => NameHelper.GetFriendlyName(ReleaseName);
    }
}
