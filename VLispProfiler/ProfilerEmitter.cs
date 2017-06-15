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
        
        // only include certain list items, i.e. "command" only profile command items
        public ISet<string> IncludeFilter { get; }

        //public ISet<string> ExcludeFilter { get; }

        public ProfilerEmitter(string sourceText)
        {
            _sourceText = sourceText;
            
            IncludeFilter = new HashSet<string>(AstIdentifierNameComparer.Instance);
            //ExcludeFilter = new HashSet<string>(AstIdentifierNameComparer.Instance);
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

            map.Append("SymbolId,SymbolType,StartLine,StartCol,EndLine,EndCol");

            foreach (var symbol in _symbols)
            {
                map.AppendLine();

                var pos1 = scanner.GetLinePosition(symbol.Expression.Pos);
                var pos2 = scanner.GetLinePosition(symbol.Expression.End);
                var s = $"{symbol.Id},{symbol.SymbolType},{pos1.Line},{pos1.Column},{pos2.Line},{pos2.Column}";
                map.Append(s);
            }
            
            return map.ToString();
        }

        private string BuildProfiler()
        {
            var scanner = new Scanner(_sourceText);
            var parser = new Parser(scanner);
            var program = parser.GetProgram();

            _symbols.Clear();

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
                    id = NewSymbol(expr, "Inline");

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
                    id = NewSymbol(expr, "Function");

                newFunc.Body = new List<AstExpr>();
                foreach (var child in func.Body)
                    newFunc.Body.Add(BuildExpression(child));

                if (id == -1)
                    return newFunc;

                var progn = new AstList("progn");
                foreach (var child in newFunc.Body)
                    progn.Expressions.Add(child);

                var trace = NewTrace(id, progn);

                newFunc.Body.Clear();
                newFunc.Body.Add(trace);

                return newFunc;
            }

            if (expr is AstCond)
            {
                var cond = expr as AstCond;
                var newCond = new AstCond(cond);

                if (ShouldInclude(expr))
                    id = NewSymbol(expr, "Inline");

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
        
        private bool ShouldInclude(AstExpr expr)
        {
            if (IncludeFilter.Count == 0)
                return true;

            if (expr is AstList)
            {
                var list = expr as AstList;
                var ident = list.Expressions.FirstOrDefault() as AstIdentifier;
                if (ident != null && IncludeFilter.Contains(ident.Name))
                    return true;

                return false;
            }

            if (expr is AstFunction)
            {
                return IncludeFilter.Contains(VLispStrings.Defun);
            }

            if (expr is AstCond)
            {
                return IncludeFilter.Contains(VLispStrings.Cond);
            }

            throw new ArgumentException("type must be AstList or AstFunction or AstCond", nameof(expr));
        }

        private int NewSymbol(AstExpr expr, string type)
        {
            var id = _symbols.Count + 101;
            _symbols.Add((expr, id, type));
            return id;
        }

        private AstList NewTrace(int symbolId, AstExpr expr)
        {
            var trace = new AstList(
                "progn",
                new AstList("prof:in", new AstAtom(symbolId.ToString())),
                new AstList("prof:out", expr)
            );

            return trace;
        }
    }
}
