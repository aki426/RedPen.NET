using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    // TODO: Configurationにはパラメータに応じたInterfaceがあるので、必要なパラメータはInterfaceを実装することで定義する。

    /// <summary>XXXのConfiguration</summary>
    public record TaigendomeConfiguration : ValidatorConfiguration
    {
        public TaigendomeConfiguration(ValidationLevel level) : base(level)
        {
        }
    }

    /// <summary>TaigendomeのValidator</summary>
    public class TaigendomeValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        public TaigendomeConfiguration Config { get; init; }

        // MEMO: サポート対象言語がANYではない場合overrideで再定義する。
        /// <summary></summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

        // MEMO: コンストラクタの引数定義は共通にすること。
        public TaigendomeValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            TaigendomeConfiguration config) :
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

            // Tokenがない場合は体言止めではないと判断する。
            if (sentence.Tokens.Any())
            {
                // 記号を除いた最後のTokenが名詞だった場合のみ体言止め表現とみなす。
                var noSymbols = sentence.Tokens.Where(t => t.Tags[0] is not "記号").ToList();
                if (noSymbols.Any() && (noSymbols.Last().Tags[0] is "名詞"))
                {
                    // TODO: MessageKey引数はErrorMessageにバリエーションがある場合にValidator内で条件判定して引数として与える。
                    result.Add(new ValidationError(
                        ValidationType.Taigendome,
                        this.Level,
                        sentence,
                        noSymbols.Last().OffsetMap[0],
                        noSymbols.Last().OffsetMap[^1],
                        MessageArgs: new object[] { noSymbols.Last().Surface }));
                }
            }

            return result;
        }
    }
}
