namespace Compiler.Symbols
{
    public sealed class ParameterSymbol : VariableSymbol
    {
        public ParameterSymbol(string name, TypeSymbol type) : base(name, type)
        {
        }
    }
}