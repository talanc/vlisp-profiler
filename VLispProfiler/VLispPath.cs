using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLispProfiler
{
    public class VLispPath
    {
        public string FilePath { get; set; }

        public string SymbolPath => $"{FilePath}.symbols.txt";
        public string FileProfilerPath => $"{FilePath}.prof.lsp";

        public VLispPath(string filePath)
        {
            FilePath = filePath;

            FilePathContents = File.ReadAllText(filePath).Replace("\t", "        "); // tabs to 8 char spaces
        }

        public string FilePathContents { get; } 
            
    }
}
