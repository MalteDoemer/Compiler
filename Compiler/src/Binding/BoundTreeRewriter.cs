using System;
using System.Collections.Immutable;
using Compiler.Symbols;

namespace Compiler.Binding
{
    internal abstract class BoundTreeRewriter
    {
        protected virtual BoundStatement RewriteStatement(BoundStatement statement)
        {
            switch (statement.Kind)
            {
                case BoundNodeKind.BoundBlockStatement:
                    return RewriteBlockStatement((BoundBlockStatement)statement);
                case BoundNodeKind.BoundExpressionStatement:
                    return RewriteExpressionStatement((BoundExpressionStatement)statement);
                case BoundNodeKind.BoundVariableDeclarationStatement:
                    return RewriteVariableDeclaration((BoundVariableDeclarationStatement)statement);
                case BoundNodeKind.BoundIfStatement:
                    return RewriteIfStatement((BoundIfStatement)statement);
                case BoundNodeKind.BoundForStatement:
                    return RewriteForStatement((BoundForStatement)statement);
                case BoundNodeKind.BoundWhileStatement:
                    return RewriteWhileStatement((BoundWhileStatement)statement);
                case BoundNodeKind.BoundDoWhileStatement:
                    return RewriteDoWhileStatement((BoundDoWhileStatement)statement);
                case BoundNodeKind.BoundConditionalGotoStatement:
                    return RewriteConditionalGotoStatement((BoundConditionalGotoStatement)statement);
                case BoundNodeKind.BoundGotoStatement:
                    return RewriteGotoStatement((BoundGotoStatement)statement);
                case BoundNodeKind.BoundLabelStatement:
                    return RewriteLabelStatement((BoundLabelStatement)statement);
                case BoundNodeKind.BoundReturnStatement:
                    return RewriteReturnStatement((BoundReturnStatement)statement);
                case BoundNodeKind.BoundNopStatement:
                    return RewriteNopStatement((BoundNopStatement)statement);
                default: throw new Exception($"Unknown BoundStatement <{statement}>");
            }
        }

        protected virtual BoundStatement RewriteReturnStatement(BoundReturnStatement node)
        {
            var expr = node.Expression is null ? null : RewriteExpression(node.Expression);
            if (expr == node.Expression)
                return node;
            return new BoundReturnStatement(expr, node.IsValid);
        }

        protected virtual BoundStatement RewriteNopStatement(BoundNopStatement node)
        {
            return node;
        }

        protected virtual BoundStatement RewriteConditionalGotoStatement(BoundConditionalGotoStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            if (condition == node.Condition)
                return node;

            return new BoundConditionalGotoStatement(node.Label, condition, node.JumpIfFalse, node.IsValid);
        }

        protected virtual BoundStatement RewriteVariableDeclaration(BoundVariableDeclarationStatement node)
        {
            var expression = RewriteExpression(node.Expression);
            if (expression == node.Expression)
                return node;

            return new BoundVariableDeclarationStatement(node.Variable, expression, node.IsValid);
        }

        protected virtual BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            var body = RewriteStatement(node.Body);
            if (condition == node.Condition && body == node.Body)
                return node;
            return new BoundWhileStatement(condition, body, node.BreakLabel, node.ContinueLabel, node.IsValid);
        }

        protected virtual BoundStatement RewriteDoWhileStatement(BoundDoWhileStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            var body = RewriteStatement(node.Body);
            if (condition == node.Condition && body == node.Body)
                return node;
            return new BoundWhileStatement(condition, body, node.BreakLabel, node.ContinueLabel, node.IsValid);
        }

        protected virtual BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            var body = RewriteStatement(node.Body);
            var elseStatement = node.ElseStatement is null ? null : RewriteStatement(node.ElseStatement);
            if (condition == node.Condition && body == node.Body && elseStatement == node.ElseStatement)
                return node;
            return new BoundIfStatement(condition, body, elseStatement, node.IsValid);
        }

        protected virtual BoundStatement RewriteForStatement(BoundForStatement node)
        {
            var variableDecl = RewriteStatement(node.VariableDeclaration);
            var condition = RewriteExpression(node.Condition);
            var increment = RewriteExpression(node.Increment);
            var body = RewriteStatement(node.Body);

            if (variableDecl == node.VariableDeclaration && condition == node.Condition && increment == node.Increment && body == node.Body)
                return node;

            return new BoundForStatement(variableDecl, condition, increment, body, node.BreakLabel, node.ContinueLabel, node.IsValid);
        }

        protected virtual BoundStatement RewriteBlockStatement(BoundBlockStatement node)
        {
            var builder = (ImmutableArray<BoundStatement>.Builder?)null;

            for (var i = 0; i < node.Statements.Length; i++)
            {
                var oldStatement = node.Statements[i];
                var newStatement = RewriteStatement(oldStatement);
                if (newStatement != oldStatement)
                {
                    if (builder is null)
                    {
                        builder = ImmutableArray.CreateBuilder<BoundStatement>(node.Statements.Length);

                        for (var j = 0; j < i; j++)
                            builder.Add(node.Statements[j]);
                    }
                }

                if (!(builder is null))
                    builder.Add(newStatement);
            }

            if (builder is null)
                return node;

            return new BoundBlockStatement(builder.MoveToImmutable(), node.IsValid);
        }

        protected virtual BoundStatement RewriteExpressionStatement(BoundExpressionStatement node)
        {
            var expr = RewriteExpression(node.Expression);
            if (expr == node.Expression)
                return node;
            return new BoundExpressionStatement(expr, node.IsValid, node.ShouldPop);
        }

        protected virtual BoundExpression RewriteExpression(BoundExpression node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.BoundLiteralExpression:
                    return RewriteLiteralExpression((BoundLiteralExpression)node);
                case BoundNodeKind.BoundVariableExpression:
                    return RewriteVariableExpression((BoundVariableExpression)node);
                case BoundNodeKind.BoundUnaryExpression:
                    return RewriteUnaryExpression((BoundUnaryExpression)node);
                case BoundNodeKind.BoundBinaryExpression:
                    return RewriteBinaryExpression((BoundBinaryExpression)node);
                case BoundNodeKind.BoundCallExpression:
                    return RewriteCallExpression((BoundCallExpression)node);
                case BoundNodeKind.BoundConversionExpression:
                    return RewriteConversionExpression((BoundConversionExpression)node);
                case BoundNodeKind.BoundAssignmentExpression:
                    return RewriteAssignmentExpression((BoundAssignmentExpression)node);
                case BoundNodeKind.BoundNewArray:
                    return RewriteNewArray((BoundNewArray)node);
                case BoundNodeKind.BoundTernaryExpression:
                    return RewriteTernaryExpression((BoundTernaryExpression)node);
                case BoundNodeKind.BoundStatementExpression:
                    return RewriteStatementExpression((BoundStatementExpression)node);
                case BoundNodeKind.BoundInvalidExpression:
                    return node;
                default: throw new Exception($"Unknown BoundExpression <{node}>");
            }
        }

        protected virtual BoundExpression RewriteStatementExpression(BoundStatementExpression node)
        {
            var statement = RewriteStatement(node.Statement);
            if (statement == node.Statement)
                return node;
            return new BoundStatementExpression(statement, node.ResultType, node.IsValid);
        }

        protected virtual BoundExpression RewriteTernaryExpression(BoundTernaryExpression node)
        {
            var condition = RewriteExpression(node.Condition);
            var thenExpr = RewriteExpression(node.ThenExpression);
            var elseExpr = RewriteExpression(node.ElseExpression);

            if (condition == node.Condition &&
                thenExpr == node.ThenExpression &&
                elseExpr == node.ElseExpression)
                return node;
            return new BoundTernaryExpression(condition, thenExpr, elseExpr, node.ResultType, node.IsValid);
        }

        protected virtual BoundExpression RewriteNewArray(BoundNewArray node)
        {
            var size = RewriteExpression(node.Size);
            if (size == node.Size)
                return node;

            return new BoundNewArray(node.ResultType, size, node.IsValid);
        }

        protected virtual BoundExpression RewriteConversionExpression(BoundConversionExpression node)
        {
            var expr = RewriteExpression(node.Expression);
            if (expr == node.Expression)
                return node;
            return new BoundConversionExpression(node.ResultType, expr, node.IsValid);
        }

        protected virtual BoundExpression RewriteCallExpression(BoundCallExpression node)
        {
            var builder = (ImmutableArray<BoundExpression>.Builder?)null;

            for (var i = 0; i < node.Arguments.Length; i++)
            {
                var oldStatement = node.Arguments[i];
                var newStatement = RewriteExpression(oldStatement);
                if (newStatement != oldStatement)
                {
                    if (builder is null)
                    {
                        builder = ImmutableArray.CreateBuilder<BoundExpression>(node.Arguments.Length);

                        for (var j = 0; j < i; j++)
                            builder.Add(node.Arguments[j]);
                    }
                }

                if (!(builder is null))
                    builder.Add(newStatement);
            }

            if (builder is null)
                return node;

            return new BoundCallExpression(node.Symbol, builder.MoveToImmutable(), node.IsValid);
        }

        protected virtual BoundExpression RewriteAssignmentExpression(BoundAssignmentExpression node)
        {
            var expr = RewriteExpression(node.Expression);
            if (expr == node.Expression)
                return node;
            return new BoundAssignmentExpression(node.Variable, expr, node.IsValid);
        }

        protected virtual BoundExpression RewriteBinaryExpression(BoundBinaryExpression node)
        {
            var left = RewriteExpression(node.Left);
            var right = RewriteExpression(node.Right);

            if (left == node.Left && right == node.Right)
                return node;

            return new BoundBinaryExpression(node.Op, left, right, node.ResultType, node.IsValid);
        }

        protected virtual BoundExpression RewriteUnaryExpression(BoundUnaryExpression node)
        {
            var right = RewriteExpression(node.Expression);
            if (right == node.Expression)
                return node;
            return new BoundUnaryExpression(node.Op, right, node.ResultType, node.IsValid);
        }

        protected virtual BoundStatement RewriteLabelStatement(BoundLabelStatement node) => node;

        protected virtual BoundStatement RewriteGotoStatement(BoundGotoStatement node) => node;

        protected virtual BoundExpression RewriteVariableExpression(BoundVariableExpression node) => node;

        protected virtual BoundExpression RewriteLiteralExpression(BoundLiteralExpression node) => node;
    }
}