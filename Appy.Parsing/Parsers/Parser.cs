using System;
using System.Collections.Generic;
using System.Linq;
using Appy.Parsing.Lexers;

namespace Appy.Parsing.Parsers
{
    public class Parser
    {
        readonly Lexer _lexer;
        readonly IEnumerable<TokenParser> _tokenParsers;

        public Parser(Lexer lexer, IEnumerable<TokenParser> tokenParsers)
        {
            _lexer = lexer;
            _tokenParsers = tokenParsers;
        }

        public object[] Parse(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return null;

            var tokens = Tokenize(expression);
            return Parse(tokens);
        }

        public object[] Tokenize(string expression) =>
            _lexer.Tokenize(expression);

        public object[] Parse(object[] tokens) =>
            _tokenParsers.Aggregate(tokens, (current, p) => p.Parse(current));

        public T Parse<T>(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return default(T);

            var tokens = Tokenize(expression);
            var result = Parse(tokens);
            if (result.Length > 1 || !(result.Single() is T))
                throw new ApplicationException($"Could not parse expression {expression} to item of type {typeof(T).Name}. Tokens: {string.Join(" ", result.Select(item => item.GetType().Name))}");

            return (T)result.Single();
        }
    }

    public class Parser<T> : Parser
    {
        public Parser(Lexer lexer, IEnumerable<TokenParser> tokenParsers) 
            : base(lexer, tokenParsers)
        { }

        public new T Parse(string expression) => 
            Parse<T>(expression);
    }
}