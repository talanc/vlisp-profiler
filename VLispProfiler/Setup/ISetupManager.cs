using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLispProfiler.Setup
{
    public interface ISetupManager
    {
        IEnumerable<SetupInstance> GetSetups();
        void InstallSetup(SetupInstance setup);
        void UninstallSetup(SetupInstance setup);
        bool IsInstalled(SetupInstance setup);
        IEnumerable<SetupInstance> FilterSetups(IEnumerable<SetupInstance> setups, IEnumerable<string> filters);
    }
}
