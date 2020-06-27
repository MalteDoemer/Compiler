using System.Collections.Immutable;
using Compiler.Binding;

namespace Compiler.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private Lowerer()
        {

        }

        public static BoundStatement Lower(BoundStatement statement)
        {
            var lowerer = new Lowerer();
            return lowerer.RewriteStatement(statement);
        }

        protected override BoundStatement RewriteForStatement(BoundForStatement node)
        {
            var decl = node.VariableDecleration;
            var inc = new BoundExpressionStatement(node.Increment);
            var body = node.Body;
            var condition = node.Condition;

            var whileBody = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(body, inc));
            var whileLoop = new BoundWhileStatement(condition, whileBody);

            var res = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(decl, whileLoop));
            return RewriteStatement(res);
        }
   }
}