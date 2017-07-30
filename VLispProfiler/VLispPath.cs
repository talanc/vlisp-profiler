using System;
using System.Collections.Generic;
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
        }
    }
}
