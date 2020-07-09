namespace Compiler.Symbols
{
    public sealed class GlobalVariableSymbol : VariableSymbol
    {
        public GlobalVariableSymbol(string name, TypeSymbol type, VariableModifier modifier = VariableModifier.None) : base(name, type, modifier)
        {
        }
    }
}