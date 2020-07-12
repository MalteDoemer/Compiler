﻿using System.Text;
using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class VariableDeclarationStatementSyntax : StatementSyntax
    {
        public VariableDeclarationStatementSyntax(SyntaxToken varKeyword, SyntaxToken identifier, TypeClauseSyntax typeClause, SyntaxToken equalToken, ExpressionSyntax expression, bool isValid, TextLocation location)
        {
            VarKeyword = varKeyword;
            Identifier = identifier;
            TypeClause = typeClause;
            EqualToken = equalToken;
            Expression = expression;
            IsValid = isValid;
            Location = location;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.VariableDeclarationStatementSyntax;
        public override TextLocation Location { get; }
        public override bool IsValid { get; }
        public SyntaxToken VarKeyword { get; }
        public SyntaxToken Identifier { get; }
        public TypeClauseSyntax TypeClause { get; }
        public SyntaxToken EqualToken { get; }
        public ExpressionSyntax Expression { get; }

        public override string ToString() => $"{VarKeyword.Value} {Identifier.Value}{TypeClause} = {Expression}";
    }
}