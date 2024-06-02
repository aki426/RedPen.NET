using System.Collections.Generic;
using System.Globalization;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace System.Runtime.CompilerServices
{
    internal sealed class IsExternalInit
    { }
}

namespace RedPen.Net.Core.Validators.Tests
{
    // NOTE: 応用アプリケーション側で新規追加するValidationのテスト用。

    /// <summary>TestのConfiguration</summary>
    public record TestConfiguration : ValidatorConfiguration
    {
        public TestConfiguration(ValidationLevel level) : base(level)
        {
        }
    }

    /// <summary>TestのValidator</summary>
    public class TestValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public TestConfiguration Config { get; init; }

        // TODO: コンストラクタの引数定義は共通にすること。
        /// <summary>
        /// Initializes a new instance of the <see cref="TestValidator"/> class.
        /// </summary>
        /// <param name="documentLangForTest">The document lang for test.</param>
        /// <param name="symbolTable">The symbol table.</param>
        /// <param name="config">The config.</param>
        public TestValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            TestConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;
        }

        /// <summary>
        /// Validate.
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <returns>A list of ValidationErrors.</returns>
        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            // validation
            // あくまでテストなので、「a」または「あ」を検出するValidationとする。
            var indexOfAlpha = sentence.Content.IndexOf('a');
            if (indexOfAlpha >= 0)
            {
                result.Add(new ValidationError(
                    Config.ValidationName,
                    this.Level,
                    sentence,
                    sentence.ConvertToLineOffset(indexOfAlpha),
                    sentence.ConvertToLineOffset(indexOfAlpha),
                    MessageArgs: new object[] { "a" }));
            }

            var indexOfHiraganaA = sentence.Content.IndexOf('あ');
            if (indexOfHiraganaA >= 0)
            {
                result.Add(new ValidationError(
                    Config.ValidationName,
                    this.Level,
                    sentence,
                    sentence.ConvertToLineOffset(indexOfHiraganaA),
                    sentence.ConvertToLineOffset(indexOfHiraganaA),
                    MessageArgs: new object[] { "あ" }));
            }

            return result;
        }
    }
}
