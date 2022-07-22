using System;
using System.Collections.Generic;
using System.Linq;
using Appy.Parsing.Lexers;

namespace Appy.Parsing.Parsers
{
    /// <summary>
    /// Creates an object from an expression
    /// </summary>
    public class Parser
    {
        readonly Lexer _lexer;
        readonly IEnumerable<TokenParser> _tokenParsers;

        /// <param name="lexer">The lexer to use to convert the expression to tokens</param>
        /// <param name="tokenParsers">The parsers to use to convert the tokens into objects</param>
        public Parser(Lexer lexer, IEnumerable<TokenParser> tokenParsers)
        {
            _lexer = lexer;
            _tokenParsers = tokenParsers;
        }

        /// <summary>
        ///  Converts an expression into an object array
        /// </summary>
        /// <param name="expression">The expression to evaluate</param>
        /// <returns></returns>
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

        /// <summary>
        ///  Converts an expression into an object
        /// </summary>
        /// <param name="expression">The expression to evaluate</param>
        /// <param name="expression"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException">Throws an exception when multiple are found are when the found item is not of type <see cref="T"/></exception>
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