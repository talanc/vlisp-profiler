using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLispProfiler
{
    public static class LispFile
    {
        private const int DefaultTabWidth = 8;

        public static string ReadAllText(string filePath) => ReadAllText(filePath, DefaultTabWidth);

        public static string ReadAllText(string filePath, int tabWidth)
        {
            var contents = File.ReadAllText(filePath);
            return ConvertTabsToSpaces(contents, tabWidth);
        }

        public static string ConvertTabsToSpaces(string contents) => ConvertTabsToSpaces(contents, DefaultTabWidth);

        public static string ConvertTabsToSpaces(string contents, int tabWidth)
        {
            int i;

            while ((i = contents.IndexOf('\t')) != -1)
            {
                var j = contents.LastIndexOf('\n', i) + 1;
                var tabs = new string(' ', tabWidth - ((i - j) % tabWidth));
                contents = contents.Substring(0, i) + tabs + contents.Substring(i + 1);
            }

            return contents;
        }
    }
}
