using RedPen.Net.Core.Config;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    public record JapaneseStyleConfiguration : ValidatorConfiguration
    {
        public JapaneseStyleConfiguration(ValidationLevel level) : base(level)
        {
        }
    }
}
