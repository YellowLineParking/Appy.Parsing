using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Appy.Parsing.Lexers;

namespace Appy.Parsing.Builder
{
    public class LexerBuilder
    {
        readonly List<Tokenizer> _tokenizers = new List<Tokenizer>();
        bool _ignoreUnmatched = false;
        public static LexerBuilder Build() =>
            new LexerBuilder();

        public Lexer Create() =>
            new Lexer(_tokenizers, _ignoreUnmatched);

        public LexerBuilder CombineWith(LexerBuilder other)
        {
            _tokenizers.AddRange(other._tokenizers);
            return this;
        }

        public LexerBuilder Match(string groupName, string expression, Func<Match, object> tokenize)
        {
            _tokenizers.Add(new Tokenizer(groupName, expression, tokenize));
            return this;
        }

        public LexerBuilder Ignore(string groupName, string expression)
        {
            _tokenizers.Add(new Tokenizer(groupName, expression, match => null));
            return this;
        }

        public LexerBuilder IgnoreUnmatched()
        {
            _ignoreUnmatched = true;
            return this;
        }
        public LexerBuilder Match<T>(string groupName, string expression, T value) => 
            Match(groupName, expression, match => value);

        public LexerBuilder Match<T>(string groupName, string expression) where T : new() => 
            Match(groupName, expression, match => new T());

        public LexerBuilder Match<T>() where T : new()
        {
            var type = typeof(T);
            var groupName = type.Name.ToLower();

            var attributes = (MatchAttribute[]) type.GetCustomAttributes(typeof(MatchAttribute), false);
            var expression = attributes.FirstOrDefault()?.Expression ?? groupName;
            return Match(groupName, expression, match => new T());
        }

        public LexerBuilder MatchEnum<T>() where T : struct, IConvertible
        {
            var enumType = typeof(T);
            if (!enumType.IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            foreach(var e in Enum.GetValues(enumType))
            {
                var name = Enum.GetName(enumType, e);

                var fi = e.GetType().GetField(e.ToString());
                var attributes = (MatchAttribute[])fi.GetCustomAttributes(typeof(MatchAttribute), false);
                if (attributes != null && attributes.Any())
                {
                    Match(name, attributes.First().Expression, (T) Enum.Parse(enumType, name, true));
                }
                else
                {
                    Match(name, $@"\b{name}\b", (T) Enum.Parse(enumType, name, true));
                }
            }

            return this;
        }

        public LexerBuilder MatchDictionary<T>(string groupName, Dictionary<string, T> dictionary) => 
            Match(groupName, string.Join("|", dictionary.Keys.Select(key => $@"\b{key}\b")), match => dictionary[match.Value]);
    }
}
