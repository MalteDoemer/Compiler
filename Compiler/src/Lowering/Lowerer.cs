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

        private BoundLabel CreateLabel() => new BoundLabel($"Label{++labelCount}");

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

            return new BoundBlockStatement(builder.ToImmutable(), true);
        }

        public static BoundBlockStatement Lower(BoundStatement statement)
        {
            var lowerer = new Lowerer();
            var res = lowerer.RewriteStatement(statement);
            return Flatten(res);
        }


        protected override BoundStatement RewriteDoWhileStatement(BoundDoWhileStatement node)
        {
            var startLabel = CreateLabel();

            var boundStartLabel = new BoundLabelStatement(startLabel, true);
            var gotoStart = new BoundConditionalGotoStatement(startLabel, node.Condition, jumpIfFalse: false, isValid: true);

            var res = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                boundStartLabel,
                node.Body,
                gotoStart
            ), isValid: true);

            return RewriteStatement(res);
        }

        protected override BoundStatement RewriteForStatement(BoundForStatement node)
        {
            var decl = node.VariableDeclaration;
            var inc = new BoundExpressionStatement(node.Increment, true);
            var body = node.Body;
            var condition = node.Condition;

            var whileBody = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(body, inc), true);
            var whileLoop = new BoundWhileStatement(condition, whileBody, true);

            var res = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(decl, whileLoop), true);
            return RewriteStatement(res);
        }

        protected override BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            if (node.ElseStatement == null)
            {
                var endLabel = CreateLabel();
                var gotoIfFalse = new BoundConditionalGotoStatement(endLabel, node.Condition, jumpIfFalse: true, isValid: true);
                var endLabelStmt = new BoundLabelStatement(endLabel, true);
                var res = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(gotoIfFalse, node.Body, endLabelStmt), true);
                return RewriteStatement(res);
            }
            else
            {
                var elseLabel = CreateLabel();
                var endLabel = CreateLabel();

                var gotoIfFalse = new BoundConditionalGotoStatement(elseLabel, node.Condition, jumpIfFalse: true, isValid: true);
                var gotoEnd = new BoundGotoStatement(endLabel, true);

                var elseLabelStmt = new BoundLabelStatement(elseLabel, true);
                var endLabelStmt = new BoundLabelStatement(endLabel, true);

                var res = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                    gotoIfFalse,
                    node.Body,
                    gotoEnd,
                    elseLabelStmt,
                    node.ElseStatement,
                    endLabelStmt
                ), isValid: true);
                return RewriteStatement(res);
            }
        }

        protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            var continueLabel = CreateLabel();
            var checkLabel = CreateLabel();
            var endLabel = CreateLabel();

            var gotoCheck = new BoundGotoStatement(checkLabel, true);
            var gotoContinue = new BoundConditionalGotoStatement(continueLabel, node.Condition, jumpIfFalse: false, isValid: true);

            var continueLabelStmt = new BoundLabelStatement(continueLabel, true);
            var checkLabelStmt = new BoundLabelStatement(checkLabel, true);
            var endLabelStmt = new BoundLabelStatement(endLabel, true);


            var res = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                gotoCheck,
                continueLabelStmt,
                node.Body,
                checkLabelStmt,
                gotoContinue,
                endLabelStmt
            ), isValid: true);

            return RewriteStatement(res);
        }
    }
}