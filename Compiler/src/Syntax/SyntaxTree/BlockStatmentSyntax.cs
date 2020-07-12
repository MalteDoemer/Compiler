using System;
using System.Linq;
using System.Collections.Immutable;
using System.Text;
using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class BlockStatmentSyntax : StatementSyntax
    {
        public BlockStatmentSyntax(SyntaxToken openCurly, ImmutableArray<StatementSyntax> statements, SyntaxToken closeCurly, bool isValid = true)
        {
            OpenCurly = openCurly;
            Statements = statements;
            CloseCurly = closeCurly;
            IsValid = isValid;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.BlockStatmentSyntax;
        public override TextSpan Location => OpenCurly.Span + CloseCurly.Span;
        public override bool IsValid { get; }
        public SyntaxToken OpenCurly { get; }
        public ImmutableArray<StatementSyntax> Statements { get; }
        public SyntaxToken CloseCurly { get; }

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