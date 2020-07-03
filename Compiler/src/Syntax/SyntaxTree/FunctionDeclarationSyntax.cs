using Compiler.Text;

namespace Compiler.Syntax
{
    internal sealed class FunctionDeclarationSyntax : MemberSyntax
    {
        public FunctionDeclarationSyntax(SyntaxToken typeKeyword, SyntaxToken identifier, SyntaxToken leftParenthesis, SeperatedSyntaxList<ParameterSyntax> parameters, SyntaxToken rightParenthesis, BlockStatment body)
        {
            TypeKeyword = typeKeyword;
            Identifier = identifier;
            LeftParenthesis = leftParenthesis;
            Parameters = parameters;
            RightParenthesis = rightParenthesis;
            Body = body;
        }

        public override TextSpan Span => TypeKeyword.Span + Body.Span;

        public override bool IsValid => TypeKeyword.IsValid &&
                                        Identifier.IsValid &&
                                        LeftParenthesis.IsValid &&
                                        Parameters.IsValid &&
                                        RightParenthesis.IsValid &&
                                        Body.IsValid;

        public SyntaxToken TypeKeyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken LeftParenthesis { get; }
        public SeperatedSyntaxList<ParameterSyntax> Parameters { get; }
        public SyntaxToken RightParenthesis { get; }
        public BlockStatment Body { get; }

        public override string ToString() => $"{TypeKeyword.Value} {Identifier.Value}({Parameters})\n{Body}";
    }

}