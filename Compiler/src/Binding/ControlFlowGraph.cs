using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using Compiler.Text;
using System.Linq;
using Compiler.Symbols;

namespace Compiler.Binding
{
    internal sealed class ControlFlowGraph
    {
        private ControlFlowGraph(BasicBlock start, BasicBlock end, List<BasicBlock> blocks, List<BasicBlockBranch> branches)
        {
            Start = start;
            End = end;
            Blocks = blocks;
            Branches = branches;
        }

        public BasicBlock Start { get; }
        public BasicBlock End { get; }
        public List<BasicBlock> Blocks { get; }
        public List<BasicBlockBranch> Branches { get; }


        internal static ControlFlowGraph Create(BoundBlockStatement body)
        {
            var basicBuilder = new BasicBlockBuilder();
            var blocks = basicBuilder.Build(body);

            var graphBuilder = new GraphBuilder();
            return graphBuilder.Build(blocks);
        }

        public static bool AllPathsReturn(FunctionSymbol symbol, BoundBlockStatement body)
        {
            if (symbol.ReturnType == TypeSymbol.Void) return true;

            var graph = Create(body);

            foreach (var toEnd in graph.End.Incoming)
            {
                if (!toEnd.From.Statements.Any()) return false;
                if (toEnd.From.Statements.Last().Kind != BoundNodeKind.BoundReturnStatement) return false;
            }

            return true;

        }

        public void WriteTo(TextWriter writer)
        {
            string Quote(string text) => "\"" + text.Replace("\"", "\\\"") + "\"";

            writer.WriteLine("digraph G {");

            var blockIds = new Dictionary<BasicBlock, string>();

            for (int i = 0; i < Blocks.Count; i++)
            {
                var id = $"N{i}";
                blockIds.Add(Blocks[i], id);
            }

            foreach (var block in Blocks)
            {
                var id = blockIds[block];
                var label = Quote(block.ToString().Replace(Environment.NewLine, "\\l"));
                writer.WriteLine($"    {id} [label = {label} shape = box]");
            }

            foreach (var branch in Branches)
            {
                var fromId = blockIds[branch.From];
                var toId = blockIds[branch.To];
                var label = Quote(branch.ToString());
                writer.WriteLine($"    {fromId} -> {toId} [label = {label}]");
            }

            writer.WriteLine("}");
        }

        internal sealed class GraphBuilder
        {
            private readonly Dictionary<BoundStatement, BasicBlock> blockFromStatement;
            private readonly Dictionary<BoundLabel, BasicBlock> blockFromLabel;
            private readonly List<BasicBlockBranch> branches;

            private BasicBlock start;
            private BasicBlock end;

            public GraphBuilder()
            {
                branches = new List<BasicBlockBranch>();
                blockFromStatement = new Dictionary<BoundStatement, BasicBlock>();
                blockFromLabel = new Dictionary<BoundLabel, BasicBlock>();
                start = new BasicBlock(isStart: true);
                end = new BasicBlock(isStart: false);
            }

            public ControlFlowGraph Build(List<BasicBlock> blocks)
            {
                if (blocks.Count == 0)
                    Connect(start, end);
                else
                {
                    Connect(start, blocks.First());
                }

                foreach (var block in blocks)
                {
                    foreach (var statement in block.Statements)
                    {
                        blockFromStatement.Add(statement, block);
                        if (statement is BoundLabelStatement labelStatement)
                            blockFromLabel.Add(labelStatement.Label, block);
                    }
                }

                for (int i = 0; i < blocks.Count; i++)
                {
                    var current = blocks[i];
                    var next = (i == blocks.Count - 1 ? end : blocks[i + 1]);

                    for (int j = 0; j < current.Statements.Count; j++)
                    {
                        var statement = current.Statements[j];
                        var isLast = j == current.Statements.Count - 1;

                        switch (statement.Kind)
                        {
                            case BoundNodeKind.BoundGotoStatement:
                                var gotoStmt = (BoundGotoStatement)statement;
                                var toBlock = blockFromLabel[gotoStmt.Label];
                                Connect(current, toBlock);
                                break;
                            case BoundNodeKind.BoundConditionalGotoStatement:
                                var cgs = (BoundConditionalGotoStatement)statement;

                                var thenBlock = blockFromLabel[cgs.Label];
                                var elseBlock = next;

                                var negated = LogicalNot(cgs.Condition);

                                var thenCondition = cgs.JumpIfFalse ? negated : cgs.Condition;
                                var elseCondition = cgs.JumpIfFalse ? cgs.Condition : negated;

                                Connect(current, thenBlock, thenCondition);
                                Connect(current, elseBlock, elseCondition);

                                break;
                            case BoundNodeKind.BoundReturnStatement:
                                Connect(current, end);
                                break;
                            case BoundNodeKind.BoundLabelStatement:
                            case BoundNodeKind.BoundVariableDeclarationStatement:
                            case BoundNodeKind.BoundNopStatement:
                            case BoundNodeKind.BoundExpressionStatement:
                                if (isLast)
                                    Connect(current, next);
                                break;

                            default: throw new Exception("Unexpected kind");
                        }
                    }
                }

            Scan:
                foreach (var block in blocks)
                {
                    if (block.Incoming.Count == 0)
                    {
                        RemoveBlock(blocks, block);
                        goto Scan;
                    }
                }

                blocks.Insert(0, start);
                blocks.Add(end);

                return new ControlFlowGraph(start, end, blocks, branches);
            }

            private BoundExpression LogicalNot(BoundExpression expr)
            {
                if (expr.Constant is not null)
                    return new BoundLiteralExpression(expr.Constant.Value, expr.ResultType, expr.IsValid);

                return new BoundUnaryExpression(BoundUnaryOperator.LogicalNot, expr, TypeSymbol.Bool, expr.IsValid);
            }

            private void Connect(BasicBlock from, BasicBlock to, BoundExpression? condition = null)
            {
                var branch = new BasicBlockBranch(from, to, condition);
                from.Outgoing.Add(branch);
                to.Incoming.Add(branch);
                branches.Add(branch);
            }

            private void RemoveBlock(List<BasicBlock> blocks, BasicBlock block)
            {
                foreach (var branch in block.Incoming)
                {
                    branch.From.Outgoing.Remove(branch);
                    branches.Remove(branch);
                }
                foreach (var branch in block.Outgoing)
                {
                    branch.To.Incoming.Remove(branch);
                    branches.Remove(branch);
                }
                blocks.Remove(block);
            }
        }

        internal sealed class BasicBlockBuilder
        {

            private readonly List<BasicBlock> blocks;
            private readonly List<BoundStatement> statements;

            public BasicBlockBuilder()
            {
                blocks = new List<BasicBlock>();
                statements = new List<BoundStatement>();
            }

            public List<BasicBlock> Build(BoundBlockStatement boundBlock)
            {
                foreach (var statement in boundBlock.Statements)
                    switch (statement.Kind)
                    {
                        case BoundNodeKind.BoundExpressionStatement:
                        case BoundNodeKind.BoundVariableDeclarationStatement:
                        case BoundNodeKind.BoundNopStatement:
                            statements.Add(statement);
                            break;
                        case BoundNodeKind.BoundLabelStatement:
                            EndBlock();
                            statements.Add(statement);
                            break;
                        case BoundNodeKind.BoundConditionalGotoStatement:
                        case BoundNodeKind.BoundGotoStatement:
                        case BoundNodeKind.BoundReturnStatement:
                            statements.Add(statement);
                            EndBlock();
                            break;
                        default: throw new Exception("Unexpected kind");
                    }

                EndBlock();

                return blocks;
            }

            private void EndBlock()
            {
                if (statements.Count > 0)
                {
                    var block = new BasicBlock();
                    block.Statements.AddRange(statements);
                    blocks.Add(block);
                    statements.Clear();
                }
            }
        }

        internal sealed class BasicBlock
        {
            public BasicBlock()
            {
            }

            public BasicBlock(bool isStart)
            {
                IsStart = isStart;
                IsEnd = !isStart;
            }

            public List<BoundStatement> Statements { get; } = new List<BoundStatement>();
            public List<BasicBlockBranch> Incoming { get; } = new List<BasicBlockBranch>();
            public List<BasicBlockBranch> Outgoing { get; } = new List<BasicBlockBranch>();
            public bool IsStart { get; }
            public bool IsEnd { get; }

            public override string ToString()
            {
                if (IsStart)
                    return "<Start>";
                if (IsEnd)
                    return "<End>";

                using (var writer = new StringWriter())
                {
                    foreach (var stmt in Statements)
                    {
                        writer.WriteBoundNode(stmt);
                        writer.WriteLine();
                    }

                    return writer.ToString();
                }
            }
        }

        internal sealed class BasicBlockBranch
        {
            public BasicBlockBranch(BasicBlock from, BasicBlock to, BoundExpression? condition)
            {
                From = from;
                To = to;
                Condition = condition;
            }

            public BasicBlock From { get; }
            public BasicBlock To { get; }
            public BoundExpression? Condition { get; }

            public override string ToString()
            {
                if (Condition is null)
                    return string.Empty;
                return Condition.ToString();
            }
        }
    }
}
