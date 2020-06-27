


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
            else if (statement is BoundPrintStatement ps)
                return RewritePrintStatement(ps);
            else if (statement is BoundVariableDecleration vs)
                return RewriteVariableDecleration(vs);
            else throw new Exception($"Unknown BoundStatement <{statement}>");
        }

        private BoundStatement RewriteVariableDecleration(BoundVariableDecleration node)
        {
            var expression = RewriteExpression(node.Expression);
            if (expression == node.Expression)
                return node;

            return new BoundVariableDecleration(node.Variable, expression);
        }

        private BoundStatement RewritePrintStatement(BoundPrintStatement node)
        {
            var expression = RewriteExpression(node.Expression);
            if (expression == node.Expression)
                return node;
            return new BoundPrintStatement(expression);
        }

        private BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            var body = RewriteStatement(node.Body);
            if (condition == node.Condition && body == node.Body)
                return node;
            return new BoundWhileStatement(condition, body);
        }

        private BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            var body = RewriteStatement(node.Body);
            var elseStatement = node.ElseStatement == null ? null : RewriteStatement(node.ElseStatement);
            if (condition == node.Condition && body == node.Body && elseStatement == node.ElseStatement)
                return node;
            return new BoundIfStatement(condition, body, elseStatement);
        }

        private BoundStatement RewriteForStatement(BoundForStatement node)
        {
            var variableDecl = RewriteStatement(node.VariableDecleration);
            var condition = RewriteExpression(node.Condition);
            var increment = RewriteExpression(node.Increment);
            var body = RewriteStatement(node.Body);

            if (variableDecl == node.VariableDecleration && condition == node.Condition && increment == node.Increment && body == node.Body)
                return node;

            return new BoundForStatement(variableDecl, condition, increment, body);
        }

        private BoundStatement RewriteBlockStatement(BoundBlockStatement node)
        {
            ImmutableArray<BoundStatement>.Builder builder = null;

            for (int i = 0; i < node.Statements.Length; i++)
            {
                var oldStmt = node.Statements[i];
                var newStmt = RewriteStatement(oldStmt);

                if (newStmt != oldStmt)
                {
                    if (builder == null)
                    {
                        builder = ImmutableArray.CreateBuilder<BoundStatement>(node.Statements.Length);
                        for (var j = 0; j < i; j++)
                            builder.Add(node.Statements[j]);
                    }
                    builder.Add(newStmt);
                }
            }
            if (builder == null)
                return node;

            return new BoundBlockStatement(builder.MoveToImmutable());
        }

        private BoundStatement RewriteExpressionStatement(BoundExpressionStatement node)
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
            else throw new Exception($"Unknown BoundExpression <{expression}>");

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

        protected virtual BoundExpression RewriteVaraibleExpression(BoundVariableExpression node)
        {
            return node;
        }

        protected virtual BoundExpression RewriteLiteralExpression(BoundLiteralExpression node)
        {
            return node;
        }
    }
}