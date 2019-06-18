using System;
using System.Collections.Generic;
using System.Text;

namespace VLispProfiler
{
    public class Emitter
    {
        private AstProgram _program;
        private Scanner _scanner;
        private StringBuilder _sb;
        
        public Emitter(AstProgram program, Scanner scanner)
        {
            _program = program;
            _scanner = scanner;
            _sb = new StringBuilder();
        }

        public string Emit()
        {
            _sb.Clear();

            if (_program.Body.Count > 0)
            {
                EmitExpression(_program.Body[0]);
                for (var i = 1; i < _program.Body.Count; i++)
                {
                    WriteLine();
                    EmitExpression(_program.Body[i]);
                }
            }

            return _sb.ToString();
        }

        private void EmitExpression(AstExpr expr)
        {
            if (expr is AstIdentifier)
            {
                EmitIdentifier(expr as AstIdentifier);
            }
            else if (expr is AstAtom)
            {
                EmitAtom(expr as AstAtom);
            }
            else if (expr is AstSymbolExpr)
            {
                EmitSymbolExpression(expr as AstSymbolExpr);
            }
            else if (expr is AstList)
            {
                EmitList(expr as AstList);
            }
            else if (expr is AstFunction)
            {
                EmitFunction(expr as AstFunction);
            }
            else if (expr is AstLambda)
            {
                EmitLambda(expr as AstLambda);
            }
            else if (expr is AstCond)
            {
                EmitCond(expr as AstCond);
            }
            else if (expr is AstDotPair)
            {
                EmitDotPair(expr as AstDotPair);
            }
            else
            {
                throw new Exception($"Unknown expression type: {expr.GetType()}");
            }
        }

        private void EmitIdentifier(AstIdentifier ident)
        {
            Write(ident.Name);
        }

        private void EmitAtom(AstAtom atom)
        {
            Write(atom.Literal);
        }

        private void EmitSymbolExpression(AstSymbolExpr symbolExpr)
        {
            Write("'");
            EmitExpression(symbolExpr.Expression);
        }

        private void EmitList(AstList list)
        {
            Write("(");
            if (list.Expressions.Count > 0)
            {
                EmitExpression(list.Expressions[0]);
                for (var i = 1; i < list.Expressions.Count; i++)
                {
                    if (i % 3 == 0)
                        WriteLine();
                    else
                        Write(" ");
                    EmitExpression(list.Expressions[i]);
                }
            }
            Write(")");
        }

        private void EmitFunction(AstFunction function)
        {
            Write("(");
            EmitIdentifier(function.Defun);
            Write(" ");
            EmitIdentifier(function.Name);
            Write(" ");
            EmitFunctionParameters(function.Parameters);
            WriteLine();

            foreach (var expr in function.Body)
            {
                EmitExpression(expr);
                WriteLine();
            }

            Write(")");
        }

        private void EmitLambda(AstLambda lambda)
        {
            Write("(");
            EmitIdentifier(lambda.Lambda);
            Write(" ");
            EmitFunctionParameters(lambda.Parameters);
            WriteLine();

            foreach (var expr in lambda.Body)
            {
                EmitExpression(expr);
                WriteLine();
            }

            Write(")");
        }

        private void EmitFunctionParameters(AstFunctionParameters parameters)
        {
            Write("(");

            if (parameters.Parameters.Count > 0)
            {
                EmitIdentifier(parameters.Parameters[0]);
                for (var i = 1; i < parameters.Parameters.Count; i++)
                {
                    Write(" ");
                    EmitIdentifier(parameters.Parameters[i]);
                }
            }

            if (parameters.Locals.Count > 0)
            {
                Write(" ");
                Write("/");

                foreach (var local in parameters.Locals)
                {
                    Write(" ");
                    EmitIdentifier(local);
                }
            }

            Write(")");
        }

        private void EmitCond(AstCond cond)
        {
            Write("(");

            EmitIdentifier(cond.Cond);

            WriteLine();

            foreach (var item in cond.Conditions)
            {
                Write("(");

                EmitExpression(item.Condition);

                foreach (var item2 in item.Body)
                {
                    Write(" ");
                    EmitExpression(item2);
                }

                Write(")");
                WriteLine();
            }

            Write(")");
        }

        private void EmitDotPair(AstDotPair dot)
        {
            Write(VLispStrings.ParenLeft);

            EmitExpression(dot.LeftExpr);

            Write(" ");

            Write(VLispStrings.Dot);

            Write(" ");

            EmitExpression(dot.RightExpr);

            Write(VLispStrings.ParenRight);
        }

        private void Write(char input)
        {
            _sb.Append(input);
        }

        private void Write(string input)
        {
            _sb.Append(input);
        }

        private void WriteLine()
        {
            _sb.AppendLine();
        }
    }
}
