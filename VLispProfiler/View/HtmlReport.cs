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
        private TextReader _input;
        private TextWriter _output;

        public HtmlReport(string filePath, string reportPath)
        {
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
</head>
<body>";

        public string HtmlFooter { get; set; } = @"</body>
</html>
";

        public void Generate()
        {
            _output.WriteLine(HtmlHeader);

            var i = 0;
            string line;

            _output.WriteLine("<table>");
            _output.WriteLine("<tr><th>#</th><th>Elapsed</th><th>Program</th></tr>");

            while ((line = _input.ReadLine()) != null)
            {
                i++;

                _output.Write("<tr>");

                _output.Write("<td>");
                _output.Write(i);
                _output.Write("</td>");

                _output.Write("<td>");
                _output.Write("00:00:00");
                _output.Write("</td>");

                _output.Write("<td style='font-family: Lucida Console; white-space: pre;'>");
                _output.Write(line);
                _output.Write("</td>");

                _output.WriteLine("</tr>");
            }

            _output.WriteLine("</table>");

            _output.WriteLine(HtmlFooter);

            _output.Flush();
        }
    }
}
