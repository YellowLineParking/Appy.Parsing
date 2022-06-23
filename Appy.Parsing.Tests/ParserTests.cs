using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Appy.Parsing.Builder;
using Appy.Parsing.Parsers;
using FluentAssertions;
using NodaTime;
using Xunit;
using static Appy.Parsing.Tests.TimeInfoTest;

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

    public class TimeInfoTest
    {
        public enum Days
        {
            [Match(@"\b(mondays|monday|mon)\b")] Mon = 1,
            [Match(@"\b(tuesdays|tuesday|tues|tue)\b")] Tue = 2,
            [Match(@"\b(wednesdays|wednesday|wed)\b")] Wed = 3,
            [Match(@"\b(thursdays|thursday|thurs|thur|thu)\b")] Thu = 4,
            [Match(@"\b(fridays|friday|fri)\b")] Fri = 5,
            [Match(@"\b(saturdays|saturday|sat)\b")] Sat = 6,
            [Match(@"\b(sundays|sunday|sun)\b")] Sun = 7
        }

        public enum Months
        {
            [Match(@"\b(january|jan)\b")] Jan = 1,
            [Match(@"\b(february|feb)\b")] Feb = 2,
            [Match(@"\b(march|mar)\b")] Mar = 3,
            [Match(@"\b(april|apr)\b")] Apr = 4,
            [Match(@"\b(may)\b")] May = 5,
            [Match(@"\b(june|jun)\b")] Jun = 6,
            [Match(@"\b(july|jul)\b")] Jul = 7,
            [Match(@"\b(august|aug)\b")] Aug = 8,
            [Match(@"\b(september|sep)\b")] Sep = 9,
            [Match(@"\b(october|oct)\b")] Oct = 10,
            [Match(@"\b(november|nov)\b")] Nov = 11,
            [Match(@"\b(december|dec)\b")] Dec = 12
        }

        [Match("-")] public struct RangeToken { }

        public class TimeInfo
        {
            public List<Months> Months { get; set; }
            public List<Days> Days { get; set; }
            public HourRange Times { get; set; }
        }
        public class HourRange
        {
            public LocalTime Start { get; set; }
            public LocalTime End { get; set; }
        }

        readonly Parser<TimeInfo> _subject;

        public TimeInfoTest()
        {
            var lexer = LexerBuilder.Build()
                        .MatchEnum<Days>()
                        .MatchEnum<Months>()
                        .Match("time", @"((?<hour>\d{1,2}):(?<minute>\d{1,2})(:(?<second>\d{0,2}))?)", match => new LocalTime(int.Parse(match.Groups["hour"].Value),
                                                                                                                      int.Parse(match.Groups["minute"].Value),
                                                                                                                      string.IsNullOrEmpty(match.Groups["second"].Value) ? 0 : int.Parse(match.Groups["second"].Value)))
                        .Match<RangeToken>();

            _subject =    // We pass in the lexer into the builder so the resulting parser can use this lexer to convert text into tokens
                ParserBuilder.Build(lexer)
                // next we define rules on how to convert tokens into objects
                // Match Jan-Mar and output new[]{ Months.January, Months.February, Months.March}
                .Match<Months, RangeToken, Months>((start, range, end) => Enumerable.Range((int)start, (int)end - (int)start + 1).Cast<Months>().ToArray())
                // Match a list of months: Jan Feb Mar Dec => new[]{ Months.January, Months.February, Months.March, Months.December}
                .MatchList<Months>()
                // Match Mon-Friday and output new[]{ Days.Monday, Days.Tuesday, Days.Wednesday, Days.Thursday, Days.Friday}
                .Match<Days, RangeToken, Days>((start, range, end) => Enumerable.Range((int)start, (int)end - (int)start + 1).Cast<Days>().ToArray())
                // Match a list of days: Mon, Tue, Wed, Thu, Fri => ew[]{ Days.Monday, Days.Tuesday, Days.Wednesday, Days.Thursday, Days.Friday}
                .MatchList<Days>()
                // Match a time-range-time token sequence into an HourRange
                .Match<LocalTime, RangeToken, LocalTime>((start, _, end) => new HourRange { Start = start, End = end})
                // Combine all of the above and create a new instance of TimeInfo
                .Match<Months[], Days[], HourRange>((months, days, hourRange) => 
                    new TimeInfo
                    {
                        Months = months.ToList(),
                        Days = days.ToList(),
                        Times = hourRange
                    })
                // With the above configuration, create a parser that can parse a text into a TimeInfo object
                .Create<TimeInfo>();
        }

        [Fact]
        public void ParsesCorrectly()
        {
            var timeInfo = _subject.Parse("jan-mar dec mon-fri 10:00-19:00");
            timeInfo.Months.Should().BeEquivalentTo(new[] { Months.Jan, Months.Feb, Months.Mar, Months.Dec });
            timeInfo.Days.Should().BeEquivalentTo(new[] { Days.Mon, Days.Tue, Days.Wed, Days.Thu, Days.Fri });
            timeInfo.Times.Start.Should().BeEquivalentTo(new LocalTime(10, 00));
            timeInfo.Times.End.Should().BeEquivalentTo(new LocalTime(19, 00));
        }
    }
}
