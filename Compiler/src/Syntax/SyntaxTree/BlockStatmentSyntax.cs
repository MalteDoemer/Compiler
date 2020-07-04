using System;
using System.Linq;
using System.Collections.Immutable;
using System.Text;
using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class BlockStatment : StatementSyntax
    {
        public BlockStatment(SyntaxToken openCurly, ImmutableArray<StatementSyntax> statements, SyntaxToken closeCurly,bool isValid = true)
        {
            OpenCurly = openCurly;
            Statements = statements;
            CloseCurly = closeCurly;
            IsValid = isValid;
        }

        public SyntaxToken OpenCurly { get; }
        public ImmutableArray<StatementSyntax> Statements { get; }
        public SyntaxToken CloseCurly { get; }
        public override TextSpan Span => OpenCurly.Span + CloseCurly.Span;

        public override bool IsValid { get; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append('{');
            builder.AppendLine();
            foreach (var stmt in Statements)
            {
                builder.Append(stmt);
                builder.AppendLine();
            }
            builder.AppendLine();
            builder.Append('}');
            return builder.ToString();
        }
    }
}