namespace Compiler.Symbols
{
    public class VariableSymbol
    {
        public VariableSymbol(string identifier, TypeSymbol type, object value)
        {
            Identifier = identifier;
            Type = type;
            Value = value;
        }

        public string Identifier { get; }
        public TypeSymbol Type { get; }
        public object Value { get; }
    }
}