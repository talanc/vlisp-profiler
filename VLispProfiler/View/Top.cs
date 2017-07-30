using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLispProfiler.View
{
    public class Top
    {
        private TextReader _input;
        private TextWriter _output;
        private int _top;

        public Top(string filePath, TextWriter output, int top)
        {
            _input = new StreamReader(filePath);
            _output = output;
            _top = top;
        }

        public void Display()
        {
            _output.WriteLine($"TODO do top {_top}");
        }
    }
}
