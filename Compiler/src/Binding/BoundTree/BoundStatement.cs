namespace Compiler.Binding
{
    internal abstract class BoundStatement : BoundNode
    {
        protected BoundStatement(bool isValid) : base(isValid)
        {
        }
    }
}