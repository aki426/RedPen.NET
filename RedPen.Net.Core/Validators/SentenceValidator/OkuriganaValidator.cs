using System.Collections.Generic;
using System.Globalization;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>OkuriganaのConfiguration</summary>
    public record OkuriganaConfiguration : ValidatorConfiguration
    {
        public OkuriganaConfiguration(ValidationLevel level) : base(level)
        {
        }
    }

    // TODO: Validation対象に応じて、IDocumentValidatable, ISectionValidatable, ISentenceValidatableを実装する。
    public class OkuriganaValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        // TODO: 専用のValidatorConfigurationを別途定義する。

        public OkuriganaConfiguration Config { get; init; }

        // TODO: サポート対象言語がANYではない場合overrideで再定義する。
        /// <summary></summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

        //private static readonly HashSet<string> InvalidOkurigana;
        //private static readonly HashSet<ExpressionRule> InvalidOkuriganaTokens;

        ///// <summary>
        ///// Initializes a new instance of the <see cref="OkuriganaValidator"/> class.
        ///// </summary>
        //static OkuriganaValidator()
        //{
        //    InvalidOkuriganaTokens = new HashSet<ExpressionRule>();
        //    InvalidOkuriganaTokens.Add(new ExpressionRule().AddElement(new TokenElement("合さ", new List<string> { "動詞", "自立" }, 0)));
        //    InvalidOkuriganaTokens.Add(new ExpressionRule().AddElement(new TokenElement("合し", new List<string> { "動詞", "自立" }, 0)));
        //    InvalidOkuriganaTokens.Add(new ExpressionRule().AddElement(new TokenElement("合す", new List<string> { "動詞", "自立" }, 0)));
        //    InvalidOkuriganaTokens.Add(new ExpressionRule().AddElement(new TokenElement("合せ", new List<string> { "動詞", "自立" }, 0)));

        //    // ... (その他のExpressionRuleの初期化は省略)

        //    InvalidOkurigana = new HashSet<string>();
        //    InvalidOkurigana.Add("恐し");
        //    InvalidOkurigana.Add("短か");
        //    InvalidOkurigana.Add("著るしい");
        //    // ... (その他のInvalidOkuriganaの初期化は省略)
        //}

        // TODO: コンストラクタの引数定義は共通にすること。
        public OkuriganaValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            OkuriganaConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;
        }

        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            // validation
            //foreach (var value in InvalidOkurigana)
            //{
            //    int startPosition = sentence.Content.IndexOf(value);
            //    if (startPosition != -1)
            //    {
            //        AddLocalizedErrorWithPosition(sentence, startPosition, startPosition + value.Length, value);
            //    }
            //}

            //foreach (var rule in InvalidOkuriganaTokens)
            //{
            //    if (rule.Match(sentence.Tokens))
            //    {
            //        AddLocalizedError(sentence, rule.ToString());
            //    }
            //}

            // TODO: MessageKey引数はErrorMessageにバリエーションがある場合にValidator内で条件判定して引数として与える。
            result.Add(new ValidationError(
                ValidationType.Okurigana,
                this.Level,
                sentence,
                MessageArgs: new object[] { "some result" }));

            return result;
        }
    }
}
