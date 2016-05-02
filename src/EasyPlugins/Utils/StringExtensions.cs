using System;
using System.Linq;
using System.Text;

namespace EasyPlugins.Utils
{
    public static class StringExtensions
    {
        public static string NormalizeCarriageReturns(this string str)
        {
            return str?.Replace("\r\n", "\n").Replace("\n", Environment.NewLine) ?? string.Empty;
        }

        public static string ToAlphaNumeric(this string src, char? replacementCharacter = null, params char[] allowTheseCharactersAsWell)
        {
            var sb = new StringBuilder();
            char? lastChar = null;
            foreach (var c in src.ToCharArray())
            {
                if (char.IsLetterOrDigit(c) || allowTheseCharactersAsWell.Contains(c))
                {
                    sb.Append(c);
                    lastChar = c;
                }
                else if (replacementCharacter.HasValue && (!lastChar.HasValue || lastChar.Value != replacementCharacter.Value))
                {
                    sb.Append(replacementCharacter.Value);
                    lastChar = replacementCharacter.Value;
                }
            }
            return sb.ToString();
        }
    }
}
