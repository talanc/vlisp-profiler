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

        public IEnumerable<TraceItem> GetTraces()
        {
            var traces = new List<TraceItem>();
            TraceItem trace = null;
            var stack = new Stack<TraceItem>();
            string line;
            int lineNum = 0;

            while ((line = _tr.ReadLine()) != null)
            {
                lineNum++;

                var arr = line.Trim().Split(',');
                if (arr.Length == 3)
                {
                    if (trace == null)
                    {
                        if (arr[1] != "In")
                            throw new Exception($"expected 'In' at line {lineNum}");

                        trace = new TraceItem()
                        {
                            Id = int.Parse(arr[0]),
                            InElapsed = TimeSpan.Parse(arr[2])
                        };

                        stack.Push(trace);
                    }
                    else if (arr[1] == "In")
                    {
                        if (trace.Items == null)
                            trace.Items = new List<TraceItem>();

                        var newTrace = new TraceItem()
                        {
                            Id = int.Parse(arr[0]),
                            InElapsed = TimeSpan.Parse(arr[2])
                        };

                        trace.Items.Add(newTrace);
                        stack.Push(newTrace);
                        trace = newTrace;
                    }
                    else if (arr[1] == "Out")
                    {
                        if (trace == null)
                            throw new Exception($"unexpected 'Out' at line {lineNum}");

                        if (arr[0] != trace.Id.ToString())
                            throw new Exception($"expected id '{trace.Id}', not '{arr[0]}', at line {lineNum}");

                        trace.OutElapsed = TimeSpan.Parse(arr[2]);
                        
                        if (stack.Count == 1)
                        {
                            traces.Add(trace);
                            trace = null;
                            stack.Pop();
                        }
                        else
                        {
                            trace = stack.Pop();
                        }
                    }
                    else
                    {
                        throw new Exception($"unexpected symbol type '{arr[1]}' at line {lineNum}");
                    }
                }
            }

            if (stack.Count > 0)
                throw new Exception($"unexpected items in stack {stack.Count}");

            return traces;
        }
    }
}
