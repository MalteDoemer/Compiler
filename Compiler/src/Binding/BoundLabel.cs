namespace Compiler.Binding
{
    public class BoundLabel
    {
        public BoundLabel(string identifier)
        {
            Identifier = identifier;
        }
        
        public string Identifier { get; }
    }
}