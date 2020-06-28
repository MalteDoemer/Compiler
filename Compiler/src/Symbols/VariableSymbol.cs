namespace Compiler.Symbols
{
    public class VariableSymbol
    {
        public VariableSymbol(string identifier, TypeSymbol type)
        {
            Identifier = identifier;
            Type = type;
        }

        public string Identifier { get; }
        public TypeSymbol Type { get; }
    }
}