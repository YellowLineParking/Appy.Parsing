using System;

namespace Appy.Parsing.Builder
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct)]
    public class MatchAttribute : Attribute
    {
        public string Expression { get; }

        public MatchAttribute(string expression) => Expression = expression;
    }
}