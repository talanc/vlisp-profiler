using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLispProfiler.View
{
    public class HtmlReport
    {
        private Scanner _scanner;
        private VLispPath _path;
        private TextReader _input;
        private TextWriter _output;

        public HtmlReport(string filePath, string reportPath)
        {
            _scanner = new Scanner(File.ReadAllText(filePath));
            _scanner.RescanComments = false;
            _path = new VLispPath(filePath);
            _input = new StreamReader(filePath);
            _output = new StreamWriter(reportPath);
        }

        public string HtmlHeader { get; set; } = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <meta http-equiv=""X-UA-Compatible"" content=""ie=edge"">
    <title>Document</title>
    <style>
        .lisp-code {
            font-family: Lucida Console;
            white-space: pre;
        }

        .lisp-comment {
            color: purple;
            background-color: lightgrey;
        }

        .lisp-identifier {
            color: blue;
        }

        .lisp-int {
            color: green;
        }

        .lisp-real {
            color: green;
        }

        .lisp-string {
            color: pink;
        }

        .lisp-quote {
            color: brown;
        }

        .lisp-dot {
            color: brown;
        }

        .lisp-parenleft {
            color: red;
        }

        .lisp-parenright {
            color: red;
        }
    </style>
</head>
<body>";

        public string HtmlFooter { get; set; } = @"</body>
</html>
";
        private int _lineNum = 1;
        private int _lineCol = 1;
        private string _lineInput = "";

        private void WriteToken()
        {
            var pos = _scanner.GetLinePosition(_scanner.CurrentStartPos);
            var tokenPos = _scanner.GetLinePosition(_scanner.CurrentTokenStartPos);
            
            while (_lineNum < pos.Line)
            {
                FinishLine();
            }

            if (_lineCol < pos.Column)
            {
                _lineInput += " ".PadLeft(pos.Column - _lineCol);
            }

            var lines = GetLiteral().Split('\n');

            _lineInput += $"<span class='{GetClass(_scanner.CurrentToken)}'>{lines[0]}</span>";
            _lineCol += lines[0].Length;

            for (int i = 1; i < lines.Length; i++)
            {
                FinishLine();
                _lineInput += $"<span class='{GetClass(_scanner.CurrentToken)}'>{lines[i]}</span>";
                _lineCol += lines[i].Length;
            }
        }

        private void FinishLine()
        {
            _output.Write("<tr>");

            _output.Write("<td>");
            _output.Write(_lineNum);
            _output.Write("</td>");

            _output.Write("<td>");
            _output.Write("00:00:00");
            _output.Write("</td>");

            _output.Write("<td>");
            _output.Write(_lineInput);
            _output.Write("</td>");

            _output.WriteLine("</tr>");

            _lineNum++;
            _lineCol = 1;
            _lineInput = "";
        }

        private string GetClass(Token tok)
        {
            return $"lisp-code lisp-{tok.ToString().ToLower()}";
        }

        private string GetLiteral()
        {
            if (_scanner.CurrentLiteral != null)
                return _scanner.CurrentLiteral;

            switch (_scanner.CurrentToken)
            {
                case Token.Quote:
                    return "'";
                case Token.Dot:
                    return ".";
                case Token.ParenLeft:
                    return "(";
                case Token.ParenRight:
                    return ")";
            }

            throw new Exception("oh...");
        }

        public void Generate()
        {
            _output.WriteLine(HtmlHeader);
            _output.WriteLine("<table>");
            _output.WriteLine("<tr><th>#</th><th>Elapsed</th><th>Program</th></tr>");

            Token tok;
            
            while ((tok = _scanner.Scan()) != Token.EndOfFile)
            {
                WriteToken();
            }
            FinishLine();

            _output.WriteLine("</table>");

            _output.WriteLine(HtmlFooter);

            _output.Flush();
        }
    }
}
