using RedPen.Net.Core.Config;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    public record SentenceLengthConfiguration : ValidatorConfiguration, IMaxLengthConfigParameter
    {
        public int MaxLength { get; init; }

        public SentenceLengthConfiguration(ValidationLevel level, int maxLength) : base(level)
        {
            MaxLength = maxLength;
        }
    }
}
