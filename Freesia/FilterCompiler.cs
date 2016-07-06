﻿using System;
using System.Collections.Generic;
using System.Linq;
using Freesia.Internal;
using Freesia.Types;

namespace Freesia
{
    public class FilterCompiler<T>
    {
        internal FilterCompiler() { }

        public static IEnumerable<CompilerToken> Parse(string text)
        {
            return new Tokenizer(text).Parse(true);
        }

        public static Func<T, bool> Compile(IEnumerable<CompilerToken> tokenList)
        {
            var c = new ExpressionBuilder<T>();
            var ast = ASTBuilder.Generate(tokenList);
            return c.CompileSyntax(ast.FirstOrDefault());
        }

        public static Func<T, bool> Compile(string text)
        {
            var tokenizer = new Tokenizer(text);
            return Compile(tokenizer.Parse());
        }

        public static IEnumerable<SyntaxInfo> SyntaxHighlight(IEnumerable<CompilerToken> tokenList)
        {
            return SyntaxHighlighter<T>.SyntaxHighlight(tokenList);
        }

        public static IEnumerable<SyntaxInfo> SyntaxHighlight(string text)
        {
            var c = new Tokenizer(text);
            return SyntaxHighlight(c.Parse(true));
        }

        public static IEnumerable<string> Completion(string text, out string prefix)
        {
            return CodeCompletion<T>.Completion(text, out prefix);
        }
    }
}
