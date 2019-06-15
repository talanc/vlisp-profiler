using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLispProfiler.Setup
{
    public static class NameHelper
    {
        public static string GetFriendlyName(string release) => $"AutoCAD {GetReleaseYear(release)}";

        public static string GetFriendlyNameWithProfile(string release, string profile)
        {
            var friendlyName = GetFriendlyName(release);
            if (profile == "<<Unnamed Profile>>")
                return friendlyName;
            return $"{friendlyName} ({profile})";
        }

        public static string GetReleaseYear(string release)
        {
            switch (release)
            {
                case "R17.2":
                    return "2009";
                case "R18.0":
                    return "2010";
                case "R18.1":
                    return "2011";
                case "R18.2":
                    return "2012";
                case "R19.0":
                    return "2013";
                case "R19.1":
                    return "2014";
                case "R20.0":
                    return "2015";
                case "R20.1":
                    return "2016";
                case "R21.0":
                    return "2017";
                case "R22.0":
                    return "2018";
                case "R23.0":
                    return "2019";
                case "R23.1":
                    return "2020";
            }
            return release;
        }
    }
}
