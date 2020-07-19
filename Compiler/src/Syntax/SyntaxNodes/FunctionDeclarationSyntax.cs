using System.Collections.Generic;
using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class FunctionDeclarationSyntax : MemberSyntax
    {
        public FunctionDeclarationSyntax(SyntaxToken functionKeyword, SyntaxToken identifier, SyntaxToken leftParenthesis, SeperatedSyntaxList<ParameterSyntax> parameters, SyntaxToken rightParenthesis, TypeClauseSyntax returnType, BlockStatmentSyntax body, bool isValid, TextLocation location) : base(isValid, location)
        {
            FunctionKeyword = functionKeyword;
            Identifier = identifier;
            LeftParenthesis = leftParenthesis;
            Parameters = parameters;
            RightParenthesis = rightParenthesis;
            ReturnType = returnType;
            Body = body;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.FunctionDeclarationSyntax;
        public SyntaxToken FunctionKeyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken LeftParenthesis { get; }
        public TypeClauseSyntax ReturnType { get; }
        public SeperatedSyntaxList<ParameterSyntax> Parameters { get; }
        public SyntaxToken RightParenthesis { get; }
        public BlockStatmentSyntax Body { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return FunctionKeyword;
            yield return Identifier;
            yield return LeftParenthesis;
            if (ReturnType.IsExplicit)
                yield return ReturnType;
            foreach (var param in Parameters)
                yield return param;
            yield return RightParenthesis;
            yield return Body;
        }
    }
}