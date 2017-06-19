using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VLispProfiler
{
    public class ProfilerEmitter
    {
        private string _sourceText;
        private List<(AstExpr Expression, int Id, string SymbolType)> _symbols = new List<(AstExpr Expression, int Id, string SymbolType)>();
        private int _lastSymbolId = 0;
        
        // only include certain list items, i.e. "command" only profile command items
        public ISet<string> IncludeFilter { get; }
        public ISet<string> ExcludeFilter { get; }
        
        public ProfilerEmitter(string sourceText)
        {
            _sourceText = sourceText;
            
            IncludeFilter = new HashSet<string>(AstIdentifierNameComparer.Instance);
            ExcludeFilter = new HashSet<string>(AstIdentifierNameComparer.Instance);
        }

        public void AddPredefinedSymbol(int id, string symbolType)
        {
            AddSymbol(null, id, symbolType);
        }

        public (string Profile, string Symbol) Emit()
        {
            var profileEmit = BuildProfiler();
            var symbolEmit = BuildSymbols();

            return (profileEmit, symbolEmit);
        }

        // depends on BuildProfiler to fill out _symbols
        private string BuildSymbols()
        {
            var scanner = new Scanner(_sourceText);

            var map = new StringBuilder();

            map.Append("SymbolId,SymbolType,StartPos,EndPos");

            foreach (var symbol in _symbols)
            {
                map.AppendLine();

                var pos1 = FilePosition.Empty;
                var pos2 = FilePosition.Empty;
                if (symbol.Expression != null)
                {
                    pos1 = scanner.GetLinePosition(symbol.Expression.Pos);
                    pos2 = scanner.GetLinePosition(symbol.Expression.End);
                }
                
                var s = $"{symbol.Id},{symbol.SymbolType},{pos1},{pos2}";
                map.Append(s);
            }
            
            return map.ToString();
        }

        private string BuildProfiler()
        {
            var scanner = new Scanner(_sourceText);
            var parser = new Parser(scanner);
            var program = parser.GetProgram();

            var profile = new AstProgram();
            profile.Body = new List<AstExpr>();
            foreach (var expr in program.Body)
                profile.Body.Add(BuildExpression(expr));

            var bodyEmitter = new Emitter(profile, scanner);

            return bodyEmitter.Emit();
        }

        private AstExpr BuildExpression(AstExpr expr)
        {
            int id = -1;
            if (expr is AstList)
            {
                var list = expr as AstList;
                var newList = new AstList(list);

                if (ShouldInclude(list))
                    id = AddSymbol(expr, -1, "Inline");

                newList.Expressions = new List<AstExpr>();
                foreach (var child in list.Expressions)
                    newList.Expressions.Add(BuildExpression(child));

                if (id == -1)
                    return newList;

                var trace = NewTrace(id, newList);

                return trace;
            }

            if (expr is AstFunction)
            {
                var func = expr as AstFunction;
                var newFunc = new AstFunction(func);

                if (ShouldInclude(func))
                    id = AddSymbol(expr, -1, "Function");

                newFunc.Body = new List<AstExpr>();
                foreach (var child in func.Body)
                    newFunc.Body.Add(BuildExpression(child));

                if (id == -1)
                    return newFunc;

                AstExpr traceExpr;
                if (newFunc.Body.Count == 1)
                {
                    traceExpr = newFunc.Body[0];
                }
                else
                {
                    var progn = new AstList("progn");
                    foreach (var child in newFunc.Body)
                        progn.Expressions.Add(child);
                    traceExpr = progn;
                }

                var trace = NewTrace(id, traceExpr);

                newFunc.Body.Clear();
                newFunc.Body.Add(trace);

                return newFunc;
            }

            if (expr is AstLambda)
            {
                var lambda = expr as AstLambda;
                var newLambda = new AstLambda(lambda);

                if (ShouldInclude(lambda))
                    id = AddSymbol(expr, -1, "Function");

                newLambda.Body = new List<AstExpr>();
                foreach (var child in lambda.Body)
                    newLambda.Body.Add(BuildExpression(child));

                if (id == -1)
                    return newLambda;

                AstExpr traceExpr;
                if (newLambda.Body.Count == 1)
                {
                    traceExpr = newLambda.Body[0];
                }
                else
                {
                    var progn = new AstList("progn");
                    foreach (var child in newLambda.Body)
                        progn.Expressions.Add(child);
                    traceExpr = progn;
                }

                var trace = NewTrace(id, traceExpr);

                newLambda.Body.Clear();
                newLambda.Body.Add(trace);

                return newLambda;
            }

            if (expr is AstCond)
            {
                var cond = expr as AstCond;
                var newCond = new AstCond(cond);

                if (ShouldInclude(expr))
                    id = AddSymbol(expr, -1, "Inline");

                newCond.Conditions = new List<AstCondCondition>();
                foreach (var item in cond.Conditions)
                {
                    var cc = new AstCondCondition();

                    cc.Condition = BuildExpression(item.Condition);
                    cc.Body = new List<AstExpr>();
                    foreach (var item2 in item.Body)
                        cc.Body.Add(BuildExpression(item2));

                    newCond.Conditions.Add(cc);
                }

                if (id == -1)
                    return newCond;

                var trace = NewTrace(id, newCond);

                return trace;
            }

            return expr;
        }

        public bool TestInclude(string ident)
        {
            if (ExcludeFilter.Contains(ident))
                return false;

            if (IncludeFilter.Count == 0)
                return true;

            if (IncludeFilter.Contains(ident))
                return true;

            return false;
        }
        
        private bool ShouldInclude(AstExpr expr)
        {
            if (IncludeFilter.Count == 0)
                return true;

            if (expr is AstList)
            {
                var list = expr as AstList;
                var ident = list.Expressions.FirstOrDefault() as AstIdentifier;
                if (ident != null && TestInclude(ident.Name))
                    return true;

                return false;
            }

            if (expr is AstFunction)
            {
                return TestInclude(VLispStrings.Defun);
            }

            if (expr is AstLambda)
            {
                return TestInclude(VLispStrings.Lambda);
            }

            if (expr is AstCond)
            {
                return TestInclude(VLispStrings.Cond);
            }

            throw new ArgumentException("type must be AstList or AstFunction or AstCond", nameof(expr));
        }

        private int AddSymbol(AstExpr expr, int id, string symbolType)
        {
            if (id == -1)
                id = _lastSymbolId + 1;
            else if (id <= _lastSymbolId)
                throw new ArgumentOutOfRangeException(nameof(id), $"value must be > {_lastSymbolId}");

            _lastSymbolId = id;

            _symbols.Add((expr, id, symbolType));

            return id;
        }

        private AstList NewTrace(int symbolId, AstExpr expr)
        {
            var trace = new AstList(
                "prof:val",
                new AstList("prof:in", new AstAtom(symbolId.ToString())),
                expr
            );

            return trace;
        }

        public static readonly string[] SaneExcludes =
        {
            "+", "-", "/", "*",
            "=", "/=", "<", "<=", ">", ">=",
            "~", "1+", "1-", "boole",
            "prin1", "princ", "print"
        };
    }
}
