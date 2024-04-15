using System.Collections.Generic;
using RedPen.Net.Core.Config;

namespace RedPen.Net.Core.Validators.DocumentValidator
{
    public record JapaneseExpressionVariationConfiguration : ValidatorConfiguration, IWordMapConfigParameter
    {
        public Dictionary<string, string> WordMap { get; init; }

        public JapaneseExpressionVariationConfiguration(ValidationLevel level, Dictionary<string, string> wordMap) : base(level)
        {
            WordMap = wordMap;
        }
    }
}
