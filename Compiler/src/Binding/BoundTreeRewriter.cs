using System;
using System.Collections.Immutable;

namespace Compiler.Binding
{
    internal abstract class BoundTreeRewriter
    {
        public virtual BoundStatement RewriteStatement(BoundStatement statement)
        {
            if (statement is BoundInvalidStatement)
                return statement;
            else if (statement is BoundExpressionStatement es)
                return RewriteExpressionStatement(es);
            else if (statement is BoundBlockStatement bs)
                return RewriteBlockStatement(bs);
            else if (statement is BoundForStatement fs)
                return RewriteForStatement(fs);
            else if (statement is BoundIfStatement ifs)
                return RewriteIfStatement(ifs);
            else if (statement is BoundWhileStatement ws)
                return RewriteWhileStatement(ws);
            else if (statement is BoundDoWhileStatement dws)
                return RewriteDoWhileStatement(dws);
            else if (statement is BoundVariableDeclarationStatement vs)
                return RewriteVariableDeclaration(vs);
            else if (statement is BoundGotoStatement gs)
                return RewriteGotoStatement(gs);
            else if (statement is BoundConditionalGotoStatement gcs)
                return RewriteConditionalGotoStatement(gcs);
            else if (statement is BoundLabelStatement ls)
                return RewriteLabelStatement(ls);

            else throw new Exception($"Unknown BoundStatement <{statement}>");
        }

        protected virtual BoundStatement RewriteConditionalGotoStatement(BoundConditionalGotoStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            if (condition == node.Condition)
                return node;

            return new BoundConditionalGotoStatement(node.Label, condition, node.JumpIfFalse);
        }

        protected virtual BoundStatement RewriteVariableDeclaration(BoundVariableDeclarationStatement node)
        {
            var expression = RewriteExpression(node.Expression);
            if (expression == node.Expression)
                return node;

            return new BoundVariableDeclarationStatement(node.Variable, expression);
        }

        protected virtual BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            var body = RewriteStatement(node.Body);
            if (condition == node.Condition && body == node.Body)
                return node;
            return new BoundWhileStatement(condition, body, node.BreakLabel, node.ContinueLabel);
        }

        protected virtual BoundStatement RewriteDoWhileStatement(BoundDoWhileStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            var body = RewriteStatement(node.Body);
            if (condition == node.Condition && body == node.Body)
                return node;
            return new BoundWhileStatement(condition, body, node.BreakLabel, node.ContinueLabel);
        }

        protected virtual BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            var body = RewriteStatement(node.Body);
            var elseStatement = node.ElseStatement == null ? null : RewriteStatement(node.ElseStatement);
            if (condition == node.Condition && body == node.Body && elseStatement == node.ElseStatement)
                return node;
            return new BoundIfStatement(condition, body, elseStatement);
        }

        protected virtual BoundStatement RewriteForStatement(BoundForStatement node)
        {
            var variableDecl = RewriteStatement(node.VariableDeclaration);
            var condition = RewriteExpression(node.Condition);
            var increment = RewriteExpression(node.Increment);
            var body = RewriteStatement(node.Body);

            if (variableDecl == node.VariableDeclaration && condition == node.Condition && increment == node.Increment && body == node.Body)
                return node;

            return new BoundForStatement(variableDecl, condition, increment, body, node.BreakLabel, node.ContinueLabel);
        }

        protected virtual BoundStatement RewriteBlockStatement(BoundBlockStatement node)
        {
            ImmutableArray<BoundStatement>.Builder builder = null;

            for (var i = 0; i < node.Statements.Length; i++)
            {
                var oldStatement = node.Statements[i];
                var newStatement = RewriteStatement(oldStatement);
                if (newStatement != oldStatement)
                {
                    if (builder == null)
                    {
                        builder = ImmutableArray.CreateBuilder<BoundStatement>(node.Statements.Length);

                        for (var j = 0; j < i; j++)
                            builder.Add(node.Statements[j]);
                    }
                }

                if (builder != null)
                    builder.Add(newStatement);
            }

            if (builder == null)
                return node;

            return new BoundBlockStatement(builder.MoveToImmutable());
        }

        protected virtual BoundStatement RewriteExpressionStatement(BoundExpressionStatement node)
        {
            var expr = RewriteExpression(node.Expression);
            if (expr == node.Expression)
                return node;
            return new BoundExpressionStatement(expr);
        }

        public virtual BoundExpression RewriteExpression(BoundExpression expression)
        {
            if (expression is BoundInvalidExpression)
                return expression;
            else if (expression is BoundLiteralExpression le)
                return RewriteLiteralExpression(le);
            else if (expression is BoundUnaryExpression ue)
                return RewriteUnaryExpression(ue);
            else if (expression is BoundBinaryExpression be)
                return RewriteBinaryExpression(be);
            else if (expression is BoundVariableExpression ve)
                return RewriteVaraibleExpression(ve);
            else if (expression is BoundAssignementExpression ae)
                return RewriteAssignmentExpression(ae);
            else if (expression is BoundCallExpression bc)
                return RewriteCallExpression(bc);
            else if (expression is BoundConversionExpression cc)
                return RewriteConversionExpression(cc);
            else throw new Exception($"Unknown BoundExpression <{expression}>");

        }

        protected virtual BoundExpression RewriteConversionExpression(BoundConversionExpression node)
        {
            var expr = RewriteExpression(node.Expression);
            if (expr == node.Expression)
                return node;
            return new BoundConversionExpression(node.Type, expr);
        }

        protected virtual BoundExpression RewriteCallExpression(BoundCallExpression node)
        {
            ImmutableArray<BoundExpression>.Builder builder = null;

            for (var i = 0; i < node.Arguments.Length; i++)
            {
                var oldStatement = node.Arguments[i];
                var newStatement = RewriteExpression(oldStatement);
                if (newStatement != oldStatement)
                {
                    if (builder == null)
                    {
                        builder = ImmutableArray.CreateBuilder<BoundExpression>(node.Arguments.Length);

                        for (var j = 0; j < i; j++)
                            builder.Add(node.Arguments[j]);
                    }
                }

                if (builder != null)
                    builder.Add(newStatement);
            }

            if (builder == null)
                return node;

            return new BoundCallExpression(node.Symbol, builder.MoveToImmutable());
        }

        protected virtual BoundExpression RewriteAssignmentExpression(BoundAssignementExpression node)
        {
            var expr = RewriteExpression(node.Expression);
            if (expr == node.Expression)
                return node;
            return new BoundAssignementExpression(node.Variable, expr);
        }

        protected virtual BoundExpression RewriteBinaryExpression(BoundBinaryExpression node)
        {
            var left = RewriteExpression(node.Left);
            var right = RewriteExpression(node.Right);

            if (left == node.Left && right == node.Right)
                return node;

            return new BoundBinaryExpression(node.Op, left, right, node.ResultType);
        }

        protected virtual BoundExpression RewriteUnaryExpression(BoundUnaryExpression node)
        {
            var right = RewriteExpression(node.Right);
            if (right == node.Right)
                return node;
            return new BoundUnaryExpression(node.Op, right, node.ResultType);
        }

        protected virtual BoundStatement RewriteLabelStatement(BoundLabelStatement node) => node;

        protected virtual BoundStatement RewriteGotoStatement(BoundGotoStatement node) => node;

        protected virtual BoundExpression RewriteVaraibleExpression(BoundVariableExpression node) => node;

        protected virtual BoundExpression RewriteLiteralExpression(BoundLiteralExpression node) => node;
    }
}