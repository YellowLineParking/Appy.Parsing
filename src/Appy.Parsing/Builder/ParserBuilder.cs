using System;
using System.Collections.Generic;
using System.Linq;
using Appy.Parsing.Lexers;
using Appy.Parsing.Parsers;

namespace Appy.Parsing.Builder
{
    public class ParserBuilder
    {
        readonly Lexer _lexer;
        readonly List<TokenParser> _tokenParsers = new List<TokenParser>();

        /// <summary>
        /// Starts the creation of a Parser Builder
        /// </summary>
        /// <param name="lexer">The lexer to use when building the parser</param>
        ParserBuilder(Lexer lexer) => 
            _lexer = lexer;

        /// <summary>
        /// Starts the creation of a Parser Builder
        /// </summary>
        /// <param name="lexer">The LexerBuilder to use to create a Lexer when building the parser</param>
        public static ParserBuilder Build(LexerBuilder lexerBuilder) =>
            new ParserBuilder(lexerBuilder.Create());

        /// <summary>
        /// Constructs a parser that parses expression into object of type T
        /// </summary>
        public Parser<T> Create<T>() =>
            new Parser<T>(_lexer, _tokenParsers);

        public Parser Create() =>
            new Parser(_lexer, _tokenParsers);

        ParserBuilder add(TokenParser parser)
        {
            _tokenParsers.Add(parser);
            return this;
        }

        /// <summary>
        ///  Converts a token of type T into a new object
        /// </summary>
        public ParserBuilder Match<T1>(Func<T1, object> create) => 
            add(new TokenParser(match => create((T1)match.First()), typeof(T1)));

        
        /// <summary> Creates a new object from two tokens </summary>
        /// <param name="options">Determine what strategy to use when matching tokens</param>
        /// <exception cref="ApplicationException"></exception>
        public ParserBuilder Match<T1, T2>(Func<T1, T2, object> create, MatchOptions options = MatchOptions.AllInOrder)
        {
            if(options == MatchOptions.AllInOrder)
                return add(new TokenParser(match => create((T1) match.First(), (T2) match.Skip(1).First()), typeof(T1), typeof(T2)));
            if(options == MatchOptions.AtLeastOne)
                return Match(create).Match<T1>(t1 => create(t1, default(T2)))
                                    .Match<T2>(t2 => create(default(T1), t2));
            if(options == MatchOptions.AnyOrder)
                return Match(create).Match<T2, T1>((t2, t1) => create(t1, t2));

            throw new ApplicationException("Unknown match options");
        }

        /// <summary> Creates a new object from three tokens </summary>
        /// <param name="options">Determine what strategy to use when matching tokens</param>
        /// <exception cref="ApplicationException"></exception>
        public ParserBuilder Match<T1, T2, T3>(Func<T1, T2, T3, object> create, MatchOptions options = MatchOptions.AllInOrder)
        {
            if(options == MatchOptions.AllInOrder)
                return add(new TokenParser(match => create((T1) match.First(), (T2) match.Skip(1).First(), (T3) match.Skip(2).First()), typeof(T1), typeof(T2), typeof(T3)));
            if(options == MatchOptions.AtLeastOne)
                return Match(create).Match<T1, T2>((t1, t2) => create(t1, t2, default(T3)))
                                    .Match<T1, T3>((t1, t3) => create(t1, default(T2), t3))
                                    .Match<T2, T3>((t2, t3) => create(default(T1), t2, t3))
                                    .Match<T1>(t1 => create(t1, default(T2), default(T3)))
                                    .Match<T2>(t2 => create(default(T1), t2, default(T3)))
                                    .Match<T3>(t3 => create(default(T1), default(T2), t3));
            if (options == MatchOptions.AnyOrder)
                return Match(create).Match<T1, T3, T2>((t1, t3, t2) => create(t1, t2, t3))
                                    .Match<T2, T3, T1>((t2, t3, t1) => create(t1, t2, t3))
                                    .Match<T2, T1, T3>((t2, t1, t3) => create(t1, t2, t3))
                                    .Match<T3, T1, T2>((t3, t1, t2) => create(t1, t2, t3))
                                    .Match<T3, T2, T1>((t3, t2, t1) => create(t1, t2, t3));

            throw new ApplicationException("Unknown match options");
        }
        /// <summary> Creates a new object from four tokens </summary>
        /// <param name="options">Determine what strategy to use when matching tokens</param>
        /// <exception cref="ApplicationException"></exception>
        public ParserBuilder Match<T1, T2, T3, T4>(Func<T1, T2, T3, T4, object> create, MatchOptions options = MatchOptions.AllInOrder)
        {
            if (options == MatchOptions.AllInOrder)
                return add(new TokenParser(match => create((T1)match.First(), (T2)match.Skip(1).First(), (T3)match.Skip(2).First(), (T4)match.Skip(3).First()), typeof(T1), typeof(T2), typeof(T3), typeof(T4)));
            if (options == MatchOptions.AtLeastOne)
                return Match(create).Match<T1, T2, T3>((t1, t2, t3) => create(t1, t2, t3, default(T4)))
                                    .Match<T1, T2, T4>((t1, t2, t4) => create(t1, t2, default(T3), t4))
                                    .Match<T1, T3, T4>((t1, t3, t4) => create(t1, default(T2), t3, t4))
                                    .Match<T2, T3, T4>((t2, t3, t4) => create(default(T1), t2, t3, t4))
                                    .Match<T1, T2>((t1, t2) => create(t1, t2, default(T3), default(T4)))
                                    .Match<T1, T3>((t1, t3) => create(t1, default(T2), t3, default(T4)))
                                    .Match<T1, T4>((t1, t4) => create(t1, default(T2), default(T3), t4))
                                    .Match<T2, T3>((t2, t3) => create(default(T1), t2, t3, default(T4)))
                                    .Match<T2, T4>((t2, t4) => create(default(T1), t2, default(T3), t4))
                                    .Match<T3, T4>((t3, t4) => create(default(T1), default(T2), t3, t4))
                                    .Match<T1>(t1 => create(t1, default(T2), default(T3), default(T4)))
                                    .Match<T2>(t2 => create(default(T1), t2, default(T3), default(T4)))
                                    .Match<T3>(t3 => create(default(T1), default(T2), t3, default(T4)))
                                    .Match<T4>(t4 => create(default(T1), default(T2), default(T3), t4));
            if (options == MatchOptions.AnyOrder)
                throw new NotSupportedException("We're not supporting random order of 4 items yet");

            throw new ApplicationException("Unknown match options");
        }
        /// <summary> Creates a new object from a list of tokens </summary>
        /// <exception cref="ApplicationException"></exception>
        public ParserBuilder MatchList<T1>(Func<T1[], object> create, int minimumItems = 1) => 
            add(new TokenParser(match => create(match.OfType<T1>().ToArray()), $"{TokenParser.GetIdentifier(typeof(T1))}{{{minimumItems},}}"));

        /// <summary> Creates a new array from a list of token </summary>
        /// <exception cref="ApplicationException"></exception>
        public ParserBuilder MatchList<T1>(int minimumItems = 1) => 
            add(new TokenParser(match => match.OfType<T1>().ToArray(), $"{TokenParser.GetIdentifier(typeof(T1))}{{{minimumItems},}}"))
            .MatchList<T1[]>(range => range.SelectMany(i => i).ToArray());

        /// <summary> Match all keys in a dictionary to their value and create a dictionary </summary>
        /// <exception cref="ApplicationException"></exception>
        public ParserBuilder MatchDictionary<TKey, TValue>() => 
            Match<TKey, TValue>((key, value) =>(key, value), MatchOptions.AtLeastOne)
           .MatchList<(TKey, TValue)>(range => range.ToDictionary(kv => kv.Item1, kv => kv.Item2));

        public ParserBuilder CombineWith(ParserBuilder other)
        {
            _tokenParsers.AddRange(other._tokenParsers);
            return this;
        }
    }

    public enum MatchOptions
    {
        /// <summary> All tokens should be available and in the order specified  </summary>
        AllInOrder,
        /// <summary> Tokens must all be available but can be defined in any order possible </summary>
        AnyOrder,
        /// <summary> Any one of the tokens should be available </summary>
        AtLeastOne
    }
}
