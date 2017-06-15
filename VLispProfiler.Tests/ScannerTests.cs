using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VLispProfiler.Tests
{
    [TestClass]
    public class ScannerTests
    {
        [DataTestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("\t\t")]
        [DataRow("\n\n\t   \t\n")]
        public void TestEndOfFile(string input)
        {
            // Arrange
            var scanner = MakeScanner(input);

            // Act
            var tok = scanner.Scan();

            // Assert
            Assert.AreEqual(Token.EndOfFile, tok);
        }

        [TestMethod]
        public void TestOperators()
        {
            // Arrange
            var scanner = MakeScanner("'.()");

            // Act
            var tok1 = scanner.Scan();
            var tok2 = scanner.Scan();
            var tok3 = scanner.Scan();
            var tok4 = scanner.Scan();
            var tok5 = scanner.Scan();

            // Assert
            Assert.AreEqual(Token.Quote, tok1);
            Assert.AreEqual(Token.Dot, tok2);
            Assert.AreEqual(Token.ParenLeft, tok3);
            Assert.AreEqual(Token.ParenRight, tok4);
            Assert.AreEqual(Token.EndOfFile, tok5);
        }

        [DataTestMethod]
        [DataRow(Token.Quote, " '")]
        [DataRow(Token.Quote, "    '")]
        [DataRow(Token.Quote, "  \n     \n '")]
        [DataRow(Token.EndOfFile, "     ")]
        public void TestWhitespace(Token token, string input)
        {
            // Arrange
            var scanner = MakeScanner(input);

            // Act
            var tok = scanner.Scan();

            // Assert
            Assert.AreEqual(token, tok);
        }

        [DataTestMethod]
        [DataRow(";; Hello")]
        [DataRow(";| Hello |;")]
        [DataRow(";| Hello\n\n")]
        public void TestComments(string input)
        {
            // Arrange
            var scanner = MakeScanner(input);
            scanner.RescanComments = false;

            // Act
            var scan = scanner.ScanTokLit();

            // Assert
            Assert.AreEqual(Token.Comment, scan.Token);
            Assert.AreEqual(input, scan.Literal);
        }

        [DataTestMethod]
        [DataRow("\"Hello World\"", Token.String)]  // "Hello World"
        [DataRow("\"\\\\\"", Token.String)]         // "\\"
        [DataRow("\"Hello", Token.Illegal)]         // "Hello
        [DataRow("\"", Token.Illegal)]              // "\
        [DataRow("3", Token.Int)]
        [DataRow("3.", Token.Real)]
        [DataRow("3..4", Token.Identifier)]
        [DataRow("1+", Token.Identifier)]
        public void TestLiterals(string input, Token token)
        {
            // Arrange
            var scanner = MakeScanner(input);

            // Act
            scanner.Scan();

            // Assert
            Assert.AreEqual(token, scanner.CurrentToken);
            Assert.AreEqual(input, scanner.CurrentLiteral);
        }

        [DataTestMethod]
        [DataRow("-3", Token.Int)]
        [DataRow("+3.14", Token.Real)]
        [DataRow("++3", Token.Identifier)]
        public void TestSignLiterals(string input, Token token)
        {
            // Arrange
            var scanner = MakeScanner(input);

            // Act
            var scan = scanner.ScanTokLit();

            // Assert
            Assert.AreEqual(token, scan.Token);
            Assert.AreEqual(input, scan.Literal);
        }

        [DataTestMethod]
        [DataRow("+2147483647" /* int32_max */, Token.Int, DisplayName = "(type +2147483647) returns INT")]
        [DataRow("+2147483648" /* int32_max + 1 */, Token.Real, DisplayName = "(type +2147483648) returns REAL")]
        [DataRow("-2147483648" /* int32_min */, Token.Real, DisplayName = "(type -2147483648) returns REAL")] // i don't know why, does vlisp use one's complement?
        public void TestTokenIntLimits(string input, Token expected)
        {
            // Arrange
            var scanner = MakeScanner(input);

            // Act
            var tok = scanner.Scan();

            // Assert
            Assert.AreEqual(expected, tok);
        }

        [TestMethod]
        public void TestProgram()
        {
            // Arrange
            var scanner = MakeScanner("(list 3 3.14 \"3.14\" 3..14)");

            // Act + Assert
            var scan = scanner.ScanTokLit();
            Assert.AreEqual(Token.ParenLeft, scan.Token);

            scan = scanner.ScanTokLit();
            Assert.AreEqual(Token.Identifier, scan.Token);
            Assert.AreEqual("list", scan.Literal);

            scan = scanner.ScanTokLit();
            Assert.AreEqual(Token.Int, scan.Token);
            Assert.AreEqual("3", scan.Literal);

            scan = scanner.ScanTokLit();
            Assert.AreEqual(Token.Real, scan.Token);
            Assert.AreEqual("3.14", scan.Literal);

            scan = scanner.ScanTokLit();
            Assert.AreEqual(Token.String, scan.Token);
            Assert.AreEqual("\"3.14\"", scan.Literal);

            scan = scanner.ScanTokLit();
            Assert.AreEqual(Token.Identifier, scan.Token);
            Assert.AreEqual("3..14", scan.Literal);

            scan = scanner.ScanTokLit();
            Assert.AreEqual(Token.ParenRight, scan.Token);
        }

        [TestMethod]
        public void TestCurrentTokenStartPos()
        {
            // Arrange
            var scanner = MakeScanner("a   b   c d");

            // Act + Assert
            scanner.Scan();
            Assert.AreEqual(0, scanner.CurrentTokenStartPos);

            scanner.Scan();
            Assert.AreEqual(4, scanner.CurrentTokenStartPos);

            scanner.Scan();
            Assert.AreEqual(8, scanner.CurrentTokenStartPos);

            scanner.Scan();
            Assert.AreEqual(10, scanner.CurrentTokenStartPos);
        }

        [DataTestMethod]
        [DataRow("a", 1, 1)]
        [DataRow("\na", 2, 1)]
        [DataRow("\n   \n   hello", 3, 4)]
        [DataRow("a\n\nb", 1, 1)]
        public void TestLineOffsets(string input, int expectedLine, int expectedColumn)
        {
            // Arrange
            var scanner = MakeScanner(input);

            // Act
            var tok = scanner.Scan();
            var linePos = scanner.GetLinePosition(scanner.CurrentTokenStartPos);

            // Assert
            Assert.AreEqual(expectedLine, linePos.Line);
            Assert.AreEqual(expectedColumn, linePos.Column);
        }

        [TestMethod]
        public void TestLineOffsets2()
        {
            // Arrange
            var input = @"(setq i 3)
(while (>= (setq i (1- i)) 0)
  (setq p1 (list 0 (* i 30))
	p2 (list 200 (* i 30))
	)
  (command ""pline"" p1 p2 "")
  )
";
            var scanner = MakeScanner(input);

            // Act
            var tok = scanner.Scan();
            var linePos = scanner.GetLinePosition(scanner.CurrentTokenStartPos);

            // Assert
            Assert.AreEqual(1, linePos.Line);
            Assert.AreEqual(1, linePos.Column);
        }

        [DataTestMethod]
        [DataRow("10e0", Token.Real)]
        [DataRow("5e-1", Token.Real)]
        [DataRow("5.5e2", Token.Real)]
        [DataRow("-7e-2", Token.Real)]
        [DataRow("10.e2", Token.Real)]
        [DataRow("10e5.5", Token.Identifier)]
        public void TestEpsilon(string input, Token token)
        {
            // Arrange
            var scanner = MakeScanner(input);

            // Act
            var tok = scanner.Scan();

            // Assert
            Assert.AreEqual(input, scanner.CurrentLiteral);
            Assert.AreEqual(token, tok);
        }

#region "Helpers"
        
        private Scanner MakeScanner(string sourceText)
        {
            return new Scanner(sourceText);
        }

#endregion
    }
}
