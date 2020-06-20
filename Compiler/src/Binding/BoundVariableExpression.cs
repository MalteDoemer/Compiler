// -1 + (4 ** 2 - 2) * -(5 / 10 -2)              = 27
//((-1) + (((4 ** 2) - 2) * (-((5 / 10) - 2))) )

/*
          +
        /  \
       *    -
     /  \   |
    -    -  1
   / \   |
  **  2  -
 /  \   / \
4    2 :  2 
      / \  
      5  10
*/


using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundVariableExpression : BoundExpression
    {
        public BoundVariableExpression(string identifier, TextSpan span, TypeSymbol resultType)
        {
            Identifier = identifier;
            Span = span;
            ResultType = resultType;
        }

        public override TypeSymbol ResultType { get; }
        public override TextSpan Span { get; }
        public string Identifier { get; }
    }
}