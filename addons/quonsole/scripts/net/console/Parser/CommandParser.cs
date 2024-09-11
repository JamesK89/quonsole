/*
MIT License

Copyright (c) 2024 James Kelly

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Text;
using Quonsole.Exceptions;

namespace Quonsole.Parser;

internal class CommandParser
{
    private enum TokenType
    {
        Normal,
        Quoted,
        Comment,
        Delimiter
    }

    private class Token
    {
        public TokenType Type
        {
            get;
            set;
        } = TokenType.Normal;

        public string Value
        {
            get;
            set;
        } = string.Empty;
    }

    private class TokenizerContext
    {
        public string Input
        {
            get;
            set;
        }

        public int Index
        {
            get;
            set;
        }

        public char Current
        {
            get => Index < Input.Length ? Input[Index] : '\0';
        }

        public IList<Token> Tokens
        {
            get;
            set;
        }
    }

    public bool SingleQuotes
    {
        get;
        private set;
    }

    public bool DoubleQuotes
    {
        get;
        private set;
    }

    public CommandParser()
    {
    }

    private IReadOnlyList<Token> Tokenize(string input)
    {
        var context = new TokenizerContext()
        {
            Input = input,
            Index = 0,
            Tokens = new List<Token>(),
        };

        while (context.Index < input.Length)
        {
            if (IsComment(context.Current))
            {
                ReadComment(context);
            }
            if (IsWhiteSpace(context.Current))
            {
                ReadWhiteSpace(context);
            }
            else if (IsDelimiter(context.Current))
            {
                ReadDelimiter(context);
            }
            else if (IsQuote(context.Current))
            {
                ReadQuoted(context);
            }
            else if (IsNormal(context.Current))
            {
                ReadNormal(context);
            }
        }

        return (IReadOnlyList<Token>)context.Tokens;
    }

    public IReadOnlyList<string[]> Parse(string input)
    {
        var tokens = Tokenize(input);
        var result = new List<string[]>();
        var current = new List<string>();

        foreach (var token in tokens)
        {
            switch (token.Type)
            {
                case TokenType.Delimiter:
                    if (current.Count > 0)
                    {
                        result.Add(current.ToArray());
                        current.Clear();
                    }
                    break;
                case TokenType.Comment:
                    continue;
                case TokenType.Quoted:
                    current.Add(token.Value);
                    break;
                case TokenType.Normal:
                    current.Add(token.Value);
                    break;
            }
        }

        if (current.Count > 0)
        {
            result.Add(current.ToArray());
        }

        return result;
    }

    private string ReadUntil(TokenizerContext context, Func<char, bool> predicate)
    {
        var sb = new StringBuilder();

        while (context.Index < context.Input.Length)
        {
            if (predicate(context.Current))
            {
                break;
            }

            sb.Append(context.Current);
            context.Index++;
        }

        return sb.ToString();
    }

    private string ReadWhile(TokenizerContext context, Func<char, bool> predicate)
    {
        return ReadUntil(context, (c) => !predicate(c));
    }

    private bool IsWhiteSpace(char c)
    {
        return c == ' ' || c == '\t' || c == '\n' || c == '\r';
    }

    private void ReadWhiteSpace(TokenizerContext context)
    {
        ReadWhile(context, IsWhiteSpace);
    }

    private void ReadNormal(TokenizerContext context)
    {
        StringBuilder sb = new StringBuilder();

        while (context.Index < context.Input.Length)
        {
            if (IsEscape(context.Current))
            {
                context.Index++;

                if (context.Index >= context.Input.Length)
                    break;
            }
            else if (!IsNormal(context.Current))
            {
                break;
            }

            sb.Append(context.Current);
            context.Index++;
        }

        var token = new Token()
        {
            Type = TokenType.Normal,
            Value = sb.ToString()
        };

        context.Tokens.Add(token);
    }

    private bool IsNormal(char c)
    {
        return !IsWhiteSpace(c) &&
                !IsQuote(c) &&
                !IsDelimiter(c);
    }

    private void ReadQuoted(TokenizerContext context)
    {
        StringBuilder sb = new StringBuilder();

        var quoteChar = context.Current;

        context.Index++;

        while (context.Index <= context.Input.Length)
        {
            if (context.Index >= context.Input.Length)
            {
                throw new ParserException("Unterminated quoted string");
            }
            else if (context.Current == quoteChar)
            {
                context.Index++;
                break;
            }
            else if (IsEscape(context.Current))
            {
                sb.Append(context.Current);
                context.Index++;

                if (context.Index >= context.Input.Length)
                    break;
            }

            sb.Append(context.Current);
            context.Index++;
        }

        var token = new Token()
        {
            Type = TokenType.Quoted,
            Value = sb.ToString()
        };

        context.Tokens.Add(token);
    }

    private bool IsQuote(char c)
    {
        return c == '\'' || c == '"';
    }

    private bool IsEscape(char c)
    {
        return c == '\\';
    }

    private void ReadComment(TokenizerContext context)
    {
        var token = new Token()
        {
            Type = TokenType.Comment,
            Value = ReadUntil(context, IsDelimiter)
        };

        context.Tokens.Add(token);
    }

    private bool IsComment(char c)
    {
        return c == '#';
    }

    private void ReadDelimiter(TokenizerContext context)
    {
        var token = new Token()
        {
            Type = TokenType.Delimiter,
            Value = ReadWhile(context, IsDelimiter)
        };

        context.Tokens.Add(token);
    }

    private bool IsDelimiter(char c)
    {
        return c == ';';
    }
}
