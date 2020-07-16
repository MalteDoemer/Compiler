using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Compiler.Binding;
using Compiler.Symbols;

namespace Compiler.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {

        private int labelCount;

        private Lowerer()
        {

        }

        private BoundLabel CreateLabel() => new BoundLabel($"Label{++labelCount}");

        public static BoundBlockStatement Lower(FunctionSymbol function, BoundStatement body)
        {
            var lowerer = new Lowerer();
            var res = lowerer.RewriteStatement(body);
            return Flatten(function, res);
        }

        private static BoundBlockStatement Flatten(FunctionSymbol function, BoundStatement node)
        {
            var builder = ImmutableArray.CreateBuilder<BoundStatement>();
            var stack = new Stack<BoundStatement>();
            stack.Push(node);

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
            if (function != null && function.ReturnType == TypeSymbol.Void)
                if (builder.Count == 0 || CanFallThrough(builder.Last()))
                    builder.Add(new BoundReturnStatement(null, true));

            return new BoundBlockStatement(builder.ToImmutable(), node.IsValid);
        }

        private static bool CanFallThrough(BoundStatement boundStatement)
        {
            return boundStatement.Kind != BoundNodeKind.BoundReturnStatement && boundStatement.Kind != BoundNodeKind.BoundGotoStatement;
        }

        protected override BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            if (node.ElseStatement == null)
            {
                var endLabel = CreateLabel();
                var gotoIfFalse = new BoundConditionalGotoStatement(endLabel, node.Condition, jumpIfFalse: true, node.IsValid);
                var endLabelStmt = new BoundLabelStatement(endLabel, node.IsValid);
                var res = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(gotoIfFalse, node.Body, endLabelStmt), node.IsValid);
                return RewriteStatement(res);
            }
            else
            {
                var elseLabel = CreateLabel();
                var endLabel = CreateLabel();

                var gotoIfFalse = new BoundConditionalGotoStatement(elseLabel, node.Condition, jumpIfFalse: true, node.IsValid);
                var gotoEnd = new BoundGotoStatement(endLabel, node.IsValid);

                var elseLabelStmt = new BoundLabelStatement(elseLabel, node.IsValid);
                var endLabelStmt = new BoundLabelStatement(endLabel, node.IsValid);

                var res = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                    gotoIfFalse,
                    node.Body,
                    gotoEnd,
                    elseLabelStmt,
                    node.ElseStatement,
                    endLabelStmt
                ), node.IsValid);
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

            var gotoContinue = new BoundGotoStatement(node.ContinueLabel, node.IsValid);
            var gotoBody = new BoundConditionalGotoStatement(bodyLabel, node.Condition, jumpIfFalse: false, node.IsValid);


            var continueLabelStmt = new BoundLabelStatement(node.ContinueLabel, node.IsValid);
            var bodyLabelStmt = new BoundLabelStatement(bodyLabel, node.IsValid);
            var breakLabelStmt = new BoundLabelStatement(node.BreakLabel, node.IsValid);


            var res = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                gotoContinue,
                bodyLabelStmt,
                node.Body,
                continueLabelStmt,
                gotoBody,
                breakLabelStmt
            ), node.IsValid);

            return RewriteStatement(res);
        }

        protected override BoundStatement RewriteDoWhileStatement(BoundDoWhileStatement node)
        {
            var continueLabelStmt = new BoundLabelStatement(node.ContinueLabel, node.IsValid);
            var breakLabelStmt = new BoundLabelStatement(node.BreakLabel, node.IsValid);

            var gotoContinue = new BoundConditionalGotoStatement(node.ContinueLabel, node.Condition, jumpIfFalse: false, node.IsValid);

            var res = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                continueLabelStmt,
                node.Body,
                gotoContinue,
                breakLabelStmt
            ), node.IsValid);

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
            var increment = new BoundExpressionStatement(node.Increment, node.IsValid);
            var body = node.Body;
            var condition = node.Condition;

            var bodyLabel = CreateLabel();
            var continueLabel = node.ContinueLabel;
            var checkLabel = CreateLabel();
            var breakLabel = node.BreakLabel;

            var bodyLabelStmt = new BoundLabelStatement(bodyLabel, node.IsValid);
            var continueLabelStmt = new BoundLabelStatement(continueLabel, node.IsValid);
            var checkLabelStmt = new BoundLabelStatement(checkLabel, node.IsValid);
            var breakLabelStmt = new BoundLabelStatement(breakLabel, node.IsValid);


            var gotoCheck = new BoundGotoStatement(checkLabel, node.IsValid);
            var gotoTrueBody = new BoundConditionalGotoStatement(bodyLabel, node.Condition, jumpIfFalse: false, node.IsValid);

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
            ), node.IsValid);
            return RewriteStatement(res);
        }

        protected override BoundStatement RewriteConditionalGotoStatement(BoundConditionalGotoStatement node)
        {
            if (node.Condition.HasConstant)
            {
                var val = (bool)node.Condition.Constant.Value;
                val = node.JumpIfFalse ? !val : val;
                if (val)
                    return new BoundGotoStatement(node.Label, node.IsValid);
                else 
                    return new BoundNopStatement(node.IsValid);
            }

            return base.RewriteConditionalGotoStatement(node);
        }

        protected override BoundExpression RewriteBinaryExpression(BoundBinaryExpression node)
        {
            if (node.HasConstant)
                return node;

            var left = RewriteExpression(node.Left);
            var right = RewriteExpression(node.Right);

            var leftType = left.ResultType;
            var rightType = right.ResultType;
            var oneLiteral = (BoundLiteralExpression)null;

            if (node.Op == BoundBinaryOperator.Root)
                oneLiteral = new BoundLiteralExpression(1.0d, TypeSymbol.Float, true);

            switch (node.Op, leftType.Name, rightType.Name)
            {
                case (BoundBinaryOperator.Addition, "float", "int"):
                case (BoundBinaryOperator.Subtraction, "float", "int"):
                case (BoundBinaryOperator.Multiplication, "float", "int"):
                case (BoundBinaryOperator.Division, "float", "int"):
                case (BoundBinaryOperator.Power, "float", "int"):
                    right = new BoundConversionExpression(TypeSymbol.Float, right, right.IsValid);
                    break;
                case (BoundBinaryOperator.Addition, "int", "float"):
                case (BoundBinaryOperator.Subtraction, "int", "float"):
                case (BoundBinaryOperator.Multiplication, "int", "float"):
                case (BoundBinaryOperator.Division, "int", "float"):
                case (BoundBinaryOperator.Power, "int", "float"):
                    left = new BoundConversionExpression(TypeSymbol.Float, left, left.IsValid);
                    break;


                case (BoundBinaryOperator.Power, "int", "int"):
                    left = new BoundConversionExpression(TypeSymbol.Float, left, left.IsValid);
                    right = new BoundConversionExpression(TypeSymbol.Float, right, right.IsValid);
                    break;

                case (BoundBinaryOperator.Root, "int", "int"):
                    left = new BoundConversionExpression(TypeSymbol.Float, left, left.IsValid);
                    right = new BoundConversionExpression(TypeSymbol.Float, right, right.IsValid);
                    right = new BoundBinaryExpression(BoundBinaryOperator.Division, oneLiteral, right, TypeSymbol.Float, right.IsValid);
                    return new BoundBinaryExpression(BoundBinaryOperator.Power, left, right, TypeSymbol.Float, node.IsValid);

                case (BoundBinaryOperator.Root, "float", "int"):
                    right = new BoundConversionExpression(TypeSymbol.Float, right, right.IsValid);
                    right = new BoundBinaryExpression(BoundBinaryOperator.Division, oneLiteral, right, TypeSymbol.Float, right.IsValid);
                    return new BoundBinaryExpression(BoundBinaryOperator.Power, left, right, TypeSymbol.Float, node.IsValid);

                case (BoundBinaryOperator.Root, "int", "float"):
                    left = new BoundConversionExpression(TypeSymbol.Float, left, left.IsValid);
                    right = new BoundBinaryExpression(BoundBinaryOperator.Division, oneLiteral, right, TypeSymbol.Float, right.IsValid);
                    return new BoundBinaryExpression(BoundBinaryOperator.Power, left, right, TypeSymbol.Float, node.IsValid);

                case (BoundBinaryOperator.Root, "float", "float"):
                    right = new BoundBinaryExpression(BoundBinaryOperator.Division, oneLiteral, right, TypeSymbol.Float, right.IsValid);
                    return new BoundBinaryExpression(BoundBinaryOperator.Power, left, right, TypeSymbol.Float, node.IsValid);


                case (BoundBinaryOperator.Addition, "int", "str"):
                case (BoundBinaryOperator.Addition, "float", "str"):
                case (BoundBinaryOperator.Addition, "bool", "str"):
                    left = new BoundConversionExpression(TypeSymbol.String, left, left.IsValid);
                    break;
                case (BoundBinaryOperator.Addition, "str", "int"):
                case (BoundBinaryOperator.Addition, "str", "float"):
                case (BoundBinaryOperator.Addition, "str", "bool"):
                    right = new BoundConversionExpression(TypeSymbol.String, right, right.IsValid);
                    break;

                default:
                    if (left == node.Left && right == node.Right)
                        return node;
                    return new BoundBinaryExpression(node.Op, left, right, node.ResultType, node.IsValid);
            }

            return new BoundBinaryExpression(node.Op, left, right, node.ResultType, node.IsValid);
        }

    }
}