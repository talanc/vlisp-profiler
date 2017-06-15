using System;
using System.Collections.Generic;
using System.Text;

namespace VLispProfiler
{
    public enum Token
    {
        Illegal,
        EndOfFile,
        Comment,        // Line comment: "; hello"   Inline comment: ;| hello |;

        Identifier,     // defun
        Int,            // 3
        Real,           // 3.14
        String,         // "PI"

        Quote,
        Dot,
        ParenLeft,
        ParenRight
    }

    public sealed class VLispStrings
    {
        public const char Quote = '\'';
        public const char Dot = '.';
        public const char ParenLeft = '(';
        public const char ParenRight = ')';

        public const string Cond = "cond";
        public const string Defun = "defun";
        public const string Lambda = "lambda";
    }

    public struct FilePosition
    {
        public int Line { get; set; }
        public int Column { get; set; }

        public FilePosition(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            return $"{Line}:{Column}";
        }
    }
}
