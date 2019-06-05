using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using CommandLine;

namespace VLispProfiler.Cmdline
{
    class Program
    {
        static int Main(string[] args)
        {
            int result = 0;

#if RELEASE
            try
            {
#endif
            result = CommandLine.Parser.Default
                .ParseArguments<ProfileVerb, ViewVerb, SetupVerb>(args)
                .MapResult(
                    (ProfileVerb opts) => RunProfile(opts),
                    (ViewVerb opts) => RunView(opts),
                    (SetupVerb opts) => RunSetup(opts),
                    errs => 1);
#if RELEASE
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 1;
            }
#endif

            return result;
        }

        [Verb("profile", HelpText = "Generate Profile LISP file and Symbols file.")]
        class ProfileVerb
        {
            [Option('f', "file", Required = true, HelpText = "LISP Files")]
            public IEnumerable<string> LispFiles { get; set; }

            [Option('i', "include", HelpText = "Includes (i.e. specify 'command' to only profile command calls)")]
            public IEnumerable<string> Includes { get; set; }

            [Option('e', "exclude", HelpText = "Excludes (i.e. specify 'command' to ignore command calls")]
            public IEnumerable<string> Excludes { get; set; }

            [Option('s', "symbol", HelpText = "Specify a pre-defined symbol as ID:Type (i.e. 1:Load)")]
            public IEnumerable<string> PredefinedSymbols { get; set; }

            [Option("no-sane-excludes", HelpText = "Disables sane excludes (arithmetic, logical, display functions)")]
            public bool NoSaneExcludes { get; set; }
        }

        static int RunProfile(ProfileVerb verb)
        {
            var err = 0;

            foreach (var file in verb.LispFiles)
            {
                if (!File.Exists(file))
                {
                    Console.WriteLine($"err: could not find file '{file}'");
                    err = 1;
                }
            }

            var symbols = new List<(int, string)>();
            foreach (var sym in verb.PredefinedSymbols)
            {
                var i = sym.IndexOf(':');
                if (i == -1)
                {
                    Console.WriteLine($"err: invalid predefined symbol '{sym}'");
                    err = 1;
                }
                else
                {
                    var id = int.Parse(sym.Substring(0, i));
                    var symbol = sym.Substring(i + 1);
                    symbols.Add((id, symbol));
                }
            }

            if (err != 0)
                return err;

            foreach (var filePath in verb.LispFiles)
            {
                var vlispPath = new VLispPath(filePath);

                var fileText = vlispPath.FilePathContents;

                var profiler = new ProfilerEmitter(fileText);

                foreach (var inc in verb.Includes)
                    profiler.IncludeFilter.Add(inc);

                foreach (var exc in verb.Excludes)
                    profiler.ExcludeFilter.Add(exc);

                if (!verb.NoSaneExcludes)
                {
                    foreach (var exc in ProfilerEmitter.SaneExcludes)
                        profiler.ExcludeFilter.Add(exc);
                }

                foreach (var sym in symbols)
                    profiler.AddPredefinedSymbol(sym.Item1, sym.Item2);

                var emit = profiler.Emit();

                var paths = new VLispPath(filePath);

                File.WriteAllText(paths.SymbolPath, emit.Symbol);
                File.WriteAllText(paths.FileProfilerPath, emit.Profile);
            }

            return 0;
        }

        [Verb("view", HelpText = "View Profiler Results.")]
        class ViewVerb
        {
            [Value(0)]
            [Option('f', "file", Required = true, HelpText = "LISP Files")]
            public IEnumerable<string> LispFiles { get; set; }

            [Option('t', "top", HelpText = "Show top (N) results")]
            public int Top { get; set; }

            [Option('p', "pause-top", HelpText = "Pauses top output until a key is pressed")]
            public bool PauseTop { get; set; }

            [Option('r', "report", HelpText = "Generate an interactive report")]
            public string Report { get; set; }
        }

        static int RunView(ViewVerb verb)
        {
            var filePath = verb.LispFiles.First();

            if (verb.Top > 0)
            {
                var top = new View.Top(filePath, Console.Out, verb.Top);
                top.Display();

                if (verb.PauseTop)
                {
                    Console.WriteLine();
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
                return 0;
            }

            if (!string.IsNullOrEmpty(verb.Report))
            {
                var report = new View.HtmlReport(filePath, verb.Report);
                report.Generate();
                System.Diagnostics.Process.Start(verb.Report);
                return 0;
            }

            Console.WriteLine("no output options specified");

            return 1;
        }

        [Verb("setup", HelpText = "Setup profiler in AutoCAD.")]
        class SetupVerb
        {
            [Option("interactive", HelpText = "Interactive mode.", SetName = nameof(Interactive))]
            public bool Interactive { get; set; }

            [Option('l', "list", HelpText = "List setups.", SetName = nameof(List))]
            public bool List { get; set; }

            [Option('i', "install", HelpText = "Install profiler script.", SetName = nameof(Install))]
            public IEnumerable<string> Install { get; set; }

            [Option('u', "uninstall", HelpText = "Uninstall profiler script.", SetName = nameof(Uninstall))]
            public IEnumerable<string> Uninstall { get; set; }
        }

        static int RunSetup(SetupVerb verb)
        {
            Console.WriteLine($"Interactive = {verb.Interactive}");
            Console.WriteLine($"List = {verb.List}");
            Console.WriteLine($"Install = {string.Join(" + ", verb.Install)}");
            Console.WriteLine($"Uninstall = {string.Join(" + ", verb.Uninstall)}");

            return 0;
        }
    }
}
