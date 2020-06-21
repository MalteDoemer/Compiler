namespace Compiler.Binding
{
    public class VariableSymbol
    {
        public VariableSymbol(string identifier, TypeSymbol type, dynamic value)
        {
            Identifier = identifier;
            Type = type;
            Value = value;
        }

        public string Identifier { get; }
        public TypeSymbol Type { get; }
        public dynamic Value { get; }
    }
}