namespace Compiler.Symbols
{
    public abstract class VariableSymbol : Symbol
    {
        public VariableSymbol(string name, TypeSymbol type) : base(name)
        {
            Type = type;
        }

        public TypeSymbol Type { get; }
    }

    public enum VariableModifier
    {
        None,
        Constant,
    }

    public sealed class GlobalVariableSymbol : VariableSymbol
    {
        public GlobalVariableSymbol(string name, TypeSymbol type, VariableModifier modifier = VariableModifier.None) : base(name, type)
        {
            Modifier = modifier;
        }

        public VariableModifier Modifier { get; }
    }

    public sealed class LocalVariableSymbol : VariableSymbol
    {
        public LocalVariableSymbol(string name, TypeSymbol type) : base(name, type)
        {
        }
    }
}