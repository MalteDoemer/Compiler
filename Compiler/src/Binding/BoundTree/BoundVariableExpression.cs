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


using Compiler.Symbols;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class BoundVariableExpression : BoundExpression
    {
        public BoundVariableExpression(VariableSymbol variable, bool isValid)
        {
            Variable = variable;
            IsValid = isValid;
        }

        public VariableSymbol Variable { get; }
        public override TypeSymbol ResultType => Variable.Type;

        public override bool IsValid { get; }
    }
}