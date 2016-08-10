using System;
using System.Collections.Generic;
using System.Linq;
using Freesia.Internal;
using Freesia.Internal.Types;
using Freesia.Types;

namespace Freesia
{
    public static class FilterCompiler
    {
        public static IEnumerable<IASTNode> Parse(string text)
        {
            return ASTBuilder.Generate(new Tokenizer(text).Parse(true));
        }

        public static Func<T, bool> Compile<T>(IEnumerable<IASTNode> ast)
        {
            var c = new ExpressionBuilder<T>();
            return c.CompileSyntax((ASTNode)ast.FirstOrDefault());
        }

        public static Func<T, bool> Compile<T>(string text)
        {
            return Compile<T>(Parse(text));
        }

        public static IEnumerable<SyntaxInfo> SyntaxHighlight<T>(IEnumerable<IASTNode> ast)
        {
            return SyntaxHighlighter<T>.SyntaxHighlight(ast);
        }

        public static IEnumerable<SyntaxInfo> SyntaxHighlight<T>(string text)
        {
            return SyntaxHighlight<T>(Parse(text));
        }

        public static IEnumerable<string> Completion<T>(string text, out string prefix)
        {
            return CodeCompletion<T>.Completion(text, out prefix);
        }
    }
}
