using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Compiler.Binding;
using Compiler.Diagnostics;
using Compiler.Text;

namespace Compiler.Syntax
{
    // public sealed class SyntaxTree
    // {
    //     internal CompilationUnitSyntax Root { get; }

    //     private SyntaxTree(CompilationUnitSyntax root)
    //     {
    //         Text = text;
    //         diagnostics = new DiagnosticBag();
    //         var parser = new Parser(text);
    //         Root = parser.ParseCompilationUnit();
    //     }

    //     internal ImmutableArray<Diagnostic> GetDiagnostics()
    //     {
    //         return diagnostics.ToImmutable();
    //     }

    //     public static SyntaxTree ParseSyntaxTree(SourceText text)
    //     {
    //         return new SyntaxTree(text);
    //     }

    //     public static ImmutableArray<SyntaxToken> ParseTokens(SourceText text)
    //     {
    //         var bag = new DiagnosticBag();
    //         return new Lexer(text).Tokenize().ToImmutableArray();
    //     }

    //     public static ConsoleColor GetTokenColor(SyntaxToken token)
    //     {
    //         switch (token.Kind)
    //         {
                
    //             case SyntaxTokenKind.Int:
    //             case SyntaxTokenKind.Float:
    //                 return ConsoleColor.Green;
    //             case SyntaxTokenKind.Identifier:
    //                 return ConsoleColor.Cyan;
    //             case SyntaxTokenKind.BoolKeyword:
    //             case SyntaxTokenKind.IntKeyword:
    //             case SyntaxTokenKind.FloatKeyword:
    //             case SyntaxTokenKind.StringKeyword:
    //             case SyntaxTokenKind.Var:
    //             case SyntaxTokenKind.True:
    //             case SyntaxTokenKind.False:
    //             case SyntaxTokenKind.Null:
    //                 return ConsoleColor.Blue;
    //             case SyntaxTokenKind.IfKeyword:
    //             case SyntaxTokenKind.ElseKeyword:
    //                 return ConsoleColor.Magenta;
    //             default: return ConsoleColor.Gray;

    //         }
    //     }

    //     public override string ToString() => Root.ToString();

    // }
}