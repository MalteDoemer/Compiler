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

            return new BoundBlockStatement(builder.ToImmutable());
        }

        public static BoundBlockStatement Lower(BoundStatement statement)
        {
            var lowerer = new Lowerer();
            var res = lowerer.RewriteStatement(statement);
            return Flatten(res);
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

            // goto continue
            // body:
            // <body>
            // continue:
            // gotoTrue <condition> body
            // break:

            var bodyLabel = CreateLabel();

            var gotoContinue = new BoundGotoStatement(node.ContinueLabel);
            var gotoBody = new BoundConditionalGotoStatement(bodyLabel, node.Condition, false);


            var continueLabelStmt = new BoundLabelStatement(node.ContinueLabel);
            var bodyLabelStmt = new BoundLabelStatement(bodyLabel);
            var breakLabelStmt = new BoundLabelStatement(node.BreakLabel);


            var res = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                gotoContinue,
                bodyLabelStmt,
                node.Body,
                continueLabelStmt,
                gotoBody,
                breakLabelStmt
            ));

            return RewriteStatement(res);
        }

        protected override BoundStatement RewriteDoWhileStatement(BoundDoWhileStatement node)
        {
            var continueLabelStmt = new BoundLabelStatement(node.ContinueLabel);
            var breakLabelStmt = new BoundLabelStatement(node.BreakLabel);

            var gotoContinue = new BoundConditionalGotoStatement(node.ContinueLabel, node.Condition, false);

            var res = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                continueLabelStmt,
                node.Body,
                gotoContinue,
                breakLabelStmt
            ));

            return RewriteStatement(res);
        }

        protected override BoundStatement RewriteForStatement(BoundForStatement node)
        {
            /*
            
            for <decl>, <condition>, <increment> {
                <body>
            }


            ----->

            {
                <decl>
                while <condition>{
                    <body>
                    continue:
                    <increment>
                }
                break:
            }


            ----->


            <decl>
            goto check
            body:
            <body>
            continue:
            <increment>
            check:
            gotoTrue body <condtiton>
            break:            
            */


            var decl = node.VariableDeclaration;
            var increment = new BoundExpressionStatement(node.Increment);
            var body = node.Body;
            var condition = node.Condition;

            var bodyLabel = CreateLabel();
            var continueLabel = node.ContinueLabel;
            var checkLabel = CreateLabel();
            var breakLabel = node.BreakLabel;

            var bodyLabelStmt = new BoundLabelStatement(bodyLabel);
            var continueLabelStmt = new BoundLabelStatement(continueLabel);
            var checkLabelStmt = new BoundLabelStatement(checkLabel);
            var breakLabelStmt = new BoundLabelStatement(breakLabel);


            var gotoCheck = new BoundGotoStatement(checkLabel);
            var gotoTrueBody = new BoundConditionalGotoStatement(bodyLabel, node.Condition, jumpIfFalse: false);

            var res = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                decl,
                gotoCheck,
                bodyLabelStmt,
                body,
                continueLabelStmt,
                increment,
                checkLabelStmt,
                gotoTrueBody,
                breakLabelStmt
            ));
            return RewriteStatement(res);
        }
    }
}