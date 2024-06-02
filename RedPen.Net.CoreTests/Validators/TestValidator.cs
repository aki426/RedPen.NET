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
    // TODO: Configurationにはパラメータに応じたInterfaceがあるので、必要なパラメータはInterfaceを実装することで定義する。
    /// <summary>TestのConfiguration</summary>
    public record TestConfiguration : ValidatorConfiguration
    {
        public TestConfiguration(ValidationLevel level) : base(level)
        {
        }
    }

    // TODO: Validation対象に応じて、IDocumentValidatable, ISectionValidatable, ISentenceValidatableを実装する。
    /// <summary>TestのValidator</summary>
    public class TestValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        // TODO: 専用のValidatorConfigurationを別途定義する。
        /// <summary>ValidatorConfiguration</summary>
        public TestConfiguration Config { get; init; }

        // TODO: サポート対象言語がANYではない場合overrideで再定義する。
        /// <summary></summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

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

            // TODO: MessageKey引数はErrorMessageにバリエーションがある場合にValidator内で条件判定して引数として与える。
            //result.Add(new ValidationError(
            //    Config.ValidationName,
            //    this.Level,
            //    sentence,
            //    MessageArgs: new object[] { argsForMessageArg }));

            return result;
        }
    }
}
