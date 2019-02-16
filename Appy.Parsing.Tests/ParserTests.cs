using System.Collections.Generic;
using Appy.Parsing.Builder;
using Appy.Parsing.Parsers;
using FluentAssertions;
using Xunit;

namespace Appy.Parsing.Tests
{
    public class CanParseGenerics
    {
        class Number { public int Value; }
        class Text {  public string Value;}

        readonly Parser<KeyValuePair<Number, Text>> _subject;
        public CanParseGenerics()
        {
            var lexer = LexerBuilder.Build()
                                    .Match("nr", @"\d+", match => new Number {Value = int.Parse(match.Value)})
                                    .Match("txt", @".+", match => new Text { Value = match.Value });
            _subject = ParserBuilder.Build(lexer)
                                    .Match<Number, Text>((nr, t) => new KeyValuePair<Number, Text>(nr, t), MatchOptions.AtLeastOne)
                                    .Create<KeyValuePair<Number, Text>>();
        }

        [Fact]
        public void ParsesTheValues() =>
            _subject.Parse("1text").Should().BeEquivalentTo(new KeyValuePair<Number, Text>(new Number {Value = 1}, new Text {Value = "text"}));

        [Fact]
        public void ParsesTheOptionalValues() =>
            _subject.Parse("1").Should().BeEquivalentTo(new KeyValuePair<Number, Text>(new Number { Value = 1 }, null));
    }
    public class CalculatorTest
    {
        class PlusToken { }
        class MinusToken { }
        class MultiToken { }
        class DivideToken { }
        readonly Parser<decimal> _subject;
        public CalculatorTest()
        {
            var lexer = LexerBuilder.Build()
                                    .Match<PlusToken>("plus", @"\+")
                                    .Match<MinusToken>("minus", @"-")
                                    .Match<MultiToken>("multi", @"\*")
                                    .Match<DivideToken>("divide", @"\/")
                                    .Match("number", @"\b[0-9]+(\.[0-9]*)?\b", match => decimal.Parse(match.Value));
            _subject = ParserBuilder.Build(lexer)
                                    .Match<decimal, DivideToken, decimal>((o1, by, o2) => o1 / o2)
                                    .Match<decimal, MultiToken, decimal>((o1, times, o2) => o1 * o2)
                                    .Match<decimal, MinusToken, decimal>((o1, minus, o2) => o1 - o2)
                                    .Match<decimal, PlusToken, decimal>((o1, plus, o2) => o1 + o2)
                                    .Create<decimal>();
        }

        [Theory]
        [InlineData("3 + 3", 6)]
        [InlineData("6 - 2", 4)]
        [InlineData("3 * 3", 9)]
        [InlineData("9 / 3", 3)]
        [InlineData("9 * 3 - 10", 17)]
        [InlineData("9 * 3 - 10 / 2", 22)]
        [InlineData("9 * 3 - 10 / 2 + 3", 25)]
        [InlineData("9 * 3 + 10 / 2 - 3", 29)]
        [InlineData("9 * 6 / 3", 18)]
        [InlineData("9 * 6 / 3 - 2", 16)]
        public void CanSum(string expression, decimal expected) => 
            _subject.Parse(expression).Should().Be(expected);
    }
}
