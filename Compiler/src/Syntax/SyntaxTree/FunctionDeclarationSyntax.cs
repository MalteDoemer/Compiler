using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class FunctionDeclarationSyntax : MemberSyntax
    {
        public FunctionDeclarationSyntax(SyntaxToken functionKeyword, SyntaxToken identifier, SyntaxToken leftParenthesis, SeperatedSyntaxList<ParameterSyntax> parameters, SyntaxToken rightParenthesis, TypeClauseSyntax returnType, BlockStatmentSyntax body, bool isValid, TextLocation location)
        {
            FunctionKeyword = functionKeyword;
            Identifier = identifier;
            LeftParenthesis = leftParenthesis;
            Parameters = parameters;
            RightParenthesis = rightParenthesis;
            ReturnType = returnType;
            Body = body;
            IsValid = isValid;
            Location = location;
        }
        public override SyntaxNodeKind Kind => SyntaxNodeKind.FunctionDeclarationSyntax;
        public override TextLocation Location { get; }
        public override bool IsValid { get; }
        public SyntaxToken FunctionKeyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken LeftParenthesis { get; }
        public TypeClauseSyntax ReturnType { get; }
        public SeperatedSyntaxList<ParameterSyntax> Parameters { get; }
        public SyntaxToken RightParenthesis { get; }
        public BlockStatmentSyntax Body { get; }

        public override string ToString() => $"{FunctionKeyword.Value} {Identifier.Value}({Parameters}){ReturnType}\n{Body}";
    }

}