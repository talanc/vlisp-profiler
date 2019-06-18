using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VLispProfiler.Tests
{
    [TestClass]
    public class ProfilerEmitterTests
    {
        [TestMethod]
        public void TestProfilerEmitter()
        {
            // Arrange
            var profiler = new ProfilerEmitter("(list (+ 1 2))");

            // Act
            var emit = profiler.Emit();

            // Assert
            var expected = Format(@"
(prof:val (prof:in ""1"")
          (list (prof:val (prof:in ""2"")
                          (+ 1 2)
                )
          )
)
");
            Assert.AreEqual(expected, emit.Profile);
        }

        [TestMethod]
        public void TestProfilerEmitterIncludes()
        {
            // Arrange
            var profiler = MakeProfilerEmitter("(add 1 2) (sub 1 2) (+ 1 2) (ADD 1 2)");
            profiler.IncludeFilter.Add("add");

            // Act
            var emit = profiler.Emit();

            // Assert
            var expected = Format(@"
(prof:val (prof:in ""1"") (add 1 2))
(sub 1 2)
(+ 1 2)
(prof:val (prof:in ""2"") (ADD 1 2))
");
            Assert.AreEqual(expected, emit.Profile);
        }

        [TestMethod]
        public void TestProfilerEmitter_NestedIncludes()
        {
            // Arrange
            var profiler = MakeProfilerEmitter("(apply '+ (list (+ 1 1) (1+ 1) (- 2 1)))");
            profiler.ExcludeFilter.Add("apply");
            profiler.ExcludeFilter.Add("+");
            profiler.ExcludeFilter.Add("-");

            // Act
            var emit = profiler.Emit();

            // Assert
            var expected = Format(@"
(apply
  '+
  (prof:val
    (prof:in ""1"")
    (list (+ 1 1)
          (prof:val
            (prof:in ""2"")
            (1+ 1)
          )
          (- 2 1)
    )
  )
)
");
            Assert.AreEqual(expected, emit.Profile);
        }

        [TestMethod]
        public void TestProfilerEmitterDefun()
        {
            // Arrange
            var profiler = MakeProfilerEmitter("(defun a (x y / z) (setq z (+ x y)) z)");
            profiler.IncludeFilter.Add("defun");

            // Act
            var emit = profiler.Emit();

            // Assert
            var expected = Format(@"
(defun a (x y / z)
  (prof:val
    (prof:in ""1"")
    (progn (setq z (+ x y)) z)
  )
)
");
            Assert.AreEqual(expected, emit.Profile);
        }

        [TestMethod]
        public void TestProfilerEmitterCond()
        {
            // Arrange
            var profiler = MakeProfilerEmitter("(cond (nil (setq v \"is nil\") v) (t \"is t\"))");

            // Act
            var emit = profiler.Emit();

            // Assert
            var expected = Format(@"
(prof:val
  (prof:in ""1"")
  (cond
    (nil
      (prof:val
        (prof:in ""2"")
        (setq v ""is nil"")
      )
      v
    )
    (t ""is t"")
  )
)
");
            Assert.AreEqual(expected, emit.Profile);
        }

        [TestMethod]
        public void TestProfilerEmitterLambda()
        {
            // Arrange
            var profiler = MakeProfilerEmitter("((lambda (x y) (+ x y)))");
            profiler.IncludeFilter.Add("lambda");

            // Act
            var emit = profiler.Emit();

            // Assert
            var expected = Format(@"
((lambda (x y)
   (prof:val
     (prof:in ""1"")
     (+ x y)
   )
 )
)
");
            Assert.AreEqual(expected, emit.Profile);
        }

        [TestMethod]
        public void TestPredefinedSymbols()
        {
            // Arrange
            var profiler = MakeProfilerEmitter("(setq a 1)");
            profiler.AddPredefinedSymbol(1, "Load");
            profiler.AddPredefinedSymbol(2, "Run");

            // Act
            var emit = profiler.Emit();
            string[] sep = { Environment.NewLine };
            var lines = emit.Symbol.Split(sep, StringSplitOptions.None);

            // Assert
            StringAssert.StartsWith(lines[1], "1,Load");
            StringAssert.StartsWith(lines[2], "2,Run");
            StringAssert.StartsWith(lines[3], "3,Inline");
        }

        [DataTestMethod]
        [DataRow("empty-always", true, "", "")]
        [DataRow("not-here", false, "", "not-here")]
        [DataRow("not-in-include", false, "another-value-here", "")]
        public void TestIncludeExclude(string test, bool expected, string include, string exclude)
        {
            // Arrange
            var profiler = MakeProfilerEmitter("emitter");
            var incArr = include.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var excArr = exclude.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var filter in incArr) profiler.IncludeFilter.Add(filter);
            foreach (var filter in excArr) profiler.ExcludeFilter.Add(filter);

            // Act
            var actual = profiler.TestInclude(test);

            // Assert
            Assert.AreEqual(expected, actual);
        }

#region "Helpers"

        private string Format(string sourceText)
        {
            var scanner = new Scanner(sourceText);
            var parser = new Parser(scanner);
            var program = parser.GetProgram();
            var emitter = new Emitter(program, scanner);
            return emitter.Emit();
        }

        private ProfilerEmitter MakeProfilerEmitter(string sourceText)
        {
            return new ProfilerEmitter(sourceText);
        }

#endregion
    }
}
