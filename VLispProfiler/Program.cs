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
            var result = CommandLine.Parser.Default.ParseArguments<ProfileVerb, ViewVerb>(args).MapResult(
                (ProfileVerb opts) => RunProfile(opts),
                (ViewVerb opts) => RunView(opts),
                errs => 1);

            return result;

            //var options = new Options();
            //var isValid = CommandLine.Parser.Default.ParseArguments<Options>(args);

            //if (isValid.Tag == ParserResultType.NotParsed)
            //    Environment.Exit(1);

            //string lispPath;

            //if (args[0] == "--prompt")
            //{
            //    Console.Write("lisp file? ");
            //    lispPath = Console.ReadLine();
            //}
            //else
            //{
            //    lispPath = args[0];
            //}

            ////if (!File.Exists(lispPath))
            //    //ExitUsage("could not find file");

            //var lispText = File.ReadAllText(lispPath);
            //var profiler = new ProfilerEmitter(lispText);
            //var emit = profiler.Emit();

            //var symbolPath = $"{lispPath}.symbols.txt";
            //File.WriteAllText(symbolPath, emit.Symbol);

            //var profilerPath = $"{lispPath}.profiler.lsp";
            //File.WriteAllText(profilerPath, emit.Profile);
        }

        static int RunProfile(ProfileVerb verb)
        {
            return 1;
        }

        static int RunView(ViewVerb verb)
        {
            return 1;
        }

        [Verb("profile", HelpText = "Generate Profile LISP file and Symbols file.")]
        class ProfileVerb
        {
            [Option('f', "file", SetName = "File", HelpText = "LISP File")]
            public string LispFile { get; set; }

            [Option(SetName = "Prompt")]
            public bool Prompt { get; set; }

            [Option('i', "include", HelpText = "Includes (i.e. specify 'command' to only profile command calls)")]
            public IEnumerable<string> Includes { get; set; }

            [Option('s', "symbol", HelpText = "Specify a pre-defined symbol as ID:Type (i.e. 1:Load)")]
            public IEnumerable<string> PredefinedSymbols { get; set; }
        }

        [Verb("view", HelpText = "View Profiler Results.")]
        class ViewVerb
        {
            [Value(0)]
            [Option('f', "file", Required = true, HelpText = "LISP File")]
            public string LispFile { get; set; }
        }
    }
}
