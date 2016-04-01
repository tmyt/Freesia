using System;
using System.Collections.Generic;
using Freesia.Internal;
using Freesia.Types;

namespace Freesia
{
    public class FilterCompiler<T>
    {
        protected class UserFunctionTypePlaceholder { }

        private static Dictionary<string, Func<T, bool>> _functions;
        public static Dictionary<string, Func<T, bool>> Functions => _functions ?? (_functions = new Dictionary<string, Func<T, bool>>());

        public static string UserFunctionNamespace { get; set; }

        internal FilterCompiler() { }

        public static IEnumerable<CompilerToken> Parse(string text)
        {
            return new Tokenizer(text).Parse(true);
        }

        public static Func<T, bool> Compile(IEnumerable<CompilerToken> tokenList)
        {
            var c = new ExpressionBuilder<T>();
            var ast = ASTBuilder.Generate(tokenList);
            return c.CompileSyntax(ast);
        }

        public static Func<T, bool> Compile(string text)
        {
            var tokenizer = new Tokenizer(text);
            return Compile(tokenizer.Parse());
        }

        public static IEnumerable<SyntaxInfo> SyntaxHighlight(IEnumerable<CompilerToken> tokenList)
        {
            return SyntaxHighlighter<T>.SyntaxHighlightInternal(tokenList);
        }

        public static IEnumerable<SyntaxInfo> SyntaxHighlight(string text)
        {
            var c = new Tokenizer(text);
            return SyntaxHighlight(c.Parse(true));
        }

        public static IEnumerable<string> Completion(string text, out string prefix)
        {
            return CodeCompletion<T>.CompletionInternal(text, out prefix);
        }
    }
}
