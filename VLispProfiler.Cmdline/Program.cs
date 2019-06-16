using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using VLispProfiler.Setup;
using VLispProfiler.View;

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
            if (args.Length == 0)
            {
                result = RunSetup(new SetupVerb() { Interactive = true });
            }
            else
            {
                result = CommandLine.Parser.Default
                .ParseArguments<ProfileVerb, ViewVerb, SetupVerb>(args)
                .MapResult(
                    (ProfileVerb opts) => RunProfile(opts),
                    (ViewVerb opts) => RunView(opts),
                    (SetupVerb opts) => RunSetup(opts),
                    errs => 1);
            }
#if RELEASE
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                result = 1;
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

            var symbols = new List<PredefinedSymbolItem>();
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
                    symbols.Add(new PredefinedSymbolItem(id, symbol));
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
                    profiler.AddPredefinedSymbol(sym.Id, sym.SymbolType);

                var emit = profiler.Emit();

                var paths = new VLispPath(filePath);

                File.WriteAllText(paths.SymbolPath, emit.Symbol);
                File.WriteAllText(paths.FileProfilerPath, emit.Profile);
            }

            return 0;
        }

        class PredefinedSymbolItem
        {
            public int Id { get; set; }
            public string SymbolType { get; set; }

            public PredefinedSymbolItem()
            {
            }

            public PredefinedSymbolItem(int id, string symbolType)
            {
                Id = id;
                SymbolType = symbolType;
            }
        }

        [Verb("view", HelpText = "View Profiler Results.")]
        class ViewVerb
        {
            [Value(0)]
            [Option('f', "file", Required = true, HelpText = "LISP Files")]
            public IEnumerable<string> LispFiles { get; set; }

            [Option('t', "top", HelpText = "Show top results.")]
            public int Top { get; set; }

            [Option('p', "pause-top", HelpText = "Pauses top output until a key is pressed.")]
            public bool PauseTop { get; set; }

            [Option('r', "report", HelpText = "Generate an interactive report.")]
            public string Report { get; set; }
        }

        static int RunView(ViewVerb verb)
        {
            var filePath = verb.LispFiles.First();

            if (verb.Top > 0)
            {
                var top = new TopReport(filePath, Console.Out, verb.Top);
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
                var report = new HtmlReport(filePath, verb.Report);
                report.Generate();
                Process.Start(verb.Report);
                return 0;
            }

            Console.WriteLine("no output options specified");

            return 1;
        }

        [Verb("setup", HelpText = "Setup profiler in AutoCAD.")]
        class SetupVerb
        {
            [Option('l', "list", HelpText = "List AutoCAD setups.", SetName = nameof(List))]
            public bool List { get; set; }

            [Option('i', "install", HelpText = "Install profiler script. Specify 'all' to install all setups, or specify by name, year, or profile name.", SetName = nameof(Install))]
            public IEnumerable<string> Install { get; set; }

            [Option('u', "uninstall", HelpText = "Uninstall profiler script. Specify 'all' to uninstall all setups, or specify by name, year, or profile name.", SetName = nameof(Uninstall))]
            public IEnumerable<string> Uninstall { get; set; }

            [Option("interactive", HelpText = "Interactive setup helper.")]
            public bool Interactive { get; set; }
        }

        static int RunSetup(SetupVerb verb)
        {
            var setupManager = new SetupManager(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            );
            var setups = setupManager.GetSetups();

            if (!setups.Any())
            {
                Console.WriteLine("No AutoCAD setups found. AutoCAD (Full) is required to be installed.");
                return 1;
            }

            if (verb.Interactive)
            {
                RunInteractive(setupManager, setups);
                return 0;
            }

            if (verb.List)
            {
                PrintSetups(setupManager, setups);

                return 0;
            }

            if (verb.Install.Any())
            {
                var setups2 = setupManager.FilterSetups(setups, verb.Install);

                foreach (var setup in setups2)
                {
                    setupManager.InstallSetup(setup);
                }

                return 0;
            }

            if (verb.Uninstall.Any())
            {
                var setups2 = setupManager.FilterSetups(setups, verb.Uninstall);

                foreach (var setup in setups2)
                {
                    setupManager.UninstallSetup(setup);
                }

                return 0;
            }

            Console.WriteLine("no setup options specified");

            return 1;
        }

        static void PrintSetups(SetupManager setupManager, IEnumerable<SetupInstance> setups)
        {
            Console.WriteLine("Setups:");
            var i = 1;
            foreach (var setup in setups)
            {
                var installed = setupManager.IsInstalled(setup) ? "yes" : "no";
                Console.WriteLine($"{i}) {setup.FriendNameWithProfile}, installed:{installed}");
                i++;
            }
        }

        static void RunInteractive(SetupManager setupManager, IEnumerable<SetupInstance> setups)
        {
            var sep = false;

            while (true)
            {
                // print a input seperator on 2nd+ loop
                if (sep)
                {
                    Console.WriteLine("---------------------------------");
                    Console.WriteLine();
                }
                sep = true;

                // show setups
                PrintSetups(setupManager, setups);
                Console.WriteLine();

                // prompt for action
                Console.WriteLine("Actions: install (i), uninstall (u), exit (x)");
                Console.Write("Action? ");
                var input = Console.ReadLine().Trim();

                // action: exit
                if (input.StartsWith("x", StringComparison.InvariantCultureIgnoreCase) ||
                    input.StartsWith("ex", StringComparison.InvariantCultureIgnoreCase))
                {
                    break;
                }

                // action: install / uninstall
                string actionStr;
                Action<SetupInstance> actionFunc;
                if (input.StartsWith("i", StringComparison.InvariantCultureIgnoreCase))
                {
                    actionStr = "Install";
                    actionFunc = setupManager.InstallSetup;
                }
                else if (input.StartsWith("u", StringComparison.InvariantCultureIgnoreCase))
                {
                    actionStr = "Uninstall";
                    actionFunc = setupManager.UninstallSetup;
                }
                else
                {
                    // action: ???
                    Console.WriteLine($"Unknown action: {input}");
                    continue;
                }

                // print setups and prompt which to install/uninstall
                PrintSetups(setupManager, setups);
                Console.Write($"{actionStr} which setup (0 to go back, 'all' to select all)? ");
                var input2 = Console.ReadLine().Trim();

                // go back
                if (input2 == "0")
                {
                    continue;
                }

                // find setup(s) specified
                IEnumerable<SetupInstance> setupsSpecified;
                if (input2.StartsWith("a", StringComparison.InvariantCultureIgnoreCase))
                {
                    // all
                    setupsSpecified = setups;
                }
                else if (int.TryParse(input2, out var result))
                {
                    var resultIdx = result - 1;
                    setupsSpecified = setups.Where((_, i) => i == resultIdx);
                }
                else
                {
                    Console.WriteLine("Please enter a number or specify 'all'.");
                    continue;
                }

                // specified setup(s) not found
                if (!setupsSpecified.Any())
                {
                    Console.WriteLine($"No setup(s) found for '{input2}'");
                    continue;
                }

                // invoke setup action
                foreach (var setup in setupsSpecified)
                {
                    Console.WriteLine($"{actionStr}ing {setup.FriendNameWithProfile}...");
                    actionFunc.Invoke(setup);
                    Console.WriteLine("Done.");
                }
            }
        }
    }
}
