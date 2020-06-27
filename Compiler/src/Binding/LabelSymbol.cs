namespace Compiler.Binding
{
    public class LabelSymbol
    {
        public LabelSymbol(string identifier)
        {
            Identifier = identifier;
        }
        
        public string Identifier { get; }
    }
}