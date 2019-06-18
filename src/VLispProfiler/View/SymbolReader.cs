using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLispProfiler.View
{
    public static class SymbolReader
    {
        public static SymbolFile ReadSymbolContent(string content)
        {
            using (var sr = new StringReader(content))
            {
                return ReadSymbolFile(sr);
            }
        }

        public static SymbolFile ReadSymbolFile(string filePath)
        {
            using (var sr = new StreamReader(filePath))
            {
                return ReadSymbolFile(sr);
            }
        }

        public static SymbolFile ReadSymbolFile(TextReader reader)
        {
            var lineNum = 0;
            var line = "";

            try
            {
                // header
                lineNum++;
                line = reader.ReadLine();

                // symbols
                var symbols = new Dictionary<int, SymbolItem>();

                while ((line = reader.ReadLine()) != null)
                {
                    lineNum++;

                    var arr = line.Trim().Split(new[] { ',' }, 5);
                    if (arr.Length != 5)
                        continue;

                    var symbol = new SymbolItem();
                    symbol.SymbolId = int.Parse(arr[0]);
                    symbol.SymbolType = arr[1];
                    symbol.StartPos = FilePosition.Parse(arr[2]);
                    symbol.EndPos = FilePosition.Parse(arr[3]);
                    symbol.Preview = arr[4];

                    symbols.Add(symbol.SymbolId, symbol);
                }

                return new SymbolFile() { Symbols = symbols };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error reading symbol file, line={line}, lineNumber={lineNum}", ex);
            }
        }
    }
}
