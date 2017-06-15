using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VLispProfiler.Tests
{
    [TestClass]
    public class EmitterTests
    {
        [DataTestMethod]
        [DataRow("list defun  oops")]
        [DataRow("3 3.14 \"3.14\"")]
        [DataRow("'list ''oops")]
        [DataRow("(list 3 3.14 \"3.14\")")]
        [DataRow("(defun a () 1)")]
        [DataRow("(lambda () 1)")]
        public void TestEmitter(string input)
        {
            // Arrange
            var emitter = MakeEmitter(input);

            // Act
            var emit = emitter.Emit();

            // Assert
            Assert.AreEqual(Format(input), emit);
        }

#region "Helpers"

        private Emitter MakeEmitter(string sourceText)
        {
            var scanner = new Scanner(sourceText);
            var parser = new Parser(scanner);
            var program = parser.GetProgram();
            var emitter = new Emitter(program, scanner);
            return emitter;
        }
        
        private string Format(string sourceText)
        {
            var scanner = new Scanner(sourceText);
            var parser = new Parser(scanner);
            var program = parser.GetProgram();
            var emitter = new Emitter(program, scanner);
            return emitter.Emit();
        }

        #endregion
    }

}
