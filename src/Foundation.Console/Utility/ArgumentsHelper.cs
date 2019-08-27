﻿namespace Naos.Foundation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class ArgumentsHelper
    {
        public static string[] Split(string line)
        {
            if (line.IsNullOrEmpty())
            {
                return Enumerable.Empty<string>().ToArray();
            }

            line = line.Replace("\0", string.Empty);
            var result = new List<string>();
            var currentArgument = new StringBuilder();
            var currentQuote = char.MinValue;

            void Reset()
            {
                result.Add(currentArgument.ToString());
                currentArgument = new StringBuilder();
                currentQuote = char.MinValue;
            }

            foreach (var c in line)
            {
                if (currentQuote == char.MinValue)
                {
                    if (c == ' ')
                    {
                        Reset();
                    }
                    else if (c == '\'')
                    {
                        Reset();
                        currentQuote = '\'';
                    }
                    else if (c == '"')
                    {
                        Reset();
                        currentQuote = '"';
                    }
                    else
                    {
                        currentArgument.Append(c);
                    }
                }
                else
                {
                    if (c == currentQuote)
                    {
                        Reset();
                    }
                    else
                    {
                        currentArgument.Append(c);
                    }
                }
            }

            Reset();
            return result.Where(a => !string.IsNullOrWhiteSpace(a)).ToArray();
        }
    }
}
