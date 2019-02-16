using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Appy.Parsing.Lexers
{
    public class Lexer
    {
        readonly ConcurrentDictionary<string, object[]> _cache = new ConcurrentDictionary<string, object[]>();
        readonly IList<Tokenizer> _tokenizers;
        readonly bool _ignoreUnmatched;
        readonly Regex _regex;

        public Lexer(IList<Tokenizer> tokenizers, bool ignoreUnmatched)
        {
            _tokenizers = tokenizers;
            _ignoreUnmatched = ignoreUnmatched;
            _regex = new Regex($"({string.Join("|", _tokenizers.Select(x => x.RegexBody))})", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        }

        public object[] Tokenize(string expression)
        {
            if (_cache.ContainsKey(expression))
                return _cache[expression];

            var matches = _regex.Matches(expression);

            var workingIndex = 0;

            // Build up sentence
            var sentence = new List<object>();

            foreach (var match in matches.OfType<Match>())
            {
                //Here we are going to take any text between the last match and the current  and record is as unrecognised.
                if (match.Index != workingIndex && !_ignoreUnmatched)
                    populateUnmatchedPart(expression, workingIndex, sentence, match.Index - workingIndex);

                // Move the working index to the location of the current match.
                workingIndex = match.Index + match.Length;

                var token = extractSentencePart(match);
                if(token != null)
                    sentence.Add(token);
            }

            // If the working index is not at the end of the string, add the tail as unrecognised
            if (workingIndex != expression.Length && !_ignoreUnmatched)
            {
                var unmatchedLength = expression.Length - workingIndex;
                populateUnmatchedPart(expression, workingIndex, sentence, unmatchedLength);
            }

            _cache.TryAdd(expression, sentence.ToArray());

            return sentence.ToArray();
        }

        void populateUnmatchedPart(string expression, int workingIndex, List<object> sentence, int unmatchedLength)
        {
            var substring = expression.Substring(workingIndex, unmatchedLength)
                                      .Trim();

            if (!string.IsNullOrEmpty(substring))
                sentence.Add(substring);
        }

        object extractSentencePart(Match match)
        {
            var lexer = _tokenizers.FirstOrDefault(x => x.IsMatch(match));
            return lexer != null ? lexer.GetToken(match) : match.Value;
        }
    }
}