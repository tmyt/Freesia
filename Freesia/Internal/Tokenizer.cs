using System;
using System.Collections.Generic;
using System.Linq;
using Freesia.Types;

namespace Freesia.Internal
{
    internal class Tokenizer
    {
        private string _text;
        private int _index;

        private void SkipWhile(params char[] chars)
        {
            while (_index < _text.Length)
            {
                if (!chars.Contains(_text[_index])) break;
                _index++;
            }
        }

        private char LexChar()
        {
            if (_index >= _text.Length) return (char)0;
            return _text[_index++];
        }

        private char PeekChar()
        {
            if (_index >= _text.Length) return (char)0;
            return _text[_index];
        }

        private char LexChars(params char[] chars)
        {
            var c = PeekChars(chars);
            if (c != 0) _index++;
            return c;
        }

        private char PeekChars(params char[] chars)
        {
            if (_index >= _text.Length) return (char)0;
            return chars.Contains(_text[_index]) ? _text[_index] : (char)0;
        }

        private string TakeWhile(params char[] chars)
        {
            var buffer = "";
            while (_index < _text.Length)
            {
                if (chars.Contains(_text[_index])) break;
                buffer += _text[_index++];
            }
            return buffer;
        }

        private TokenType DeterminTokenType(string str)
        {
            var chars = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', '-' };
            var signed = str[0] == '-';
            var point = str.Contains(".");
            var mayDigits = true;
            for (var i = 0; i < str.Length; ++i)
            {
                mayDigits &= chars.Contains(str[i]);
            }
            if (!mayDigits)
            {
                var s = str.ToLowerInvariant();
                return (s == "true" || s == "false") ? TokenType.Bool : (s == "null" ? TokenType.Null : TokenType.Symbol);
            }
            if (point) return TokenType.Double;
            return signed ? TokenType.Long : TokenType.ULong;
        }

        public IEnumerable<CompilerToken> Parse(bool errorRecovery = false)
        {
            _index = 0;

            var quote = false;
            var qchar = default(char);
            var buffer = "";
            var start = _index;

            while (_index < _text.Length)
            {
                // read chars in quoted
                while (quote)
                {
                    var c = LexChar();
                    if (c == '\0')
                    {
                        if (!errorRecovery) throw new ParseException("Unexpected string termination.", _index - 1);
                        // recovery error
                        c = qchar;
                    }
                    if (c == qchar)
                    {
                        yield return new CompilerToken { Type = TokenType.String, Value = buffer, Position = start, Length = _index - start };
                        buffer = "";
                        quote = false;
                        break;
                    }
                    if (c == '\\')
                    {
                        var d = PeekChar();
                        if (d == qchar)
                        {
                            buffer += qchar;
                            LexChar();
                        }
                        else if (d == '\\')
                        {
                            buffer += '\\';
                            LexChar();
                        }
                        continue;
                    }
                    buffer += c;
                }
                SkipWhile(' ', '\t', '\n', '\r');
                start = _index;
                char current;
                switch ((current = LexChar()))
                {
                    // EOF
                    case (char)0:
                        yield break;
                    // その他トークン
                    case '(':
                        yield return new CompilerToken { Type = TokenType.OpenBracket, Position = start, Length = 1 };
                        break;
                    case ')':
                        yield return new CompilerToken { Type = TokenType.CloseBracket, Position = start, Length = 1 };
                        break;
                    case '{':
                        yield return new CompilerToken { Type = TokenType.ArrayStart, Position = start, Length = 1 };
                        break;
                    case '}':
                        yield return new CompilerToken { Type = TokenType.ArrayEnd, Position = start, Length = 1 };
                        break;
                    case ',':
                        yield return new CompilerToken { Type = TokenType.ArrayDelimiter, Position = start, Length = 1 };
                        break;
                    case '.':
                        yield return new CompilerToken { Type = TokenType.PropertyAccess, Position = start, Length = 1 };
                        break;
                    case '=':
                        switch (LexChars('~', '=', '@', '>'))
                        {
                            case '~':
                                yield return new CompilerToken { Type = TokenType.Regexp, Position = start, Length = 2 };
                                break;
                            case '=':
                                if (LexChars('i') == 'i')
                                    yield return new CompilerToken { Type = TokenType.EqualsI, Position = start, Length = 2 };
                                else
                                    yield return new CompilerToken { Type = TokenType.Equals, Position = start, Length = 2 };
                                break;
                            case '@':
                                if (LexChars('i') == 'i')
                                    yield return new CompilerToken { Type = TokenType.ContainsI, Position = start, Length = 2 };
                                else
                                    yield return new CompilerToken { Type = TokenType.Contains, Position = start, Length = 2 };
                                break;
                            case '>':
                                yield return new CompilerToken { Type = TokenType.Lambda, Position = start, Length = 2 };
                                break;
                            default:
                                if (!errorRecovery) throw new ParseException("Must be '=', '~' or '@' after '='.", _index);
                                yield return new CompilerToken { Type = TokenType.Symbol, Value = "=", Position = start, Length = 1 };
                                break;
                        }
                        break;
                    case '!':
                        switch (LexChars('=', '~'))
                        {
                            case '=':
                                switch (LexChars('@', 'i'))
                                {
                                    case 'i':
                                        yield return new CompilerToken { Type = TokenType.NotEqualsI, Position = start, Length = 3 };
                                        break;
                                    case '@':
                                        if (LexChars('i') == 'i')
                                            yield return new CompilerToken { Type = TokenType.NotContainsI, Position = start, Length = 4 };
                                        else
                                            yield return new CompilerToken { Type = TokenType.NotContains, Position = start, Length = 3 };
                                        break;
                                    default:
                                        yield return new CompilerToken { Type = TokenType.NotEquals, Position = start, Length = 2 };
                                        break;
                                }
                                break;
                            case '~':
                                yield return new CompilerToken { Type = TokenType.NotRegexp, Position = start, Length = 2 };
                                break;
                            default:
                                yield return new CompilerToken { Type = TokenType.Not, Position = start, Length = 1 };
                                break;
                        }
                        break;
                    case '&':
                        if (LexChars('&') != 0)
                            yield return new CompilerToken { Type = TokenType.And, Position = start, Length = 2 };
                        else
                        {
                            if (!errorRecovery) throw new ParseException("Must be '&' after '&'.", _index);
                            yield return new CompilerToken { Type = TokenType.Symbol, Value = "&", Position = start, Length = 1 };
                        }
                        break;
                    case '|':
                        if (LexChars('|') != 0)
                            yield return new CompilerToken { Type = TokenType.Or, Position = start, Length = 2 };
                        else
                        {
                            if (!errorRecovery) throw new ParseException("Must be '|' after '|'.", _index);
                            yield return new CompilerToken { Type = TokenType.Symbol, Value = "|", Position = start, Length = 1 };
                        }
                        break;
                    case '"':
                        quote = true;
                        qchar = '"';
                        break;
                    case '\'':
                        quote = true;
                        qchar = '\'';
                        break;
                    case '>':
                        if (LexChars('=') != 0)
                            yield return new CompilerToken { Type = TokenType.GreaterThanEquals, Position = start, Length = 2 };
                        else
                            yield return new CompilerToken { Type = TokenType.GreaterThan, Position = start, Length = 1 };
                        break;
                    case '<':
                        if (LexChars('=') != 0)
                            yield return new CompilerToken { Type = TokenType.LessThanEquals, Position = start, Length = 2 };
                        else
                            yield return new CompilerToken { Type = TokenType.LessThan, Position = start, Length = 1 };
                        break;
                    // 文字列とか数字とかそのへん
                    default:
                        var str = "" + current;
                        switch (DeterminTokenType(str))
                        {
                            case TokenType.Symbol:
                                str += TakeWhile('(', ')', '{', '}', ',', '.', '=', '!', '&', '|', '"', '\'', '>', '<', ' ', '\t', '\r', '\n');
                                break;
                            case TokenType.Double:
                            case TokenType.Long:
                            case TokenType.ULong:
                                str += TakeWhile('(', ')', '{', '}', ',', '=', '!', '&', '|', '"', '\'', '>', '<', ' ', '\t', '\r', '\n');
                                break;
                        }
                        if (!String.IsNullOrEmpty(str))
                            yield return new CompilerToken { Type = DeterminTokenType(str), Value = str, Position = start, Length = _index - start };
                        break;
                }
            }
        }

        public Tokenizer(string text)
        {
            _text = text;
        }
    }
}
