using System;
using System.Collections.Generic;
using System.Linq;
using Freesia.Internal;
using Freesia.Internal.Types;
using Freesia.Types;

namespace Freesia
{
    public class FilterCompiler<T>
    {
        internal FilterCompiler() { }

        public static IEnumerable<IASTNode> Parse(string text)
        {
            return ASTBuilder.Generate(new Tokenizer(text).Parse(true));
        }

        public static Func<T, bool> Compile(IEnumerable<IASTNode> ast)
        {
            var c = new ExpressionBuilder<T>();
            return c.CompileSyntax((ASTNode)ast.FirstOrDefault());
        }

        public static Func<T, bool> Compile(string text)
        {
            return Compile(Parse(text));
        }

        public static IEnumerable<SyntaxInfo> SyntaxHighlight(IEnumerable<IASTNode> ast)
        {
            return SyntaxHighlighter<T>.SyntaxHighlight(ast);
        }

        public static IEnumerable<SyntaxInfo> SyntaxHighlight(string text)
        {
            return SyntaxHighlight(Parse(text));
        }

        public static IEnumerable<string> Completion(string text, out string prefix)
        {
            return CodeCompletion<T>.Completion(text, out prefix);
        }
    }
}
