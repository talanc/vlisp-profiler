using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLispProfiler.View
{
    public class SymbolFile
    {
        public IDictionary<int, SymbolItem> Symbols { get; set; }

        public SymbolItem GetOrDefault(int id)
        {
            if (Symbols != null && Symbols.ContainsKey(id))
                return Symbols[id];
            return null;
        }
    }

    public class SymbolItem
    {
        public int SymbolId { get; set; }
        public string SymbolType { get; set; }
        public FilePosition StartPos { get; set; }
        public FilePosition EndPos { get; set; }
        public string Preview { get; set; }
    }
}
