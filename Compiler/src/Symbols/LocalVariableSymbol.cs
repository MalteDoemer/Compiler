namespace Compiler.Symbols
{
    public sealed class LocalVariableSymbol : VariableSymbol
    {
        public LocalVariableSymbol(string name, TypeSymbol type, VariableModifier modifier = VariableModifier.None) : base(name, type, modifier)
        {
        }
    }
}