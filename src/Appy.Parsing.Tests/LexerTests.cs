using Appy.Parsing.Builder;
using Appy.Parsing.Lexers;
using FluentAssertions;
using Xunit;

namespace Appy.Parsing.Tests
{
    public class LexerTests
    {
        struct PlusToken { }
        struct MinusToken { }
        struct MultiToken { }
        struct DivideToken { }
        readonly Lexer _subject;
        public LexerTests() =>
            _subject = LexerBuilder.Build()
                                   .Match<PlusToken>("plus", @"\+")
                                   .Match<MinusToken>("minus", @"-")
                                   .Match<MultiToken>("multi", @"\*")
                                   .Match<DivideToken>("divide", @"\/")
                                   .Match("number", @"\b[0-9]+(\.[0-9]*)?\b", match => decimal.Parse(match.Value))
                                   .Create();

        [Fact]
        public void ParsesTokens() =>
            _subject.Tokenize("9 * 6 / 3 - 2")
                .Should().BeEquivalentTo(9, new MultiToken(), 6, new DivideToken(), 3, new MinusToken(), 2);
    }
}