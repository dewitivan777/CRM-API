using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ApiGateway.Extentions
{
    /// <summary>
    /// Collection of string extension methods
    /// </summary>
    public static class StringExtentions
    {
        private static readonly Regex _mobileNumber = new Regex(@"^(\+?27|0)[6-9][1-9][0-9]{7}$", RegexOptions.Compiled);
        private static readonly Regex _whitespace = new Regex(@"\s+", RegexOptions.Compiled);
        private static readonly Regex _invalidCharacters = new Regex(@"[^a-z0-9\s-]", RegexOptions.Compiled);
        private static readonly Regex _multipleHyphens = new Regex(@"([-]){2,}", RegexOptions.Compiled);

        /// <summary>
        /// Validate mobile number
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsValidMobileNumber(this string input)
        {
            var match = _mobileNumber.Match(input);

            return match.Success;
        }

        /// <summary>
        /// Base 64 encode
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Reverse string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Reverse(this string input)
        {
            char[] charArray = input.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        /// <summary>
        /// Split the string in half
        /// </summary>
        /// <param name="input"></param>
        /// <param name="firstHalfBigger"></param>
        /// <returns></returns>
        public static string[] SplitInHalf(this string input, bool firstHalfBigger = false)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            var length = input.Length;

            string firstHalf;
            string secondHalf;

            if (firstHalfBigger)
            {
                var half = (int)Math.Ceiling(length / 2.0);

                firstHalf = input.Substring(0, half);
                secondHalf = input.Substring(half, length - half);
            }
            else
            {
                firstHalf = input.Substring(0, length / 2);
                secondHalf = input.Substring(length / 2, length - (length / 2));
            }

            return new string[]
            {
                firstHalf,
                secondHalf
            };
        }

        /// <summary>
        /// Base 64 decode
        /// </summary>
        /// <param name="base64EncodedData"></param>
        /// <returns></returns>
        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToUppercaseFirst(this string text)
        {
            if (string.IsNullOrWhiteSpace(text) || char.IsUpper(text[0]))
            {
                return text;
            }

            char[] a = text.ToCharArray();
            a[0] = char.ToUpperInvariant(a[0]);

            return new string(a);
        }

        public static string ToUrlSlug(this string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase)) return string.Empty;

            //Special case for keywords. Keywords should keep original formatting
            if (phrase.StartsWith("q-"))
            {
                return Uri.EscapeDataString(phrase.Trim());
            }

            // strip diacritics
            // and lower case
            var urlSlug = phrase
                .RemoveDiacritics()
                .ToLower();

            // replace spaces
            urlSlug = urlSlug.ReplaceWhitespace("-");

            // remove invalid chars
            urlSlug = urlSlug.ReplaceInvalidCharacters("-");

            // trim dashes from end
            urlSlug = urlSlug.Trim('-');

            // replace double occurences of -
            urlSlug = urlSlug.ReplaceMultipleHyphens("$1");

            return urlSlug;
        }

        public static string RemoveDiacritics(this string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string ReplaceWhitespace(this string input, string replacement)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            return _whitespace.Replace(input, replacement);
        }

        public static string ReplaceInvalidCharacters(this string input, string replacement)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            return _invalidCharacters.Replace(input, replacement);
        }

        public static string ReplaceMultipleHyphens(this string input, string replacement)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            return _multipleHyphens.Replace(input, replacement);
        }
    }
}
