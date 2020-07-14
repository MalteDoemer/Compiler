namespace Compiler.Symbols
{
    public sealed class ParameterSymbol : VariableSymbol
    {
        public ParameterSymbol(string name, int index, TypeSymbol type) : base(name, type, VariableModifier.None)
        {
            Index = index;
        }

        public int Index { get; }
    }
}