using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VLispProfiler
{
    public class AstProgram
    {
        public IList<AstExpr> Body { get; set; }
    }

    public abstract class AstExpr
    {
        public abstract int Pos { get; }
        public abstract int End { get; }
    }

    public class AstSymbolExpr : AstExpr
    {
        public int QuotePos { get; set; }
        public AstExpr Expression { get; set; }

        public override int Pos => QuotePos;
        public override int End => Expression.End;
    }

    public class AstList : AstExpr
    {
        public int ParenLeftPos { get; set; }
        public int ParenRightPos { get; set; }

        public IList<AstExpr> Expressions { get; set; }

        public override int Pos => ParenLeftPos;
        public override int End => ParenRightPos + 1;

        public AstList() { }

        public AstList(AstList list)
        {
            ParenLeftPos = list.ParenLeftPos;
            ParenRightPos = list.ParenRightPos;
            Expressions = list.Expressions;
        }

        public AstList(string name, params AstExpr[] content)
        {
            Expressions = new List<AstExpr>();
            Expressions.Add(new AstIdentifier(name));
            foreach (var expr in content)
                Expressions.Add(expr);
        }
    }

    public class AstDotPair : AstExpr
    {
        public int ParenLeftPos { get; set; }
        public int ParenRightPos { get; set; }
        public int DotPos { get; set; }

        public AstExpr LeftExpr { get; set; }
        public AstExpr RightExpr { get; set; }

        public override int Pos => ParenLeftPos;
        public override int End => ParenRightPos;

        public AstDotPair() { }
    }

    public class AstCond : AstExpr
    {
        public int ParenLeftPos { get; set; }
        public int ParenRightPos { get; set; }

        public AstIdentifier Cond { get; set; }
        public IList<AstCondCondition> Conditions { get; set; }

        public override int Pos => ParenLeftPos;
        public override int End => ParenRightPos + 1;

        public AstCond() { }

        public AstCond(AstCond cond)
        {
            ParenLeftPos = cond.ParenLeftPos;
            ParenRightPos = cond.ParenRightPos;

            Cond = cond.Cond;
            Conditions = cond.Conditions;
        }
    }

    public class AstCondCondition : AstExpr
    {
        public int ParenLeftPos { get; set; }
        public int ParenRightPos { get; set; }

        public AstExpr Condition { get; set; }
        public IList<AstExpr> Body { get; set; }

        public override int Pos => ParenLeftPos;
        public override int End => ParenRightPos + 1;

        public AstCondCondition() { }
    }

    public class AstFunction : AstExpr
    {
        public int ParenLeftPos { get; set; }
        public int ParenRightPos { get; set; }

        public AstIdentifier Defun { get; set; }
        public AstIdentifier Name { get; set; }
        public AstFunctionParameters Parameters { get; set; }
        public IList<AstExpr> Body { get; set; }

        public override int Pos => ParenLeftPos;
        public override int End => ParenRightPos + 1;

        public AstFunction() { }

        public AstFunction(AstFunction func)
        {
            ParenLeftPos = func.ParenLeftPos;
            ParenRightPos = func.ParenRightPos;

            Defun = func.Defun;
            Name = func.Name;
            Parameters = func.Parameters;
            Body = func.Body;
        }
    }

    public class AstLambda : AstExpr
    {
        public int ParenLeftPos { get; set; }
        public int ParenRightPos { get; set; }

        public AstIdentifier Lambda { get; set; }
        public AstFunctionParameters Parameters { get; set; }
        public IList<AstExpr> Body { get; set; }

        public override int Pos => ParenLeftPos;
        public override int End => ParenRightPos + 1;
    }

    public class AstFunctionParameters : AstExpr
    {
        public int NilPos { get; set; }         // -1 when IsNil=false

        public int ParenLeftPos { get; set; }   // -1 when IsNil=true
        public int ParenRightPos { get; set; }  // -1 when IsNil=true
        public int LocalSlashPos { get; set; }  // -1 when IsNil=true

        public bool IsNil { get; set; } // is 'nil' instead of parens
        public bool HasLocalSlash { get; set; }

        public IList<AstIdentifier> Parameters { get; set; }
        public IList<AstIdentifier> Locals { get; set; }

        public override int Pos => (IsNil ? NilPos : ParenLeftPos);
        public override int End => (IsNil ? NilPos + "nil".Length : ParenRightPos + 1);
    }

    public class AstAtom : AstExpr
    {
        public const int IntMin = int.MinValue + 1;
        public const int IntMax = int.MaxValue;

        public int AtomPos { get; set; }
        public Token AtomType { get; set; }
        public string Literal { get; set; }

        public override int Pos => AtomPos;
        public override int End => AtomPos + Literal.Length;

        public AstAtom() { }

        public AstAtom(int i)
        {
            if (i < IntMin || i > IntMax)
                AtomType = Token.Real;
            else
                AtomType = Token.Int;

            Literal = i.ToString();
        }

        public AstAtom(float f)
        {
            AtomType = Token.Real;
            Literal = f.ToString();
        }

        public AstAtom(string s)
        {
            AtomType = Token.String;
            Literal = '"' + s + '"';
        }
    }

    public class AstIdentifier : AstExpr
    {
        public int IdentifierPos { get; set; }
        public string Name { get; set; }

        public override int Pos => IdentifierPos;
        public override int End => IdentifierPos + Name.Length;

        public AstIdentifier() { }

        public AstIdentifier(string name)
        {
            Name = name;
        }

        public bool Equals(string name)
        {
            return AstIdentifierNameComparer.Instance.Equals(Name, name);
        }
    }

    public class AstIdentifierNameComparer : IEqualityComparer<string>
    {
        public static readonly AstIdentifierNameComparer Instance = new AstIdentifierNameComparer();

        public bool Equals(string x, string y)
        {
            return x.ToUpperInvariant().Equals(y.ToUpperInvariant());
        }

        public int GetHashCode(string obj)
        {
            return obj.ToUpperInvariant().GetHashCode();
        }
    }
}
