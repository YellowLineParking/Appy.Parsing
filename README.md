# Appy.Parsing

Appy.Parsing is an Open source library for parsing raw text into objects

It's comprised of two main components:
- A lexer that parses text into tokens
- A parser that converts tokens into objects.

It also has a number of Builders that allow you to easily define a grammar and build parsers and lexers.

# Usage
Let's consider the following example where we want to parse a numeric expression and convert that into the result

Input: `jan-mar dec mon-fri 10:00-19:00`
Expected output: 
```
{
    daysOfWeek: [1, 2, 3, 4, 5],
    months: "January, February, March, December",
    times: {
        start: "10:00",
        end: "19:00"
    }
}
```

## Lexing (tokenisation)
First of all, we need to create a Lexer that parses the individual bits into tokens. To do that, we use regular expressions.

First, we need to define our tokens, we will need a token for days, one for months and one for hours:

```
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
```
First we create an enum where each of the values matches a regular expression. As seen in the example above, we can define flexible rules to match days

The same approach can be used for the months:
```
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
```

The hours can be tokenised into a local time, so we don't need to create a specific token for that
We have one missing token however, which is the range token (`-`). We can define that as a simple token without anything else:
```
[Match("-")] public struct RangeToken { }
```

These classes are just so we can represent each of the text parts as tokens, rather than as plain text

With these tokens, we can now create a lexer:
```
var lexer = LexerBuilder.Build()
                        .MatchEnum<Days>()
                        .MatchEnum<Months>()
                        .Match("time", @"((?<hour>\d{1,2}):(?<minute>\d{1,2})(:(?<second>\d{0,2}))?)", match => new LocalTime(int.Parse(match.Groups["hour"].Value),
                                                                                                                      int.Parse(match.Groups["minute"].Value),
                                                                                                                      string.IsNullOrEmpty(match.Groups["second"].Value) ? 0 : int.Parse(match.Groups["second"].Value)))
                        .Match<RangeToken>();
```
In the above sample, you can see how it matches regular expressions to convert text bits to tokens. For the time, we lex them into a NodaTime LocalTime instance

We can now use this lexer to get back an array of tokens:
```
var tokens = lexer.Tokenize("Jan-Mar Dec Mon-Fri 10:00-19:00");
```
The resulting array will be equivalent to this:
```
[]{ Months.January, new RangeToken(), Months.March, Months.December, Days.Monday, new RangeToken(), Days.Friday, new LocalTime(10, 0), new RangeToken(), new LocalTime(19, 0)}
```

## Parsing (tokens to objects)
Now that we have the tokens, we want to convert that into an object. We'll create a class to hold the data:
```
public class TimeInfo
{
    public List<Months> Months {get; set; }
    public List<Days> Days {get; set; }
    public HourRange Times { get; set; }
}

public class HourRange
{
    public LocalTime Start { get; set; }
    public LocalTime End { get; set; }
}
```

Now we need to build a parser that uses this lexer to convert the tokens into meaningful objects. 

```
var parser =    // We pass the lexer into the builder so the resulting parser can use this lexer to convert text into tokens
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

var timeInfo = parser.Parse("jan-mar dec mon-fri 10:00-19:00");

```

# More options

## Lexer
The following options are available when building a Lexer:

### Match input with an expression and convert it into an object.
`Match(string groupName, string expression, Func<Match, object> tokenize)`

Example:
```
lexerBuilder.Match("percent", @"(?<value>\d+)\%", match => new PercentToken(int.Parse(match.Groups["value"].Value)))
```
### Ignore a part of the expression
``` Ignore(string groupName, string expression)``` 

### Match an expression and return a value
``` Match<T>(string groupName, string expression, T value)``` 

The following calls are equivalent:

```
Match("group", "expression", match => new object());
Match("group", "expression", new object());
```

### Match an expression and return a default value
``` Match<T>(string groupName, string expression)``` 

The following calls are equivalent:

```
Match("group", "expression", match => new object());
Match<object>("group", "expression");
```

### Match an expression based on an attribute
``` Match<T>()``` 

This will lookup the match attribute on type T. It extracts the group name from the type and expression from the attribute.

Example:

```
[Match("-")] 
public struct RangeToken { }

lexerBuilder.Match<RangeToken>();
```

### Match any value inside an Enum
Looks up the enum and extracts all values and their matching expressions

Example:

```
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

lexerBuilder.MatchEnum<Days>();
```

## Parser

### Match a token of type T1 and creat a new object
``` Match<T1>(Func<T1, object> create)``` 

Example:
This expression converts "Monday" into a DateTime and the first upcoming Monday

``` parserBuilder.Match<Day>(day => DateTime.Now.AddDays(((int) day - (int) start.DayOfWeek + 7) % 7)``` 

### Match two tokens and create a new object (see also overloads for 3 and 4 tokens)
``` Match<T1, T2>(Func<T1, T2, object> create, MatchOptions options = MatchOptions.AllInOrder)``` 

Example:
This example matches two days and creates a range between them

``` parserBuilder.Match<Days, Days>((start, end) => Enumerable.Range((int)start, (int)end - (int)start + 1).Cast<Days>().ToArray())``` 

The options define whether all tokens are necessary and in the correct order. Keep in mind that you might get default values in the callback

### Create an object from a list of tokens
``` MatchList<T1>(Func<T1[], object> create, int minimumItems = 1)``` 

Example:
This example takes a list of numbers and returns the sum

```parserBuilder.MatchList<decimal>(numbers => numbers.Sum())``` 

### Create an array<T> from a list of tokens
``` MatchList<T1>(int minimumItems = 1)``` 

Example: this example takes a sequence of days and converts it into a Days[]

``` parserBuilder.MatchList<Days>()``` 

### Create a dictionary from key value pairs
``` MatchDictionary<TKey, TValue>()``` 

Example: The following sample converts a sequence of months and days into a dictionary

``` parserBuilder.MatchDictionary<Months, Days>()``` 

```
jan mon feb tue mar sun ==>

{
    { Months.Jan, Days.Mon },
    { Months.Feb, Days.Tue },
    { Months.Mar, Days.Sun }
}
```

### Combine multiple builders into one
`CombineWith(ParserBuilder other)`

In the following example the combined parser will be able to parse both Days and Months. This is useful when building discrete parsers that can be used as building blocks for other parsers.
```
var dayParser = ParserBuilder.Builder(lexer)
                             .MatchList<Days>();
var monthParser = ParserBuilder.Builder(lexer)
                               .MatchList<Months>();

var combinedParser = ParserBuilder.Build(lexer)
                                  .CombineWith(dayParser)
                                  .CombineWith(monthParser);
```
