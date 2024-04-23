using System.Collections.Generic;
using RedPen.Net.Core.Config;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    public record SuggestExpressionConfiguration : ValidatorConfiguration, IWordMapConfigParameter
    {
        public Dictionary<string, string> WordMap { get; init; }

        public SuggestExpressionConfiguration(ValidationLevel level, Dictionary<string, string> wordMap) : base(level)
        {
            WordMap = wordMap;
        }
    }
}
