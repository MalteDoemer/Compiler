namespace Compiler.Symbols
{

    public sealed class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol Int = new TypeSymbol("Int");
        public static readonly TypeSymbol Float = new TypeSymbol("Float");
        public static readonly TypeSymbol Bool = new TypeSymbol("Bool");
        public static readonly TypeSymbol String = new TypeSymbol("String");
        public static readonly TypeSymbol ErrorType = new TypeSymbol("Error-Type");

        private TypeSymbol(string name) : base(name)
        {

        }
    }
}