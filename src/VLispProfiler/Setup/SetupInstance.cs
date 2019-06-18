using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLispProfiler.Setup
{
    public class SetupInstance
    {
        public string ReleaseName { get; set; }
        public string VersionName { get; set; }
        public string ProfileName { get; set; }

        public string FriendlyName => NameHelper.GetFriendlyName(ReleaseName);

        public string FriendNameWithProfile => NameHelper.GetFriendlyNameWithProfile(ReleaseName, ProfileName);
    }
}
