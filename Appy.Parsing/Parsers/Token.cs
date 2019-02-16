using System.Text.RegularExpressions;

namespace Appy.Parsing.Parsers
{
    public abstract class Token
    {
        public string Expression { get; }

        protected Token(string expression) => Expression = expression;
    }

    public class WordToken : Token
    {
        public static readonly Regex Regex = new Regex("\\G\\w+", RegexOptions.Compiled);
        public WordToken(string value) : base($"(\\b{value}\\b\\(\\d+\\) {{0,1}})") { }
        public static WordToken FromMatch(Match match) => new WordToken(match.Value);
    }

    public class OpenGroupToken : Token
    {
        public OpenGroupToken() : base("("){}
        public static readonly Regex Regex = new Regex("\\G\\(", RegexOptions.Compiled);
    }
    public class CloseGroupToken : Token
    {
        public CloseGroupToken() : base(")") { }
        public static readonly Regex Regex = new Regex("\\G\\)", RegexOptions.Compiled);
    }

    public class RepeaterToken : Token
    {
        public RepeaterToken(int min, int? max) : base($"{{{min},{max}}}"){}
        public static readonly Regex Regex = new Regex("\\G\\{(?<min>\\d+)(?<maxpart>,|,(?<max>\\d+))?\\}", RegexOptions.Compiled);
        public static RepeaterToken FromMatch(Match match)
        {
            var min = match.Groups["min"].Value;
            var max = match.Groups["max"].Value;

            var minValue = int.Parse(min);
            var maxValue = match.Groups["maxpart"].Success ?
                string.IsNullOrEmpty(max) ? (int?)null : int.Parse(max)
                : minValue;

            return new RepeaterToken(minValue, maxValue);
        }
    }

    public class OrToken : Token
    {
        public OrToken() : base("|"){ }
        public static readonly Regex Regex = new Regex("\\G\\|", RegexOptions.Compiled);
    }

    public class WhiteSpaceToken : Token
    {
        public WhiteSpaceToken() : base(" "){ }
        public static readonly Regex Regex = new Regex("\\G +", RegexOptions.Compiled);
    }
}