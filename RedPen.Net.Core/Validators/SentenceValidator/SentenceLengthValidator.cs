using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    // MEMO: JAVA版ではpublic final class指定なので、sealed classに変更している。

    /// <summary>
    /// The sentence length validator.
    /// </summary>
    public sealed class SentenceLengthValidator : Validator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SentenceLengthValidator"/> class.
        /// </summary>
        public SentenceLengthValidator() : base(new object[] { "max_len", 120 })
        {
        }

        /// <summary>
        /// validates the.
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        public override void Validate(Sentence sentence)
        {
            // int maxLength = GetProperty<int>("max_len");
            int maxLength = GetInt("max_len");
            if (sentence.Content.Length > maxLength)
            {
                addLocalizedError(sentence, sentence.Content.Length, maxLength);
            }
        }
    }
}
