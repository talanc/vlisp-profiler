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
        private VLispPath _path;
        private TextReader _input;
        private TextWriter _output;
        private Scanner _scanner;

        public HtmlReport(string filePath, string reportPath)
        {
            _path = new VLispPath(filePath);
            _input = new StreamReader(filePath);
            _output = new StreamWriter(reportPath);
            _scanner = new Scanner(_path.FilePathContents);
            _scanner.RescanComments = false;
        }

        private int _lineNum = 1;
        private int _lineCol = 1;
        private string _lineInput = "";

        private void WriteToken()
        {
            var pos = _scanner.GetLinePosition(_scanner.CurrentTokenStartPos);

            while (_lineNum < pos.Line)
            {
                FinishLine();
            }

            if (_lineCol < pos.Column)
            {
                var spaces = " ".PadLeft(pos.Column - _lineCol);
                AddInput(spaces, "lisp-code");
            }

            var lines = GetLiteral().Split('\n');

            AddInput(lines[0], GetClass());

            for (int i = 1; i < lines.Length; i++)
            {
                FinishLine();
                AddInput(lines[i], GetClass());
            }
        }

        private void AddInput(string input, string cls)
        {
            _lineInput += $"<span class='{cls}'>{input}</span>";
            _lineCol += input.Length;
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

        private string GetClass()
        {
            if (_scanner.CurrentToken == Token.Identifier && !VLispStrings.IsPredefinedIdentifier(_scanner.CurrentLiteral))
                return "lisp-code lisp-predefined-user";
            
            return $"lisp-code lisp-{_scanner.CurrentToken.ToString().ToLower()}";
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

        public string HtmlHeader { get; set; } = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <meta http-equiv=""X-UA-Compatible"" content=""ie=edge"">
    <title>Document</title>
    <style>
        .lisp-code {
            unicode-bidi: embed;
            font-family: monospace;
            font-size: 16px;
            white-space: pre;
        }

        .lisp-comment {
            color: purple;
            background-color: lightgrey;
        }

        .lisp-identifier {
            color: blue;
        }

        .lisp-identifier-user {
            color: black;
        }

        .lisp-int {
            color: green;
        }

        .lisp-real {
            color: green;
        }

        .lisp-string {
            color: magenta;
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
    }
}
