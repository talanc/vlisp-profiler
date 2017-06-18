using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VLispProfiler
{
    public class Scanner
    {
        private string _input;
        private int _currentPosition = 0;
        private List<int> _lineOffsets;
        private List<ScannerStack> _stacks = new List<ScannerStack>();

        public int CurrentStartPos { get; private set; }
        public int CurrentTokenStartPos { get; private set; }
        public Token CurrentToken { get; private set; }
        public string CurrentLiteral { get; private set; }
        public string CurrentError { get; private set; }

        public bool RescanComments { get; set; }

        public Scanner(string input)
        {
            _input = input;
            RescanComments = true;
        }

        public ScannerStack PushStack()
        {
            return new ScannerStack(this);
        }

        public void PopStack()
        {
            _stacks.Last().Dispose();
        }

        public bool ScanIf(Token? token, string literal, bool caseSens = false)
        {
            var cmp = (caseSens ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);

            // save state
            var prevPos = _currentPosition;
            var prevStartPos = CurrentStartPos;
            var prevTokenStartPos = CurrentTokenStartPos;
            var prevToken = CurrentToken;
            var prevLiteral = CurrentLiteral;
            var prevError = CurrentError;

            Scan();

            if ((!token.HasValue || token == CurrentToken) && (literal == null || literal.Equals(CurrentLiteral, cmp)))
            {
                return true;
            }

            // restore state
            _currentPosition = prevPos;
            CurrentStartPos = prevStartPos;
            CurrentTokenStartPos = prevTokenStartPos;
            CurrentToken = prevToken;
            CurrentLiteral = prevLiteral;
            CurrentError = prevError;

            return false;
        }

        public Token Scan()
        {
            rescan:

            CurrentStartPos = _currentPosition;

            CurrentTokenStartPos = -1;
            CurrentToken = Token.Illegal;
            CurrentLiteral = null;
            CurrentError = null;

            SkipWhiteSpace();

            CurrentTokenStartPos = _currentPosition;

            // end of file
            if (_currentPosition == _input.Length)
            {
                return CurrentToken = Token.EndOfFile;
            }

            var ch = _input[_currentPosition];

            if (ch == '\'')
            {
                _currentPosition++;
                CurrentToken = Token.Quote;
                return CurrentToken;
            }

            if (ch == '.')
            {
                _currentPosition++;
                CurrentToken = Token.Dot;
                return CurrentToken;
            }

            if (ch == '(')
            {
                _currentPosition++;
                CurrentToken = Token.ParenLeft;
                return CurrentToken;
            }

            if (ch == ')')
            {
                _currentPosition++;
                CurrentToken = Token.ParenRight;
                return CurrentToken;
            }

            if (ch == ';')
            {
                _currentPosition++;

                int i = _currentPosition;

                if (i < _input.Length)
                {
                    var ch2 = _input[_currentPosition];
                    _currentPosition++;
                    if (ch2 == '|')
                    {
                        if (SkipTo("|;"))
                        {
                            _currentPosition += "|;".Length;
                        }
                    }
                    else
                    {
                        if (SkipTo("\n"))
                        {
                            _currentPosition--; // don't include newline
                        }
                    }
                }

                if (RescanComments)
                    goto rescan;

                CurrentLiteral = GetCurrentLiteral();

                CurrentToken = Token.Comment;
                return CurrentToken;
            }

            if (ch == '"')
            {
                _currentPosition++; // "

                var escape = false;
                for (; _currentPosition < _input.Length; _currentPosition++)
                {
                    if (escape)
                    {
                        escape = false;
                        continue;
                    }

                    var ch2 = _input[_currentPosition];

                    if (ch2 == '\\')
                    {
                        escape = true;
                    }

                    if (ch2 == '"')
                    {
                        _currentPosition++; // "

                        CurrentLiteral = GetCurrentLiteral();
                        CurrentToken = Token.String;
                        return CurrentToken;
                    }
                }

                CurrentToken = Token.Illegal;
                CurrentLiteral = GetCurrentLiteral();
                CurrentError = "malformed string";
                return CurrentToken;
            }

            if (char.IsLetterOrDigit(ch) || char.IsSymbol(ch) || char.IsPunctuation(ch))
            {
                _currentPosition++; // ch
                SkipAtom();

                CurrentLiteral = GetCurrentLiteral();

                if (_regexInt.IsMatch(CurrentLiteral))
                {
                    if (Int32.TryParse(CurrentLiteral, out int result) && result != Int32.MinValue)
                        CurrentToken = Token.Int;
                    else
                        CurrentToken = Token.Real;
                }
                else if (_regexFloat1.IsMatch(CurrentLiteral) || _regexFloat2.IsMatch(CurrentLiteral) || _regexFloat3.IsMatch(CurrentLiteral))
                    CurrentToken = Token.Real;
                else
                    CurrentToken = Token.Identifier;

                return CurrentToken;
            }

            return CurrentToken;
        }

        public FilePosition GetLinePosition(int position)
        {
            if (position < 0 || position > _input.Length)
                throw new ArgumentOutOfRangeException(nameof(position));

            GetLineOffsets();

            // todo, binary search
            for (var i = 1; i < _lineOffsets.Count; i++)
            {
                if (position < _lineOffsets[i])
                {
                    return new FilePosition(i, position - _lineOffsets[i - 1] + 1);
                }
            }

            return new FilePosition(_lineOffsets.Count, position - _lineOffsets.Last() + 1);
        }

        private IList<int> GetLineOffsets()
        {
            if (_lineOffsets == null)
            {
                _lineOffsets = new List<int> { 0 };

                for (var i = 0; i < _input.Length - 1; i++)
                {
                    if (_input[i] == '\n')
                        _lineOffsets.Add(i + 1);
                }
            }
            return _lineOffsets;
        }

        private string GetCurrentLiteral()
        {
            return _input.Substring(CurrentTokenStartPos, _currentPosition - CurrentTokenStartPos);
        }

        private void SkipWhiteSpace()
        {
            while (_currentPosition < _input.Length && char.IsWhiteSpace(_input, _currentPosition))
            {
                _currentPosition++;
            }
        }

        private bool SkipTo(string s)
        {
            _currentPosition = _input.IndexOf(s, _currentPosition);
            if (_currentPosition == -1)
            {
                _currentPosition = _input.Length;
                return false;
            }
            return true;
        }

        private void SkipAtom()
        {
            while (_currentPosition < _input.Length)
            {
                var ch2 = _input[_currentPosition];
                if (char.IsWhiteSpace(ch2) || ch2 == '(' || ch2 == ')' || ch2 == '"')
                {
                    break;
                }

                _currentPosition++;
            }
        }

        private readonly static Regex _regexInt = new Regex("^[+-]?[0-9]+$");
        private readonly static Regex _regexFloat1 = new Regex(@"^[+-]?[0-9]+\.[0-9]*$");
        private readonly static Regex _regexFloat2 = new Regex(@"^[+-]?[0-9]+\.?e[+-]?[0-9]+$");
        private readonly static Regex _regexFloat3 = new Regex(@"^[+-]?[0-9]+\.[0-9]+e[+-]?[0-9]+$");

        public class ScannerStack : IDisposable
        {
            private Scanner _scanner;
            private bool _commit;
            private bool _dispose;

            private int PrevPosition { get; set; }
            private int PrevStartPos { get;  set; }
            private int PrevTokenStartPos { get; set; }
            private Token PrevToken { get; set; }
            private string PrevLiteral { get; set; }
            private string PrevError { get; set; }

            public ScannerStack(Scanner scanner)
            {
                _scanner = scanner;
                _commit = false;
                _dispose = false;

                _scanner._stacks.Add(this);

                PrevPosition = _scanner._currentPosition;
                PrevStartPos = _scanner.CurrentStartPos;
                PrevTokenStartPos = _scanner.CurrentTokenStartPos;
                PrevToken = _scanner.CurrentToken;
                PrevLiteral = _scanner.CurrentLiteral;
                PrevError = _scanner.CurrentError;
            }

            public void Commit()
            {
                _commit = true;
                Pop();
            }

            private void Pop()
            {
                var last = _scanner._stacks.Last();
                if (last != this)
                    throw new ScannerException("Stacks must be committed in FILO order");

                _scanner._stacks.RemoveAt(_scanner._stacks.Count - 1);
            }

            public void Dispose()
            {
                if (_dispose) return;
                _dispose = true;
                if (_commit) return;
                _commit = true;

                Pop();

                _scanner._currentPosition = PrevPosition;
                _scanner.CurrentStartPos = PrevStartPos;
                _scanner.CurrentTokenStartPos = PrevTokenStartPos;
                _scanner.CurrentToken = PrevToken;
                _scanner.CurrentLiteral = PrevLiteral;
                _scanner.CurrentError = PrevError;
            }
        }
    }
}
