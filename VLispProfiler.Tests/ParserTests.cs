using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace VLispProfiler.Tests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void TestAtoms()
        {
            // Arrange
            var parser = MakeParser("3 3.14 \"3.14\"  ");

            // Act
            var program = parser.GetProgram();

            // Assert
            Assert.AreEqual(3, program.Body.Count);
            var atom1 = program.Body[0] as AstAtom;
            var atom2 = program.Body[1] as AstAtom;
            var atom3 = program.Body[2] as AstAtom;

            Assert.AreEqual(Token.Int, atom1.AtomType);
            Assert.AreEqual("3", atom1.Literal);
            Assert.AreEqual(0, atom1.Pos);
            Assert.AreEqual(1, atom1.End);

            Assert.AreEqual(Token.Real, atom2.AtomType);
            Assert.AreEqual("3.14", atom2.Literal);
            Assert.AreEqual(2, atom2.Pos);
            Assert.AreEqual(6, atom2.End);

            Assert.AreEqual(Token.String, atom3.AtomType);
            Assert.AreEqual("\"3.14\"", atom3.Literal);
            Assert.AreEqual(7, atom3.Pos);
            Assert.AreEqual(13, atom3.End);
        }

        [TestMethod]
        public void TestIdentifier()
        {
            // Arrange
            var parser = MakeParser("  3..14");

            // Act
            var program = parser.GetProgram();

            // Assert
            Assert.AreEqual(1, program.Body.Count);
            var ident1 = program.Body[0] as AstIdentifier;

            Assert.AreEqual("3..14", ident1.Name);
            Assert.AreEqual(2, ident1.Pos);
            Assert.AreEqual(7, ident1.End);
        }

        [TestMethod]
        public void TestList()
        {
            // Arrange
            var parser = MakeParser("(list 3 3.14 \"3.14\")");

            // Act
            var program = parser.GetProgram();

            // Assert
            Assert.AreEqual(1, program.Body.Count);
            Assert.IsInstanceOfType(program.Body[0], typeof(AstList));
            var list = program.Body[0] as AstList;
            Assert.AreEqual(4, list.Expressions.Count);
            Assert.AreEqual(0, list.Pos);
            Assert.AreEqual(20, list.End);
            var list0 = list.Expressions[0] as AstIdentifier;
            Assert.AreEqual("list", list0.Name);
            var list1 = list.Expressions[1] as AstAtom;
            Assert.AreEqual(Token.Int, list1.AtomType);
            var list2 = list.Expressions[2] as AstAtom;
            Assert.AreEqual(Token.Real, list2.AtomType);
            var list3 = list.Expressions[3] as AstAtom;
            Assert.AreEqual(Token.String, list3.AtomType);
        }

        [TestMethod]
        public void TestSymbolExpr()
        {
            // Arrange
            var parser = MakeParser("'a");

            // Act
            var program = parser.GetProgram();

            // Assert
            Assert.AreEqual(1, program.Body.Count);
            var symbolExpr = program.Body[0] as AstSymbolExpr;
            Assert.AreEqual(0, symbolExpr.Pos);
            Assert.AreEqual(2, symbolExpr.End);

            var ident = symbolExpr.Expression as AstIdentifier;
            Assert.AreEqual("a", ident.Name);
        }

        [TestMethod]
        public void TestFunction()
        {
            // Arrange
            var parser = MakeParser("(defun a (/) 1)");

            // Act
            var program = parser.GetProgram();

            // Assert
            Assert.AreEqual(1, program.Body.Count);
            Assert.IsInstanceOfType(program.Body[0], typeof(AstFunction));
            var function = program.Body[0] as AstFunction;
            Assert.AreEqual("a", function.Name.Name);
            Assert.AreEqual(7, function.Name.Pos);
            Assert.IsFalse(function.Parameters.IsNil);
            Assert.IsTrue(function.Parameters.HasLocalSlash);
            Assert.AreEqual(0, function.Parameters.Parameters.Count);
            Assert.AreEqual(0, function.Parameters.Locals.Count);
            Assert.AreEqual(9, function.Parameters.ParenLeftPos);
            Assert.AreEqual(10, function.Parameters.LocalSlashPos);
            Assert.AreEqual(11, function.Parameters.ParenRightPos);
            Assert.AreEqual(1, function.Body.Count);
            Assert.AreEqual("1", (function.Body[0] as AstAtom).Literal);
            Assert.AreEqual(0, function.Pos);
            Assert.AreEqual(1, function.Defun.Pos);
            Assert.AreEqual(15, function.End);
        }

        [TestMethod]
        public void TestFunction2()
        {
            // Arrange
            var parser = MakeParser("( defun a nil 1)");

            // Act
            var program = parser.GetProgram();

            // Assert
            Assert.AreEqual(1, program.Body.Count);
            Assert.IsInstanceOfType(program.Body[0], typeof(AstFunction));
            var function = program.Body[0] as AstFunction;
            Assert.AreEqual("a", function.Name.Name);
            Assert.IsTrue(function.Parameters.IsNil);
            Assert.AreEqual(10, function.Parameters.NilPos);
            Assert.AreEqual(0, function.Parameters.Parameters.Count);
            Assert.AreEqual(0, function.Parameters.Locals.Count);
            Assert.AreEqual(1, function.Body.Count);
            Assert.AreEqual("1", (function.Body[0] as AstAtom).Literal);
            Assert.AreEqual(0, function.Pos);
            Assert.AreEqual(2, function.Defun.Pos);
            Assert.AreEqual(16, function.End);
        }

        [TestMethod]
        public void TestFunction3()
        {
            // Arrange
            var parser = MakeParser("(defun a (x y / z) (setq z (+ x y)) z)");

            // Act
            var program = parser.GetProgram();

            // Assert
            Assert.AreEqual(1, program.Body.Count);
            Assert.IsInstanceOfType(program.Body[0], typeof(AstFunction));
            var function = program.Body[0] as AstFunction;

            Assert.AreEqual("a", function.Name.Name);
            Assert.AreEqual(2, function.Parameters.Parameters.Count);
            Assert.AreEqual(1, function.Parameters.Locals.Count);

            Assert.AreEqual(2, function.Body.Count);
            Assert.IsInstanceOfType(function.Body[0], typeof(AstList));
            var body1 = function.Body[0] as AstList;
            Assert.AreEqual("setq", (body1.Expressions[0] as AstIdentifier).Name);

            Assert.IsInstanceOfType(function.Body[1], typeof(AstIdentifier));
            var body2 = function.Body[1] as AstIdentifier;
            Assert.AreEqual("z", body2.Name);
        }

        [TestMethod]
        public void TestLambda()
        {
            // Arrange
            var parser = MakeParser("(lambda (a b / c) (setq c (+ a b)))");

            // Act
            var program = parser.GetProgram();

            // Assert
            Assert.AreEqual(1, program.Body.Count);
            Assert.IsInstanceOfType(program.Body[0], typeof(AstLambda));
            var lambda = program.Body[0] as AstLambda;
            Assert.IsFalse(lambda.Parameters.IsNil);
            Assert.AreEqual(2, lambda.Parameters.Parameters.Count);
            Assert.AreEqual("a", lambda.Parameters.Parameters[0].Name);
            Assert.AreEqual("b", lambda.Parameters.Parameters[1].Name);
            Assert.IsTrue(lambda.Parameters.HasLocalSlash);
            Assert.AreEqual(13, lambda.Parameters.LocalSlashPos);
            Assert.AreEqual(1, lambda.Parameters.Locals.Count);
            Assert.AreEqual("c", lambda.Parameters.Locals[0].Name);
            Assert.AreEqual(1, lambda.Body.Count);
            Assert.AreEqual(0, lambda.Pos);
            Assert.AreEqual(35, lambda.End);
            Assert.AreEqual(1, lambda.Lambda.Pos);
        }

        [TestMethod]
        public void TestComments()
        {
            // Arrange
            var parser = MakeParser("(list 1 ;| oops |; 2 3)");

            // Act
            var program = parser.GetProgram();

            // Assert
            Assert.AreEqual(1, program.Body.Count);
            Assert.IsInstanceOfType(program.Body[0], typeof(AstList));
            var list = program.Body[0] as AstList;
            Assert.AreEqual(4, list.Expressions.Count);
        }

        [DataTestMethod]
        [DataRow("(cond)", 0)]
        [DataRow("(cond (t))", 1)]
        [DataRow("(cond (nil 1) (t 2))", 2)]
        public void TestCond(string input, int conditions)
        {
            // Arrange
            var parser = MakeParser(input);

            // Act
            var program = parser.GetProgram();

            // Assert
            Assert.AreEqual(1, program.Body.Count);
            Assert.IsInstanceOfType(program.Body[0], typeof(AstCond));
            var cond = program.Body[0] as AstCond;
            Assert.AreEqual(conditions, cond.Conditions.Count);
        }

        [DataTestMethod]
        [DataRow("'(1 . 2)", 1)]
        [DataRow("'((1 . 2) 3 4 5)", 1)]
        [DataRow("'((1 . 2) (3 . 4))", 2)]
        public void TestDotPairs(string input, int numPairs)
        {
            // Arrange
            var parser = MakeParser(input);

            // Act
            var program = parser.GetProgram();
            var actualNumPairs = 0;
            foreach (var item in program.Body)
            {
                Visit(item, expr =>
                {
                    if (expr is AstDotPair) actualNumPairs++;
                });
            }

            // Assert
            Assert.AreEqual(numPairs, actualNumPairs);
        }

#region "Helpers"

        private void Visit(AstExpr expr, Action<AstExpr> action)
        {
            action(expr);
            if (expr is AstList)
            {
                foreach (var item in (expr as AstList).Expressions)
                {
                    Visit(item, action);
                }
            }
            if (expr is AstSymbolExpr)
            {
                Visit((expr as AstSymbolExpr).Expression, action);
            }
            if (expr is AstDotPair)
            {
                var pair = expr as AstDotPair;
                Visit(pair.LeftExpr, action);
                Visit(pair.RightExpr, action);
            }
        }
        
        private Parser MakeParser(string sourceText)
        {
            var scanner = new Scanner(sourceText);
            var parser = new Parser(scanner);
            return parser;
        }

#endregion
    }
}
