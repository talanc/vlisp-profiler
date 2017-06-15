﻿using System;
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
            Assert.AreEqual(Format("(progn (prof:in \"101\") (prof:out (list (progn (prof:in \"102\") (prof:out (+ 1 2))))))"), emit.Profile);
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
(progn (prof:in ""101"") (prof:out (add 1 2)))
(sub 1 2)
(+ 1 2)
(progn (prof:in ""102"") (prof:out (ADD 1 2)))
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
  (progn 
    (prof:in ""101"")
    (prof:out (progn (setq z (+ x y)) z))
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
(progn
  (prof:in ""101"")
  (prof:out (cond
              (nil
                (progn 
                  (prof:in ""102"")
                  (prof:out (setq v ""is nil""))
                )
                v
              )
              (t ""is t"")
            )
  )
)
");
            Assert.AreEqual(expected, emit.Profile);
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