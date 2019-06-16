using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLispProfiler.View
{
    public class TopReport
    {
        private VLispPath _path;
        private TextWriter _output;
        private int _top;
        private TraceSummary _summary;
        private SymbolFile _symbolFile;

        public TopReport(string filePath, TextWriter output, int top)
        {
            _path = new VLispPath(filePath);
            _output = output;
            _top = top;

            using (var sr = new StreamReader(_path.TracesPath))
            {
                var reader = new TraceReader(sr);
                var traceItem = reader.GetTrace();
                _summary = new TraceSummary(traceItem);
            }

            _symbolFile = SymbolReader.ReadSymbolFile(_path.SymbolPath);
        }

        public void Display()
        {
            var elapsed = _summary.GetTopElapsed().Take(_top);
            _output.WriteLine("Rank,Elapsed,Elapsed (Self),Called,Pos,Preview");
            var i = 0;
            foreach (var item in elapsed)
            {
                i++;
                var symbol = _symbolFile.GetOrDefault(item.Id);

                var preview = "?";
                var pos = "?";
                if (item.Id == 0)
                {
                    preview = "Root";
                    pos = "N/A";
                }
                else if (symbol != null)
                {
                    preview = string.IsNullOrWhiteSpace(symbol.Preview) ? symbol.SymbolType : symbol.Preview;
                    pos = symbol.StartPos.ToString();
                }
                else
                {
                    preview = $"ID={item.Id}";
                    pos = "?";
                }

                _output.WriteLine($"{i},{TimeStr(item.Elapsed)},{TimeStr(item.SelfElapsed)},{item.Called},{pos},{preview}");
            }
        }

        private string TimeStr(TimeSpan value) => value.TotalMilliseconds.ToString("0") + "ms";
    }
}
