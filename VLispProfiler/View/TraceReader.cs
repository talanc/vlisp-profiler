using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLispProfiler.View
{
    public class TraceReader
    {
        private TextReader _tr;

        public TraceReader(string content)
        {
            _tr = new StringReader(content);
        }

        public TraceReader(Stream stream)
        {
            _tr = new StreamReader(stream);
        }

        public TraceItem GetTrace()
        {
            var root = new TraceItem()
            {
                Id = 0,
                Items = new List<TraceItem>()
            };

            var trace = root;
            
            var stack = new Stack<TraceItem>();
            string line;
            int lineNum = 0;

            while ((line = _tr.ReadLine()) != null)
            {
                lineNum++;

                var arr = line.Trim().Split(',');
                if (arr.Length != 3)
                    continue;

                if (arr[1] == "In")
                {
                    var newTrace = new TraceItem()
                    {
                        Id = int.Parse(arr[0]),
                        InElapsed = TimeSpan.Parse(arr[2]),
                        Items = new List<TraceItem>()
                    };

                    trace.Items.Add(newTrace);
                    stack.Push(trace);
                    trace = newTrace;
                }
                else if (arr[1] == "Out")
                {
                    if (stack.Count == 0)
                        throw new TraceReaderException("unexpected 'Out' with no stack");

                    if (arr[0] != trace.Id.ToString())
                        throw new TraceReaderException($"expected id '{trace.Id}', not '{arr[0]}', at line {lineNum}");

                    trace.OutElapsed = TimeSpan.Parse(arr[2]);

                    trace = stack.Pop();
                }
                else
                {
                    throw new TraceReaderException($"unexpected symbol type '{arr[1]}' at line {lineNum}");
                }
            }

            if (stack.Count > 0)
                throw new TraceReaderException($"unexpected items in stack {stack.Count}");

            root.InElapsed = root.Items.First().InElapsed;
            root.OutElapsed = root.Items.Last().OutElapsed;

            return root;
        }
    }
}
