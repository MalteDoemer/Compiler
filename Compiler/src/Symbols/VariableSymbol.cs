namespace Compiler.Symbols
{
    public abstract class VariableSymbol : Symbol
    {
        public VariableSymbol(string name, TypeSymbol type, VariableModifier modifiers) : base(name)
        {
            Type = type;
            Modifiers = modifiers;
        }

        public TypeSymbol Type { get; }
        public VariableModifier Modifiers { get; }
    }

    public enum VariableModifier
    {
        None,
        Constant,
    }

    public sealed class GlobalVariableSymbol : VariableSymbol
    {
        public GlobalVariableSymbol(string name, TypeSymbol type, VariableModifier modifier = VariableModifier.None) : base(name, type, modifier)
        {
        }
    }

    public sealed class LocalVariableSymbol : VariableSymbol
    {
        public LocalVariableSymbol(string name, TypeSymbol type, VariableModifier modifier = VariableModifier.None) : base(name, type, modifier)
        {
        }
    }
}