using System;
using System.IO;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;

namespace VLispProfiler
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
                result = CommandLine.Parser.Default.ParseArguments<ProfileVerb, ViewVerb>(args).MapResult(
                    (ProfileVerb opts) => RunProfile(opts),
                    (ViewVerb opts) => RunView(opts),
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

            [Option('s', "symbol", HelpText = "Specify a pre-defined symbol as ID:Type (i.e. 1:Load)")]
            public IEnumerable<string> PredefinedSymbols { get; set; }
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
            if (err != 0)
                return err;

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
                var fileText = File.ReadAllText(filePath);

                var profiler = new ProfilerEmitter(fileText);

                foreach (var inc in verb.Includes)
                    profiler.IncludeFilter.Add(inc);

                foreach (var sym in symbols)
                    profiler.AddPredefinedSymbol(sym.Item1, sym.Item2);

                var emit = profiler.Emit();

                var symbolPath = $"{filePath}.symbols.txt";
                File.WriteAllText(symbolPath, emit.Symbol);

                var profilerPath = $"{filePath}.prof.lsp";
                File.WriteAllText(profilerPath, emit.Profile);
            }

            return 0;
        }

        [Verb("view", HelpText = "View Profiler Results.")]
        class ViewVerb
        {
            [Value(0)]
            [Option('f', "file", Required = true, HelpText = "LISP Files")]
            public IEnumerable<string> LispFiles { get; set; }
        }

        static int RunView(ViewVerb verb)
        {
            Console.WriteLine("not implemented yet");

            return 1;
        }
    }
}
