using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VLispProfiler
{
    public class Parser
    {
        private Scanner _scanner;

        public Parser(Scanner scanner)
        {
            _scanner = scanner;
            NextToken();
        }

        public AstProgram GetProgram()
        {
            var expressions = new List<AstExpr>();
            while (true)
            {
                if (_scanner.CurrentToken == Token.EndOfFile)
                    break;

                var expr = GetExpression();
                if (expr == null)
                    ThrowParserException("Expecting expression");

                expressions.Add(expr);
            }

            return new AstProgram
            {
                Body = expressions
            };
        }

        private void NextToken()
        {
            _scanner.Scan();
        }

        private AstExpr GetExpression(bool isSymbol = false)
        {
            AstExpr expr;

            if (_scanner.CurrentToken == Token.EndOfFile)
                ThrowParserException("Unexpected end of file");

            if (isSymbol)
            {
                expr = GetDotPair();
                if (expr != null) return expr;
            }

            expr = GetSymbolExpression();
            if (expr != null) return expr;

            expr = GetCond();
            if (expr != null) return expr;

            expr = GetFunction();
            if (expr != null) return expr;

            expr = GetLambda();
            if (expr != null) return expr;

            expr = GetList(isSymbol);
            if (expr != null) return expr;

            expr = GetIdentifier();
            if (expr != null) return expr;

            expr = GetAtom();
            if (expr != null) return expr;

            return null;
        }

        private AstDotPair GetDotPair()
        {
            if (_scanner.CurrentToken != Token.ParenLeft)
                return null;

            var lparen = _scanner.CurrentTokenStartPos;

            using (var stack = _scanner.PushStack())
            {
                NextToken(); // (

                var lexpr = GetExpression(true);
                if (lexpr == null)
                    return null;

                if (_scanner.CurrentToken != Token.Dot)
                    return null;

                var dot = _scanner.CurrentTokenStartPos;
                NextToken(); // .

                stack.Commit();
                
                var rexpr = GetExpression(true);
                if (rexpr == null)
                    ThrowParserException("expecting expression");

                if (_scanner.CurrentToken != Token.ParenRight)
                    ThrowParserException("expecting ')'");

                var rparen = _scanner.CurrentTokenStartPos;
                NextToken(); // )

                return new AstDotPair
                {
                    ParenLeftPos = lparen,
                    ParenRightPos = rparen,
                    DotPos = dot,
                    LeftExpr = lexpr,
                    RightExpr = rexpr
                };
            }
        }

        private AstSymbolExpr GetSymbolExpression()
        {
            if (_scanner.CurrentToken != Token.Quote)
                return null;

            var pos = _scanner.CurrentTokenStartPos;

            NextToken();

            var expr = GetExpression(true);
            if (expr == null)
                ThrowParserException("Expecting expression");

            return new AstSymbolExpr
            {
                Expression = expr,
                QuotePos = pos
            };
        }

        private AstCond GetCond()
        {
            if (_scanner.CurrentToken == Token.ParenLeft)
            {
                var lparen = _scanner.CurrentTokenStartPos;

                if (_scanner.ScanIf(Token.Identifier, VLispStrings.Cond))
                {
                    var cond = GetIdentifier();

                    var conditions = GetCondConditions();

                    var rparen = _scanner.CurrentTokenStartPos;
                    NextToken();

                    return new AstCond
                    {
                        ParenLeftPos = lparen,
                        ParenRightPos = rparen,
                        Cond = cond,
                        Conditions = conditions
                    };
                }
            }

            return null;
        }

        private IList<AstCondCondition> GetCondConditions()
        {
            var conditions = new List<AstCondCondition>();

            while (_scanner.CurrentToken != Token.ParenRight)
            {
                var list = GetList(false);
                if (list == null || list.Expressions.Count == 0)
                    ThrowParserException("expecting Condition");

                var condition = new AstCondCondition()
                {
                    ParenLeftPos = list.ParenLeftPos,
                    ParenRightPos = list.ParenRightPos,
                    Condition = list.Expressions.First(),
                    Body = list.Expressions.Skip(1).ToList()
                };
                conditions.Add(condition);
            }

            return conditions;
        }

        private AstFunction GetFunction()
        {
            if (_scanner.CurrentToken != Token.ParenLeft)
                return null;

            var lparen = _scanner.CurrentTokenStartPos;

            if (!_scanner.ScanIf(Token.Identifier, VLispStrings.Defun))
                return null;

            var defun = GetIdentifier();

            var name = GetIdentifier();
            if (name == null)
                ThrowParserException("Expecting identifier");

            var fp = GetFunctionParams_Strict();

            var fb = GetFunctionBody_Strict();

            var rparen = _scanner.CurrentTokenStartPos;
            NextToken(); // )

            return new AstFunction
            {
                Defun = defun,
                Name = name,
                Parameters = fp,
                Body = fb,
                ParenLeftPos = lparen,
                ParenRightPos = rparen,
            };
        }

        private AstLambda GetLambda()
        {
            if (_scanner.CurrentToken != Token.ParenLeft)
                return null;

            var lparen = _scanner.CurrentTokenStartPos;

            if (!_scanner.ScanIf(Token.Identifier, VLispStrings.Lambda))
                return null;

            var lambda = GetIdentifier();

            var fp = GetFunctionParams_Strict();

            var fb = GetFunctionBody_Strict();

            var rparen = _scanner.CurrentTokenStartPos;
            NextToken(); // )

            return new AstLambda
            {
                Lambda = lambda,
                Parameters = fp,
                Body = fb,
                ParenLeftPos = lparen,
                ParenRightPos = rparen,
            };
        }

        private AstFunctionParameters GetFunctionParams_Strict()
        {
            if (_scanner.CurrentToken == Token.Identifier && _scanner.CurrentLiteral == "nil")
            {
                var nilPos = _scanner.CurrentTokenStartPos;

                NextToken();

                return new AstFunctionParameters
                {
                    IsNil = true,
                    NilPos = nilPos,
                    ParenLeftPos = -1,
                    ParenRightPos = -1,
                    LocalSlashPos = -1,
                    Parameters = Array.Empty<AstIdentifier>(),
                    Locals = Array.Empty<AstIdentifier>()
                };
            }

            if (_scanner.CurrentToken != Token.ParenLeft)
                ThrowParserException("Expecting '(' or 'nil'");

            var lparen = _scanner.CurrentTokenStartPos;
            var rparen = -1;
            var localslash = -1;

            NextToken();

            var parameters = new List<AstIdentifier>();
            var locals = new List<AstIdentifier>();

            var curr = parameters;

            while (true)
            {
                if (_scanner.CurrentToken == Token.ParenRight)
                {
                    rparen = _scanner.CurrentTokenStartPos;
                    NextToken();
                    break;
                }

                var ident = GetIdentifier();
                if (ident == null)
                    ThrowParserException("Expecting identifier");

                if (ident.Name == "/")
                {
                    localslash = ident.Pos;
                    if (curr == locals)
                        ThrowParserException("Unexpected identifier '/'");
                    curr = locals;
                    continue;
                }

                curr.Add(ident);
            }

            return new AstFunctionParameters
            {
                IsNil = false,
                HasLocalSlash = (localslash != -1),
                NilPos = -1,
                ParenLeftPos = lparen,
                ParenRightPos = rparen,
                LocalSlashPos = localslash,
                Parameters = parameters,
                Locals = locals
            };
        }

        private List<AstExpr> GetFunctionBody_Strict()
        {
            var expressions = new List<AstExpr>();

            var expr = GetExpression();
            if (expr == null)
                ThrowParserException("Expecting Expression");

            expressions.Add(expr);

            while (_scanner.CurrentToken != Token.ParenRight)
            {
                expr = GetExpression();
                expressions.Add(expr);
            }

            return expressions;
        }

        private AstList GetList(bool isSymbol)
        {
            if (_scanner.CurrentToken != Token.ParenLeft)
                return null;

            var lparen = _scanner.CurrentTokenStartPos;
            var rparen = -1;

            NextToken();

            var expressions = new List<AstExpr>();
            while (true)
            {
                if (_scanner.CurrentToken == Token.ParenRight)
                {
                    rparen = _scanner.CurrentTokenStartPos;
                    NextToken();
                    break;
                }

                var expr = GetExpression(isSymbol);
                if (expr == null)
                    ThrowParserException("Expecting Expression or ')'");

                expressions.Add(expr);
            }

            return new AstList
            {
                Expressions = expressions,
                ParenLeftPos = lparen,
                ParenRightPos = rparen
            };
        }

        private AstIdentifier GetIdentifier()
        {
            if (_scanner.CurrentToken == Token.Identifier)
            {
                var ident = new AstIdentifier
                {
                    Name = _scanner.CurrentLiteral,
                    IdentifierPos = _scanner.CurrentTokenStartPos
                };
                NextToken();
                return ident;
            }
            return null;
        }

        private AstAtom GetAtom()
        {
            if (_scanner.CurrentToken == Token.Int || _scanner.CurrentToken == Token.Real || _scanner.CurrentToken == Token.String)
            {
                var atom = new AstAtom
                {
                    AtomType = _scanner.CurrentToken,
                    Literal = _scanner.CurrentLiteral,
                    AtomPos = _scanner.CurrentTokenStartPos
                };
                NextToken();
                return atom;
            }
            return null;
        }

        private void ThrowParserException(string message)
        {
            var pos = _scanner.GetLinePosition(_scanner.CurrentStartPos);
            throw new ParserException($"{pos}: {message}");
        }
    }
}
