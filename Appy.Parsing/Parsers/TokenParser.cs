using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Appy.Parsing.Parsers
{
    public class TokenParser
    {
        static readonly Regex _wordIndexRegex = new Regex("\\w+\\((?<digits>\\d+)\\)", RegexOptions.Compiled);
        readonly Func<object[], object> _combine;
        readonly string _rawPattern;
        const int MaximumIterations = 100;

        public TokenParser(Func<object[], object> combine, string rawPattern)
        {
            _combine = combine;
            _rawPattern = rawPattern;
        }
        public TokenParser(Func<object[], object> combine, params Type[] patternTypes) : this(combine, string.Join(" ", patternTypes.Select(GetIdentifier))) {}

        public static string GetIdentifier(Type type) => 
            (type.IsGenericType
                ? type.FullName.Split('`')[0] + "Of" + string.Join("-", type.GetGenericArguments().Select(GetIdentifier).ToArray())
                : type.FullName).Replace("[", "").Replace("]", "s").Replace(".", "").Replace("+", "").Replace("-", "");


        public object[] Parse(object[] parts)
        {
            if (parts.Any(p => p is string))
                throw new ArgumentException("Unexpected sentence format. Unrecognized tokens: " + string.Join(" ", parts.Where(p => p is string)));

            for (var i = 0; i < MaximumIterations; i++)
            {
                var newParts = innerCombine(parts);

                if (newParts == parts)
                    return parts;

                parts = newParts;
            }

            return parts;
        }

        object[] innerCombine(object[] parts)
        {
            var matchableSentence = patterniseSentence(parts);
            var pattern = patterniseRegex(_rawPattern);

            var match = Regex.Match(matchableSentence, pattern);
            if (!match.Success)
                return parts;

            var ordinals = getOrdinals(match.Value);
        
            var capture = parts.Where((p, i) => ordinals.Contains(i))
                               .ToArray();

            var result = new []{ _combine(capture) };
            // TODO: short circuit is not working
            if (result == capture)
                return parts;

            var start = ordinals.Min();
            var end = ordinals.Max();

            var tail = parts.Skip(end + 1)
                            .Take(parts.Length - end)
                            .ToArray();

            return parts.Take(start)
                        .Concat(result)
                        .Concat(tail)
                        .ToArray();
        }

         static string patterniseSentence(IEnumerable<object> parts) =>
            string.Join(" ", parts.Select(p => p.GetType())
                                   .Select(GetIdentifier)
                                   .Select((identifier, index) => $"{identifier}({index})"));

        static string patterniseRegex(string input) => 
            string.IsNullOrEmpty(input) 
                ? throw new ApplicationException("Bad Expression")
                : string.Join("", getTokens(input).Select(t => t.Expression));

        static int[] getOrdinals(string input) => 
            input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                  .Select(p =>
                  {
                      var match = _wordIndexRegex.Match(p);
                      if (!match.Success)
                          throw new ApplicationException("Unhandled Match: " + p);
                      return int.Parse(match.Groups["digits"].Value);
                  }).ToArray();

        static IEnumerable<Token> getTokens(string expression)
        {
            var groupCounter = 0;
            var tokens = new List<Token>();

            for (var i = 0; i < expression.Length; i++)
            {
                var wordMatch = WordToken.Regex.Match(expression, i);
                var repeaterMatch = RepeaterToken.Regex.Match(expression, i);
                var orMatch = OrToken.Regex.Match(expression, i);
                var openGroupMatch = OpenGroupToken.Regex.Match(expression, i);
                var closeGroupMatch = CloseGroupToken.Regex.Match(expression, i);
                var whiteSpaceMatch = WhiteSpaceToken.Regex.Match(expression, i);

                if (wordMatch.Success)
                {
                    tokens.Add(WordToken.FromMatch(wordMatch));
                    i = i + (wordMatch.Length - 1);
                }
                else if (repeaterMatch.Success)
                {
                    tokens.Add(RepeaterToken.FromMatch(repeaterMatch));
                    i = i + (repeaterMatch.Length - 1);
                }
                else if (orMatch.Success)
                {
                    tokens.Add(new OrToken());
                    i = i + (orMatch.Length - 1);
                }
                else if (openGroupMatch.Success)
                {
                    groupCounter++;
                    tokens.Add(new OpenGroupToken());
                    i = i + (openGroupMatch.Length - 1);
                }
                else if (closeGroupMatch.Success)
                {
                    groupCounter--;
                    tokens.Add(new CloseGroupToken());
                    i = i + (closeGroupMatch.Length - 1);
                }
                else if (whiteSpaceMatch.Success)
                {
                    i = i + (whiteSpaceMatch.Length - 1);
                }
                else
                    throw new ApplicationException($"Unhandled Token at {i} in text {expression}");
            }

            if (groupCounter != 0)
                throw new ApplicationException("Invalid pattern, unmatched braces.");

            return tokens;
        }
    }
}
