using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Compiler.Binding;

namespace Compiler.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private int labelCount;

        private Lowerer()
        {

        }

        private LabelSymbol CreateLabel() => new LabelSymbol($"Label{labelCount}");

        private static BoundBlockStatement Flatten(BoundStatement statement)
        {
            var builder = ImmutableArray.CreateBuilder<BoundStatement>();
            var stack = new Stack<BoundStatement>();
            stack.Push(statement);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current is BoundBlockStatement block)
                {
                    foreach (var s in block.Statements.Reverse())
                        stack.Push(s);
                }
                else 
                {
                    builder.Add(current);
                }
            }

            return new BoundBlockStatement(builder.ToImmutable());
        }


        public static BoundBlockStatement Lower(BoundStatement statement)
        {
            var lowerer = new Lowerer();
            var res = lowerer.RewriteStatement(statement);
            return Flatten(res);
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

        protected override BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            if (node.ElseStatement == null)
            {
                var endLabel = CreateLabel();
                var gotoIfFalse = new BoundConditionalGotoStatement(endLabel, node.Condition, true);
                var endLabelStmt = new BoundLabelStatement(endLabel);
                var res = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(gotoIfFalse, node.Body, endLabelStmt));
                return RewriteStatement(res);
            }
            else
            {
                var elseLabel = CreateLabel();
                var endLabel = CreateLabel();

                var gotoIfFalse = new BoundConditionalGotoStatement(elseLabel, node.Condition, true);
                var gotoEnd = new BoundGotoStatement(endLabel);

                var elseLabelStmt = new BoundLabelStatement(elseLabel);
                var endLabelStmt = new BoundLabelStatement(endLabel);

                var res = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                    gotoIfFalse,
                    node.Body,
                    gotoEnd,
                    elseLabelStmt,
                    node.ElseStatement,
                    endLabelStmt
                ));
                return RewriteStatement(res);
            }
        }

        protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            var continueLabel = CreateLabel();
            var checkLabel = CreateLabel();
            var endLabel = CreateLabel();

            var gotoCheck = new BoundGotoStatement(checkLabel);
            var gotoContinue = new BoundConditionalGotoStatement(continueLabel, node.Condition, false);

            var continueLabelStmt = new BoundLabelStatement(continueLabel);
            var checkLabelStmt = new BoundLabelStatement(checkLabel);
            var endLabelStmt = new BoundLabelStatement(endLabel);


            var res = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                    gotoCheck,
                    continueLabelStmt,
                    node.Body,
                    checkLabelStmt,
                    gotoContinue,
                    endLabelStmt
            ));

            return RewriteStatement(res);
        }
    }
}