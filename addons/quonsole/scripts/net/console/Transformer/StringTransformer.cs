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

using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using Godot;
using Quonsole.Interfaces;

namespace Quonsole.Transformer;

public class StringTransformer : IStringTransformer
{
    private static Regex _variableRegex =
        new Regex(@"(?<!\$)\$\{(?<name>[a-zA-Z_][a-zA-Z0-9_]*)\}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    private static Regex _gdExpressionRegex =
        new Regex(@"(?<!\$)\$\((?<expression>(?:[^\)]|\\\))*)(?<!\\)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    public ICommandRepository CommandRepository { get; private set; }

    public IStringTransformer Previous { get; private set; }

    public bool AllowEscape { get; set; } = true;
    public bool AllowVariables { get; set; } = true;
    public bool AllowGdExpressions { get; set; } = true;

    public StringTransformer(ICommandRepository commandRepository, IStringTransformer previous = null)
    {
        CommandRepository = commandRepository;
        Previous = previous;
    }

    public string Transform(string input)
    {
        var output = input;

        output = Previous?.Transform(output) ?? output;
        output = AllowVariables ? InsertVariables(output) : output;
        output = AllowGdExpressions ? InsertGdExpressions(output) : output;
        output = AllowEscape ? Unescape(output) : output;

        return output;
    }

    private string Escape(string input)
    {
        StringBuilder sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(input))
        {
            foreach (char c in input)
            {
                if (c == '\n')
                {
                    sb.Append("\\n");
                }
                else if (c == '\r')
                {
                    sb.Append("\\r");
                }
                else if (c == '\t')
                {
                    sb.Append("\\t");
                }
                else if (c == '$')
                {
                    sb.Append("$$");
                }
                else if (c == ')')
                {
                    sb.Append("\\)");
                }
                else
                {
                    sb.Append(c);
                }
            }
        }
        else
        {
            sb.Append(input ?? string.Empty);
        }

        return sb.ToString();
    }

    private string Unescape(string input)
    {
        StringBuilder sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(input))
        {
            var escapeChar = '\0';

            foreach (char c in input)
            {
                if (escapeChar == '\\')
                {
                    if (c == 'n')
                    {
                        sb.Append('\n');
                    }
                    else if (c == 'r')
                    {
                        sb.Append('\r');
                    }
                    else if (c == 't')
                    {
                        sb.Append('\t');
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    escapeChar = '\0';
                }
                else if (c == '\\')
                {
                    escapeChar = '\\';
                }
                else if (escapeChar == '$')
                {
                    sb.Append('$');

                    if (c != '$')
                    {
                        sb.Append(c);
                    }

                    escapeChar = '\0';
                }
                else if (c == '$')
                {
                    escapeChar = '$';
                }
                else
                {
                    sb.Append(c);
                }
            }
        }
        else
        {
            sb.Append(input ?? string.Empty);
        }

        return sb.ToString();
    }

    private string InsertVariables(string input)
    {
        return _variableRegex.Replace(input, (m) =>
        {
            var name = Unescape(m.Groups["name"].Value);

            var variable = CommandRepository.GetVariable(name);

            return Transform(variable?.Get().AsString() ?? string.Empty);
        });
    }

    private string InsertGdExpressions(string input)
    {
        return _gdExpressionRegex.Replace(input, (m) =>
        {
            var expression = Unescape(m.Groups["expression"].Value);

            var gdExpr = new Godot.Expression();
            gdExpr.Parse(expression);

            var gdResult = gdExpr.Execute();

            if (gdExpr.HasExecuteFailed())
            {
                gdResult = Variant.CreateFrom(string.Empty);
            }

            return Transform(gdResult.AsString() ?? string.Empty);
        });
    }
}
