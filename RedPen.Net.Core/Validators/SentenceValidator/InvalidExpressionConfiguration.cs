using System.Collections.Generic;
using RedPen.Net.Core.Config;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    public record InvalidExpressionConfiguration : ValidatorConfiguration, IWordListConfigParameter
    {
        public List<string> WordList { get; init; }

        public InvalidExpressionConfiguration(ValidationLevel level, List<string> wordList) : base(level)
        {
            WordList = wordList;
        }
    }
}
