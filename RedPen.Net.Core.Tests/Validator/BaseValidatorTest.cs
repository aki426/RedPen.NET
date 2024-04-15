using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPenTokenizerFactory = RedPen.Net.Core.Tokenizer.RedPenTokenizerFactory;

namespace RedPen.Net.Core.Tests.Validator.DocumentValidator
{
    /// <summary>
    /// The base validator test.
    /// </summary>
    public abstract class BaseValidatorTest
    {
        protected string validatorName;
        protected Configuration config;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseValidatorTest"/> class.
        /// </summary>
        /// <param name="validatorName">The validator name.</param>
        protected BaseValidatorTest(string validatorName)
        {
            this.validatorName = validatorName;
            this.config = getConfiguration("en");
        }

        /// <summary>
        /// gets the configuration.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <returns>A Configuration.</returns>
        protected Configuration getConfiguration(string language)
        {
            return Configuration.Builder()
              .AddValidatorConfig(new ValidatorConfiguration(validatorName))
              .SetLang(language).Build();
        }

        /// <summary>
        /// prepares the simple document.
        /// </summary>
        /// <param name="sentrence">The sentrence.</param>
        /// <returns>A Document.</returns>
        protected Document prepareSimpleDocument(string sentrence)
        {
            // MEMO: Document.Builder(config.Tokenizer)の時点ですでにTokenizeが完了する。
            // configは言語設定だけでなくTokenizerの引き当てを機能に含む。
            return Document.Builder(RedPenTokenizerFactory.CreateTokenizer(config.CultureInfo))
              .AddSection(1)
              .AddParagraph()
              .AddSentence(new Sentence(sentrence, 1))
              .Build();
        }

        /// <summary>
        /// sentences the.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>A Sentence.</returns>
        protected Sentence sentence(string text)
        {
            // TODO: Null参照の可能性あり。
            return prepareSimpleDocument(text).GetLastSection().Paragraphs[0].Sentences[0];
        }
    }
}
