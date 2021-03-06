using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Compiler.Diagnostics;
using Compiler.Lowering;
using Compiler.Symbols;
using Compiler.Syntax;
using Compiler.Text;

namespace Compiler.Binding
{
    internal sealed class Binder : IDiagnostable
    {
        private readonly DiagnosticBag diagnostics;
        private readonly FunctionSymbol? function;
        private readonly BoundTypeResolver resolver;
        private readonly bool isScript;
        private readonly Stack<(BoundLabel breakLabel, BoundLabel continueLabel)> labelStack;

        private bool isTreeValid;
        private bool isStatementValid;
        private int labelCounter;
        private BoundScope scope;

        public IEnumerable<Diagnostic> GetDiagnostics() => diagnostics;

        private void ReportError(ErrorMessage message, TextLocation span, params object[] values)
        {
            if (isStatementValid)
                diagnostics.ReportError(message, span, values);
            isStatementValid = false;
            isTreeValid = false;
        }

        private Binder(BoundScope parentScope, BoundTypeResolver resolver, bool isScript, FunctionSymbol? function)
        {
            this.resolver = resolver;
            this.isScript = isScript;
            this.function = function;
            this.isTreeValid = true;
            this.isStatementValid = true;
            this.labelStack = new Stack<(BoundLabel breakLabel, BoundLabel continueLabel)>();
            this.scope = new BoundScope(parentScope);
            this.diagnostics = new DiagnosticBag();

            if (function is FunctionSymbol)
                foreach (var param in function.Parameters)
                    scope.TryDeclareVariable(param);
        }

        public static BoundProgram BindProgram(string moduleName, string[] references, bool isScript, IEnumerable<CompilationUnitSyntax> units)
        {
            var parentScope = CreateRootScope();
            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
            var functionSyntax = units.SelectMany(u => u.Members.OfType<FunctionDeclarationSyntax>());
            var statementSyntax = units.SelectMany(u => u.Members.OfType<GlobalStatementSynatx>());
            var globalStatementBuilder = ImmutableArray.CreateBuilder<BoundStatement>();
            var resolver = new BoundTypeResolver(moduleName, references);
            var globalBinder = new Binder(parentScope, resolver, isScript, function: null);
            var isProgramValid = true;

            globalBinder.ResolvePredefinedTypes();

            foreach (var func in functionSyntax)
                globalBinder.DeclareFunction(func);

            foreach (var stmt in statementSyntax)
                globalStatementBuilder.Add(globalBinder.BindStatement(stmt.Statement));

            diagnostics.AddRange(globalBinder.GetDiagnostics());
            isProgramValid = isProgramValid && globalBinder.isTreeValid;

            var declaredFunctions = globalBinder.scope.GetDeclaredFunctions();
            var declaredVariables = globalBinder.scope.GetDeclaredVariables();

            var currentScope = globalBinder.scope;
            var functions = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();

            foreach (var symbol in declaredFunctions)
            {
                var binder = new Binder(currentScope, resolver, isScript, symbol);
                var body = binder.BindBlockStatmentSyntax(symbol.Syntax!.Body);
                var loweredBody = Lowerer.Lower(symbol, body);

                if (!ControlFlowGraph.AllPathsReturn(symbol, loweredBody))
                    binder.ReportError(ErrorMessage.AllPathsMustReturn, symbol.Syntax.Identifier.Location);

                functions.Add(symbol, loweredBody);
                diagnostics.AddRange(binder.GetDiagnostics());
                isProgramValid = isProgramValid && binder.isTreeValid;
            }

            globalBinder.scope.TryLookUpFunction("main", out var mainFunction);

            if (mainFunction != FunctionSymbol.Invalid)
            {
                void Report(string message, TextLocation location)
                {
                    if (isProgramValid)
                        diagnostics.Add(new Diagnostic(message, location, ErrorLevel.Error));
                    isProgramValid = false;
                }

                if (mainFunction.Parameters.Length != 0)
                    Report("Main function cannot have arguments.", mainFunction.Syntax!.Parameters.Location);
                if (mainFunction.ReturnType != TypeSymbol.Void)
                    Report("Main function must return void.", mainFunction.Syntax!.ReturnType.Location);
            }
            else if (isScript)
            {
                mainFunction = new FunctionSymbol("main", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Void);
                var mainBody = new BoundBlockStatement(ImmutableArray<BoundStatement>.Empty, isProgramValid);
                functions.Add(mainFunction, Lowerer.Lower(mainFunction, mainBody));
            }

            var globalFunction = (FunctionSymbol?)null;

            if (globalStatementBuilder.Count > 0)
            {
                globalFunction = new FunctionSymbol("$global", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Void);
                var globalBody = Lowerer.Lower(globalFunction, new BoundBlockStatement(globalStatementBuilder.ToImmutable(), isProgramValid));
                functions.Add(globalFunction, globalBody);
            }

            // Special case 
            // In a script global statements can have nested scopes 
            if (isScript && !(globalFunction is null))
            {
                var body = functions[globalFunction];
                var stmts = body.Statements.OfType<BoundVariableDeclarationStatement>();
                declaredVariables = declaredVariables.Union(stmts.Select(s => s.Variable)).ToImmutableArray();
            }

            if (mainFunction == FunctionSymbol.Invalid)
                mainFunction = null;

            return new BoundProgram(declaredVariables, globalFunction, mainFunction, functions.ToImmutable(), resolver.MainAssembly, resolver.References, resolver.Types.ToImmutableDictionary(), new DiagnosticReport(diagnostics.ToImmutable()), isProgramValid);
        }

        private static BoundScope CreateRootScope()
        {
            var scope = new BoundScope(null);
            foreach (var b in BuiltInFunctions.GetAll())
                scope.TryDeclareFunction(b);
            return scope;
        }

        private void ResolvePredefinedTypes()
        {
            foreach (var primitive in TypeSymbol.GetPrimitiveTypes())
            {
                if (!resolver.ResolveType(primitive))
                    ReportError(ErrorMessage.MissingRequiredType, TextLocation.Undefined, primitive.Name);
            }
        }

        private void DeclareFunction(FunctionDeclarationSyntax func)
        {
            var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
            var seenParameters = new HashSet<string>();

            for (var i = 0; i < func.Parameters.Length; i++)
            {
                var parameterSyntax = func.Parameters[i];
                var type = BindType(parameterSyntax.TypeClause.TypeSyntax);

                if (!resolver.ResolveType(type))
                    ReportError(ErrorMessage.TypeNotFound, parameterSyntax.TypeClause.TypeSyntax.Location, type.Name);

                var name = parameterSyntax.Identifier.Location.ToString();

                if (!seenParameters.Add(name))
                    diagnostics.ReportError(ErrorMessage.DuplicatedParameters, parameterSyntax.Location, name);
                else parameters.Add(new ParameterSymbol(name, i, type));
            }

            TypeSymbol returnType;
            if (func.ReturnType.IsExplicit)
                returnType = BindType(func.ReturnType.TypeSyntax);
            else
                returnType = TypeSymbol.Void;

            if (!resolver.ResolveType(returnType))
                ReportError(ErrorMessage.TypeNotFound, func.ReturnType.TypeSyntax.Location);

            var symbol = new FunctionSymbol(func.Identifier.Location.ToString(), parameters.ToImmutable(), returnType, func);

            if (!scope.TryDeclareFunction(symbol))
                diagnostics.ReportError(ErrorMessage.FunctionAlreadyDeclared, func.Identifier.Location, func.Identifier.Value);
        }

        private BoundStatement BindStatement(StatementSyntax syntax)
        {
            isStatementValid = true;
            if (!syntax.IsValid)
            {
                isStatementValid = false;
                isTreeValid = false;
            }

            switch (syntax.Kind)
            {
                case SyntaxNodeKind.BlockStatmentSyntax:
                    return BindBlockStatmentSyntax((BlockStatmentSyntax)syntax);
                case SyntaxNodeKind.ExpressionStatementSyntax:
                    return BindExpressionStatementSyntax((ExpressionStatementSyntax)syntax);
                case SyntaxNodeKind.VariableDeclarationStatementSyntax:
                    return BindVariableDeclarationStatementSyntax((VariableDeclarationStatementSyntax)syntax);
                case SyntaxNodeKind.IfStatementSyntax:
                    return BindIfStatementSyntax((IfStatementSyntax)syntax);
                case SyntaxNodeKind.WhileStatementSyntax:
                    return BindWhileStatementSyntax((WhileStatementSyntax)syntax);
                case SyntaxNodeKind.ForStatementSyntax:
                    return BindForStatementSyntax((ForStatementSyntax)syntax);
                case SyntaxNodeKind.DoWhileStatementSyntax:
                    return BindDoWhileStatementSyntax((DoWhileStatementSyntax)syntax);
                case SyntaxNodeKind.BreakStatementSyntax:
                    return BindBreakStatementSyntax((BreakStatementSyntax)syntax);
                case SyntaxNodeKind.ContinueStatementSyntax:
                    return BindContinueStatementSyntax((ContinueStatementSyntax)syntax);
                case SyntaxNodeKind.ReturnStatementSyntax:
                    return BindReturnStatementSyntax((ReturnStatementSyntax)syntax);
                default: throw new Exception($"Unexpected SyntaxKind <{syntax.Kind}>");
            }
        }

        private BoundStatement BindBlockStatmentSyntax(BlockStatmentSyntax syntax)
        {
            var builder = ImmutableArray.CreateBuilder<BoundStatement>();
            scope = new BoundScope(scope);
            foreach (var stmt in syntax.Statements)
            {
                var bound = BindStatement(stmt);
                builder.Add(bound);
            }
            scope = scope.Parent!;
            return new BoundBlockStatement(builder.ToImmutable(), isTreeValid);
        }

        private BoundStatement BindExpressionStatementSyntax(ExpressionStatementSyntax syntax)
        {
            var expr = BindExpression(syntax.Expression, true);
            return new BoundExpressionStatement(expr, isTreeValid, true);
        }

        private BoundStatement BindVariableDeclarationStatementSyntax(VariableDeclarationStatementSyntax syntax)
        {
            TypeSymbol type;
            BoundExpression expr;

            if (syntax.TypeClause.IsExplicit)
            {
                type = BindType(syntax.TypeClause.TypeSyntax);

                if (!resolver.ResolveType(type))
                    ReportError(ErrorMessage.TypeNotFound, syntax.TypeClause.TypeSyntax.Location, type.Name);

                expr = CheckTypeAndConversion(type, syntax.Expression);
            }
            else
            {
                expr = BindExpression(syntax.Expression, canBeVoid: false);
                type = expr.ResultType;

                if (!resolver.ResolveType(type))
                    ReportError(ErrorMessage.TypeNotFound, syntax.VarKeyword.Location, type.Name);
            }

            bool isConst = syntax.VarKeyword.TokenKind == SyntaxTokenKind.LetKeyword;



            VariableSymbol variable;
            if (function is null)
                variable = new GlobalVariableSymbol(syntax.Identifier.Location.ToString(), type, isConst, expr.Constant);
            else
                variable = new LocalVariableSymbol(syntax.Identifier.Location.ToString(), type, isConst, expr.Constant);

            if (!scope.TryDeclareVariable(variable))
                ReportError(ErrorMessage.VariableAlreadyDeclared, syntax.Identifier.Location, variable.Name);
            return new BoundVariableDeclarationStatement(variable, expr, isTreeValid);
        }

        private BoundStatement BindIfStatementSyntax(IfStatementSyntax syntax)
        {
            var condition = CheckTypeAndConversion(TypeSymbol.Bool, syntax.Condition);
            var stmt = BindStatement(syntax.Body);
            var elseStmt = syntax.ElseStatement is null ? null : BindStatement(syntax.ElseStatement.Body);
            return new BoundIfStatement(condition, stmt, elseStmt, isTreeValid);
        }

        private BoundStatement BindWhileStatementSyntax(WhileStatementSyntax syntax)
        {
            var condition = CheckTypeAndConversion(TypeSymbol.Bool, syntax.Condition);
            var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
            return new BoundWhileStatement(condition, body, breakLabel, continueLabel, isTreeValid);
        }

        private BoundStatement BindForStatementSyntax(ForStatementSyntax syntax)
        {
            var variableDecl = BindStatement(syntax.VariableDeclaration);
            var condition = CheckTypeAndConversion(TypeSymbol.Bool, syntax.Condition);
            var increment = BindExpression(syntax.Increment);
            var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
            return new BoundForStatement(variableDecl, condition, increment, body, breakLabel, continueLabel, isTreeValid);
        }

        private BoundStatement BindDoWhileStatementSyntax(DoWhileStatementSyntax syntax)
        {
            var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
            var condition = CheckTypeAndConversion(TypeSymbol.Bool, syntax.Condition);
            return new BoundDoWhileStatement(body, condition, breakLabel, continueLabel, isTreeValid);
        }

        private BoundStatement BindBreakStatementSyntax(BreakStatementSyntax syntax)
        {
            BoundLabel label;

            if (labelStack.Count == 0)
            {
                ReportError(ErrorMessage.InvalidBreakOrContinue, syntax.Location, "break");
                label = new BoundLabel("Invalid break");
            }
            else
                label = labelStack.Peek().breakLabel;

            return new BoundGotoStatement(label, isTreeValid);
        }

        private BoundStatement BindContinueStatementSyntax(ContinueStatementSyntax syntax)
        {
            BoundLabel label;

            if (labelStack.Count == 0)
            {
                ReportError(ErrorMessage.InvalidBreakOrContinue, syntax.Location, "continue");
                label = new BoundLabel("Invalid continue");
            }
            else
                label = labelStack.Peek().continueLabel;

            return new BoundGotoStatement(label, isTreeValid);
        }

        private BoundStatement BindReturnStatementSyntax(ReturnStatementSyntax syntax)
        {
            BoundExpression? expr;

            if (function is null)
            {
                ReportError(ErrorMessage.ReturnOnlyInFunction, syntax.Location);
                expr = null;
            }
            else if (syntax.ReturnExpression is null)
            {
                if (function.ReturnType != TypeSymbol.Void)
                    ReportError(ErrorMessage.IncompatibleTypes, syntax.VoidToken!.Location, function.ReturnType, TypeSymbol.Void);
                expr = null;
            }
            else
            {
                expr = CheckTypeAndConversion(function.ReturnType, syntax.ReturnExpression);
            }

            return new BoundReturnStatement(expr, isTreeValid);
        }

        private BoundStatement BindLoopBody(StatementSyntax syntax, out BoundLabel breakLabel, out BoundLabel continueLabel)
        {
            labelCounter++;
            breakLabel = new BoundLabel($"break{labelCounter}");
            continueLabel = new BoundLabel($"continue{labelCounter}");

            labelStack.Push((breakLabel, continueLabel));
            var res = BindStatement(syntax);
            labelStack.Pop();
            return res;
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax, bool canBeVoid = false)
        {
            var res = BindExpressionInternal(syntax);
            if (!canBeVoid && res.ResultType == TypeSymbol.Void)
            {
                ReportError(ErrorMessage.CannotBeVoid, syntax.Location);
                return new BoundInvalidExpression();
            }

            return res;
        }

        private BoundExpression BindExpressionInternal(ExpressionSyntax syntax)
        {
            if (!syntax.IsValid)
            {
                isStatementValid = false;
                isTreeValid = false;
            }

            switch (syntax.Kind)
            {
                case SyntaxNodeKind.LiteralExpressionSyntax:
                    return BindLiteralExpressionSyntax((LiteralExpressionSyntax)syntax);
                case SyntaxNodeKind.VariableExpressionSyntax:
                    return BindVariableExpressionSyntax((VariableExpressionSyntax)syntax);
                case SyntaxNodeKind.UnaryExpressionSyntax:
                    return BindUnaryExpressionSyntax((UnaryExpressionSyntax)syntax);
                case SyntaxNodeKind.BinaryExpressionSyntax:
                    return BindBinaryExpressionSyntax((BinaryExpressionSyntax)syntax);
                case SyntaxNodeKind.CallExpressionSyntax:
                    return BindCallExpressionSyntax((CallExpressionSyntax)syntax);
                case SyntaxNodeKind.AssignmentExpressionSyntax:
                    return BindAssignmentExpressionSyntax((AssignmentExpressionSyntax)syntax);
                case SyntaxNodeKind.AdditionalAssignmentExpressionSyntax:
                    return BindAdditionalAssignmentExpressionSyntax((AdditionalAssignmentExpressionSyntax)syntax);
                case SyntaxNodeKind.PostIncDecExpressionSyntax:
                    return BindPostIncDecExpressionSyntax((PostIncDecExpressionSyntax)syntax);
                case SyntaxNodeKind.ArrayCreationSyntax:
                    return BindNewArraySyntax((ArrayCreationSyntax)syntax);
                case SyntaxNodeKind.TernaryExpressionSyntax:
                    return BindTernaryExpressionSyntax((TernaryExpressionSyntax)syntax);
                case SyntaxNodeKind.ParenthesizedExpression:
                    return BindExpression(((ParenthesizedExpression)syntax).Expression);
                default: throw new Exception($"Unexpected SyntaxKind <{syntax.Kind}>");
            }
        }

        private BoundExpression BindTernaryExpressionSyntax(TernaryExpressionSyntax syntax)
        {
            var condition = CheckTypeAndConversion(TypeSymbol.Bool, syntax.Condition);
            var thenExpr = BindExpression(syntax.ThenExpression);
            var elseExpr = CheckTypeAndConversion(thenExpr.ResultType, syntax.ElseExpression); // TODO find base class
            return new BoundTernaryExpression(condition, thenExpr, elseExpr, thenExpr.ResultType, isTreeValid);
        }

        private BoundExpression BindNewArraySyntax(ArrayCreationSyntax syntax)
        {
            BoundExpression size;

            var typeSyntax = syntax.ArrayTypeSyntax;

            while (typeSyntax.UnderlyingType is ArrayTypeSyntax array)
                typeSyntax = array;


            if (typeSyntax.Size is null)
            {
                var span = TextSpan.FromBounds(typeSyntax.LeftBracket.Location.Start, typeSyntax.RightBracket.Location.End);
                ReportError(ErrorMessage.ArrayCreationMustHaveSize, new TextLocation(typeSyntax.Location.Text, span));
                size = new BoundLiteralExpression(0, TypeSymbol.Int, isTreeValid);
            }
            else
            {
                size = CheckTypeAndConversion(TypeSymbol.Int, typeSyntax.Size);
            }

            var type = (ArrayTypeSymbol)BindType(syntax.ArrayTypeSyntax);

            if (!resolver.ResolveType(type))
                ReportError(ErrorMessage.TypeNotFound, syntax.ArrayTypeSyntax.Location, type.Name);

            if (!resolver.ResolveType(type.UnderlyingType))
                ReportError(ErrorMessage.TypeNotFound, syntax.ArrayTypeSyntax.UnderlyingType.Location, type.Name);

            return new BoundArrayCreation(type, size, isTreeValid);
        }

        private BoundExpression BindLiteralExpressionSyntax(LiteralExpressionSyntax syntax)
        {
            var value = syntax.Literal.Value;
            var type = BindFacts.GetTypeSymbol(syntax.Literal.TokenKind);
            return new BoundLiteralExpression(value, type, isTreeValid);
        }

        private BoundExpression BindVariableExpressionSyntax(VariableExpressionSyntax syntax)
        {
            var identifier = syntax.Name.Location.ToString();
            if (!scope.TryLookUpVariable(identifier, out var variable))
                ReportError(ErrorMessage.UnresolvedIdentifier, syntax.Name.Location, syntax.Name.Value);
            return new BoundVariableExpression(variable, isTreeValid);
        }

        private BoundExpression BindUnaryExpressionSyntax(UnaryExpressionSyntax syntax)
        {
            var right = BindExpression(syntax.Expression);
            var boundOperator = BindUnaryOperator(syntax.Op.TokenKind);
            var resultType = BindFacts.ResolveUnaryType(boundOperator, right.ResultType);
            if (boundOperator == BoundUnaryOperator.Invalid || resultType == TypeSymbol.Invalid)
                ReportError(ErrorMessage.UnsupportedUnaryOperator, syntax.Op.Location, syntax.Op.Location.ToString(), right.ResultType);
            return new BoundUnaryExpression(boundOperator, right, resultType, isTreeValid);
        }

        private BoundExpression BindBinaryExpressionSyntax(BinaryExpressionSyntax syntax)
        {
            var left = BindExpression(syntax.Left);
            var right = BindExpression(syntax.Right);
            var boundOperator = BindBinaryOperator(syntax.Op.TokenKind);
            var resultType = BindFacts.ResolveBinaryType(boundOperator, left.ResultType, right.ResultType);

            if (boundOperator == BoundBinaryOperator.Invalid || resultType == TypeSymbol.Invalid)
                ReportError(ErrorMessage.UnsupportedBinaryOperator, syntax.Op.Location, syntax.Op.Location.ToString(), left.ResultType, right.ResultType);
            return new BoundBinaryExpression(boundOperator, left, right, resultType, isTreeValid);
        }

        private BoundExpression BindCallExpressionSyntax(CallExpressionSyntax syntax)
        {
            var name = syntax.Identifier.Location.ToString();
            var argLen = syntax.Arguments.Length;

            if (argLen == 1 && TypeSymbol.Lookup(name) is TypeSymbol type)
            {
                if (!resolver.ResolveType(type))
                    ReportError(ErrorMessage.TypeNotFound, syntax.Identifier.Location, type.Name);
                return BindExplicitConversion(type, syntax.Arguments[0]);
            }


            if (!scope.TryLookUpFunction(name, out var symbol))
                ReportError(ErrorMessage.UnresolvedIdentifier, syntax.Identifier.Location, name);

            var paramLen = symbol.Parameters.Length;
            if (argLen != paramLen)
                ReportError(ErrorMessage.WrongAmountOfArguments, syntax.Identifier.Location, symbol.Name, paramLen, argLen);

            var len = Math.Min(argLen, paramLen);
            var argBuilder = ImmutableArray.CreateBuilder<BoundExpression>(len);

            for (int i = 0; i < len; i++)
            {
                var arg = syntax.Arguments[i];
                var param = symbol.Parameters[i];

                var boundArg = CheckTypeAndConversion(param.Type, arg);
                argBuilder.Add(boundArg);
            }

            return new BoundCallExpression(symbol, argBuilder.MoveToImmutable(), isTreeValid);
        }

        private BoundExpression BindAssignmentExpressionSyntax(AssignmentExpressionSyntax syntax)
        {
            var name = syntax.Identifier.Location.ToString();

            if (!scope.TryLookUpVariable(name, out var variable))
                ReportError(ErrorMessage.UnresolvedIdentifier, syntax.Identifier.Location, name);

            if (variable.IsReadOnly)
                ReportError(ErrorMessage.CannotAssignToReadOnly, syntax.Identifier.Location, syntax.Identifier.Value);

            var expr = CheckTypeAndConversion(variable.Type, syntax.Expression);
            return new BoundAssignmentExpression(variable, expr, isTreeValid);
        }

        private BoundExpression BindAdditionalAssignmentExpressionSyntax(AdditionalAssignmentExpressionSyntax syntax)
        {
            var name = syntax.Identifier.Location.ToString();

            if (!scope.TryLookUpVariable(name, out var variable))
                ReportError(ErrorMessage.UnresolvedIdentifier, syntax.Identifier.Location, name);

            if (variable.IsReadOnly)
                ReportError(ErrorMessage.CannotAssignToReadOnly, syntax.Identifier.Location, syntax.Identifier.Value);

            var left = new BoundVariableExpression(variable, isTreeValid);
            var right = BindExpression(syntax.Expression);

            var op = BindBinaryOperator(syntax.Op.TokenKind);
            var resultType = BindFacts.ResolveBinaryType(op, left.ResultType, right.ResultType);

            if (op == BoundBinaryOperator.Invalid || resultType == TypeSymbol.Invalid)
                ReportError(ErrorMessage.UnsupportedBinaryOperator, syntax.Op.Location, syntax.Op.Location.ToString(), left.ResultType, right.ResultType);

            var binaryExpression = new BoundBinaryExpression(op, left, right, resultType, isTreeValid);
            return new BoundAssignmentExpression(variable, binaryExpression, isTreeValid);
        }

        private BoundExpression BindPostIncDecExpressionSyntax(PostIncDecExpressionSyntax syntax)
        {
            if (!scope.TryLookUpVariable((string)syntax.Identifier.Value, out var variable))
                ReportError(ErrorMessage.UnresolvedIdentifier, syntax.Identifier.Location, (string)syntax.Identifier.Value);

            if (variable.IsReadOnly)
                ReportError(ErrorMessage.CannotAssignToReadOnly, syntax.Identifier.Location, syntax.Identifier.Value);

            var left = new BoundVariableExpression(variable, isTreeValid);
            var right = new BoundLiteralExpression(1, TypeSymbol.Int, isTreeValid);
            var op = BindBinaryOperator(syntax.Op.TokenKind);
            var resultType = BindFacts.ResolveBinaryType(op, left.ResultType, right.ResultType);

            if (op == BoundBinaryOperator.Invalid || resultType == TypeSymbol.Invalid)
                ReportError(ErrorMessage.UnsupportedBinaryOperator, syntax.Op.Location, syntax.Op.Location.ToString(), left.ResultType, right.ResultType);

            var binaryExpression = new BoundBinaryExpression(op, left, right, resultType, isTreeValid);
            return new BoundAssignmentExpression(variable, binaryExpression, isTreeValid);
        }

        private BoundBinaryOperator BindBinaryOperator(SyntaxTokenKind op)
        {
            switch (op)
            {
                case SyntaxTokenKind.Plus: return BoundBinaryOperator.Addition;
                case SyntaxTokenKind.Minus: return BoundBinaryOperator.Subtraction;
                case SyntaxTokenKind.Star: return BoundBinaryOperator.Multiplication;
                case SyntaxTokenKind.Slash: return BoundBinaryOperator.Division;
                case SyntaxTokenKind.StarStar: return BoundBinaryOperator.Power;
                case SyntaxTokenKind.SlashSlah: return BoundBinaryOperator.Root;
                case SyntaxTokenKind.Percentage: return BoundBinaryOperator.Modulo;
                case SyntaxTokenKind.Ampersand: return BoundBinaryOperator.BitwiseAnd;
                case SyntaxTokenKind.Pipe: return BoundBinaryOperator.BitwiseOr;
                case SyntaxTokenKind.Hat: return BoundBinaryOperator.BitwiseXor;
                case SyntaxTokenKind.EqualEqual: return BoundBinaryOperator.EqualEqual;
                case SyntaxTokenKind.NotEqual: return BoundBinaryOperator.NotEqual;
                case SyntaxTokenKind.LessThan: return BoundBinaryOperator.LessThan;
                case SyntaxTokenKind.LessEqual: return BoundBinaryOperator.LessEqual;
                case SyntaxTokenKind.GreaterThan: return BoundBinaryOperator.GreaterThan;
                case SyntaxTokenKind.GreaterEqual: return BoundBinaryOperator.GreaterEqual;
                case SyntaxTokenKind.AmpersandAmpersand: return BoundBinaryOperator.LogicalAnd;
                case SyntaxTokenKind.PipePipe: return BoundBinaryOperator.LogicalOr;
                case SyntaxTokenKind.PlusEqual: return BoundBinaryOperator.Addition;
                case SyntaxTokenKind.MinusEqual: return BoundBinaryOperator.Subtraction;
                case SyntaxTokenKind.StarEqual: return BoundBinaryOperator.Multiplication;
                case SyntaxTokenKind.SlashEqual: return BoundBinaryOperator.Division;
                case SyntaxTokenKind.AmpersandEqual: return BoundBinaryOperator.BitwiseAnd;
                case SyntaxTokenKind.PipeEqual: return BoundBinaryOperator.BitwiseOr;
                case SyntaxTokenKind.PlusPlus: return BoundBinaryOperator.Addition;
                case SyntaxTokenKind.MinusMinus: return BoundBinaryOperator.Subtraction;
                default: return BoundBinaryOperator.Invalid;
            }
        }

        private BoundUnaryOperator BindUnaryOperator(SyntaxTokenKind op)
        {
            switch (op)
            {
                case SyntaxTokenKind.Plus: return BoundUnaryOperator.Identety;
                case SyntaxTokenKind.Minus: return BoundUnaryOperator.Negation;
                case SyntaxTokenKind.Bang: return BoundUnaryOperator.LogicalNot;
                case SyntaxTokenKind.Tilde: return BoundUnaryOperator.BitwiseNot;
                default: return BoundUnaryOperator.Invalid;
            }
        }

        private BoundExpression CheckTypeAndConversion(TypeSymbol type, ExpressionSyntax expression)
        {
            var expr = BindExpression(expression);
            var conversionType = BindFacts.ClassifyConversion(expr.ResultType, type);

            if (conversionType == ConversionType.Identety)
                return expr;
            else if (conversionType == ConversionType.Explicit)
                ReportError(ErrorMessage.MissingExplicitConversion, expression.Location, type, expr.ResultType);
            else if (conversionType == ConversionType.None)
            {
                ReportError(ErrorMessage.IncompatibleTypes, expression.Location, type, expr.ResultType);
            }

            return new BoundConversionExpression(type, expr, isTreeValid);
        }

        private BoundExpression BindExplicitConversion(TypeSymbol type, ExpressionSyntax syntax)
        {
            var expr = BindExpression(syntax);
            var conversion = BindFacts.ClassifyConversion(expr.ResultType, type);

            if (conversion == ConversionType.None)
                ReportError(ErrorMessage.CannotConvert, syntax.Location, expr.ResultType, type);

            return new BoundConversionExpression(type, expr, isTreeValid);
        }

        private TypeSymbol BindType(TypeSyntax syntax)
        {
            if (syntax is PreDefinedTypeSyntax preDefinedType)
            {
                var type = BindFacts.GetTypeSymbol(preDefinedType.TypeToken.TokenKind);
                return type;
            }
            else if (syntax is ArrayTypeSyntax arrayType)
            {
                var underlying = BindType(arrayType.UnderlyingType);
                return new ArrayTypeSymbol(underlying);
            }
            else throw new Exception($"Unexpected type {syntax.GetType()}");
        }
    }
}
