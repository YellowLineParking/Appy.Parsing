using System;
using System.Text.RegularExpressions;

namespace Appy.Parsing.Lexers
{
    /// <summary>
    /// Creates a token based on a regular expression and a callback function to convert the text into a token
    /// </summary>
    public class Tokenizer
    {
        readonly string _groupName;
        readonly Func<Match, object> _tokenize;

        
        /// <param name="groupName">The group to match</param>
        /// <param name="regex">The regular expression to run against the expression</param>
        /// <param name="tokenize">A function that creates a token from a <see cref="Match"/></param>
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
