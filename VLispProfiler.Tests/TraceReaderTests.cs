using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VLispProfiler.View;

namespace VLispProfiler.Tests
{
    [TestClass]
    public class TraceReaderTests
    {
        [TestMethod]
        public void TestGetTrace()
        {
            // Arrange
            var reader = new TraceReader(@"
1,In,00:00:00
1,Out,00:00:01
2,In,00:00:01
2,Out,00:00:03
");

            // Act
            var root = reader.GetTrace();

            // Assert
            Assert.AreEqual(2, root.Items.Count);
            Assert.AreEqual(1, root.Items.ElementAt(0).Id);
            Assert.AreEqual(new TimeSpan(0, 0, 0), root.Items.ElementAt(0).InElapsed);
            Assert.AreEqual(new TimeSpan(0, 0, 1), root.Items.ElementAt(0).OutElapsed);
            Assert.AreEqual(new TimeSpan(0, 0, 1), root.Items.ElementAt(0).Elapsed);
            Assert.AreEqual(2, root.Items.ElementAt(1).Id);
            Assert.AreEqual(new TimeSpan(0, 0, 1), root.Items.ElementAt(1).InElapsed);
            Assert.AreEqual(new TimeSpan(0, 0, 3), root.Items.ElementAt(1).OutElapsed);
            Assert.AreEqual(new TimeSpan(0, 0, 2), root.Items.ElementAt(1).Elapsed);
        }

        [TestMethod]
        public void TestChildren()
        {
            // Arrange
            var reader = new TraceReader(@"
1,In,00:00:00
2,In,00:00:00
2,Out,00:00:01
3,In,00:00:01
3,Out,00:00:02
1,Out,00:00:03
");

            // Act
            var root = reader.GetTrace();

            // Assert
            Assert.AreEqual(1, root.Items.Count());
            Assert.AreEqual(1, root.Items.ElementAt(0).Id);
            Assert.AreEqual(2, root.Items.ElementAt(0).Items.Count());
            Assert.AreEqual(2, root.Items.ElementAt(0).Items.ElementAt(0).Id);
            Assert.AreEqual(3, root.Items.ElementAt(0).Items.ElementAt(1).Id);
        }

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
