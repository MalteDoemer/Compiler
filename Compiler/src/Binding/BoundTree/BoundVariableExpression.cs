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
        public BoundVariableExpression(VariableSymbol variable, TextSpan span)
        {
            Variable = variable;
            Span = span;
        }

        public VariableSymbol Variable { get; }
        public override TextSpan Span { get; }
        public override TypeSymbol ResultType => Variable.Type;
    }
}