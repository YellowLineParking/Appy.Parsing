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

        ParserBuilder(Lexer lexer) => 
            _lexer = lexer;

        public static ParserBuilder Build(LexerBuilder lexerBuilder) =>
            new ParserBuilder(lexerBuilder.Create());

        public Parser<T> Create<T>() =>
            new Parser<T>(_lexer, _tokenParsers);

        public Parser Create() =>
            new Parser(_lexer, _tokenParsers);

        ParserBuilder add(TokenParser parser)
        {
            _tokenParsers.Add(parser);
            return this;
        }

        public ParserBuilder Match<T1>(Func<T1, object> create) => 
            add(new TokenParser(match => create((T1)match.First()), typeof(T1)));

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

        public ParserBuilder MatchList<T1>(Func<T1[], object> create, int minimumItems = 1) => 
            add(new TokenParser(match => create(match.OfType<T1>().ToArray()), $"{TokenParser.GetIdentifier(typeof(T1))}{{{minimumItems},}}"));

        public ParserBuilder MatchList<T1>(int minimumItems = 1) => 
            add(new TokenParser(match => match.OfType<T1>().ToArray(), $"{TokenParser.GetIdentifier(typeof(T1))}{{{minimumItems},}}"))
            .MatchList<T1[]>(range => range.SelectMany(i => i).ToArray());

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
        AllInOrder,
        AnyOrder,
        AtLeastOne
    }
}
