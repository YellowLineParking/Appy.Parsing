using System;
using System.Text.RegularExpressions;

namespace Appy.Parsing.Lexers
{
    public class Tokenizer
    {
        readonly string _groupName;
        readonly Func<Match, object> _tokenize;

        public Tokenizer(string groupName, string regex, Func<Match, object> tokenize)
        {
            _groupName = groupName;
            _tokenize = tokenize;
            RegexBody = $"(?<{groupName}>{regex})";
        }

        public string RegexBody { get; }

        public bool IsMatch(Match match) =>
            match.Success && match.Groups[_groupName].Success;

        public object GetToken(Match match) => 
            IsMatch(match) 
            ? _tokenize(match)
            : (object) null;
    }
}
