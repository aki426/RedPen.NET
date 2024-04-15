using RedPen.Net.Core.Config;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    public record CommaNumberConfiguration : ValidatorConfiguration, IMaxNumberConfigParameter
    {
        public int MaxNumber { get; init; }

        public CommaNumberConfiguration(ValidationLevel level, int maxNumber) : base(level)
        {
            MaxNumber = maxNumber;
        }
    }
}
