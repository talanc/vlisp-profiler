using System;
using System.Collections.Generic;
using System.Text;

namespace VLispProfiler
{
    public class ParserException : Exception
    {
        public ParserException() : base()
        {
        }

        public ParserException(string message) : base(message)
        {
        }
    }

    public class ScannerException : Exception
    {
        public ScannerException() : base()
        {
        }

        public ScannerException(string message) : base(message)
        {
        }
    }

    public class TraceReaderException : Exception
    {
        public TraceReaderException() : base() { }
        public TraceReaderException(string message) : base(message) { }
    }
}
