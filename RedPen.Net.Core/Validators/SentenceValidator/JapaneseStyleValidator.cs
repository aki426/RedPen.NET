using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    // MEMO: Configurationの定義は短いのでValidatorファイル内に併記する。

    /// <summary>JapaneseStyleのConfiguration</summary>
    public record JapaneseStyleConfiguration : ValidatorConfiguration
    {
        public JapaneseStyleConfiguration(ValidationLevel level) : base(level)
        {
        }
    }

    /// <summary>JapaneseStyleのValidator</summary>
    public class JapaneseStyleValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        public JapaneseStyleConfiguration Config { get; init; }

        /// <summary></summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

        // TODO: コンストラクタの引数定義は共通にすること。
        public JapaneseStyleValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            JapaneseStyleConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;
        }

        private static readonly Regex DearuPattern = new Regex(@"である|のだが|であった|あるが|あった|だった");
        private static readonly Regex DesumasuPattern = new Regex(@"ですね|でした|ました|でしたが|でしたので|ですので|ですが|です|ます");

        private int dearuCount = 0;
        private int desumasuCount = 0;

        private int CountMatch(Sentence sentence, Regex pattern)
        {
            var content = sentence.Content;
            var matches = pattern.Matches(content);
            return matches.Count;
        }

        private void DetectPattern(Sentence sentence, Regex pattern)
        {
            var matches = pattern.Matches(sentence.Content);
            foreach (Match match in matches)
            {
                //AddLocalizedErrorWithPosition(sentence, match.Index, match.Index + match.Length, match.Value);
            }
        }

        public void PreValidate(Sentence sentence)
        {
            // コンテンツとのマッチを数える
            dearuCount += CountMatch(sentence, DearuPattern);
            desumasuCount += CountMatch(sentence, DesumasuPattern);
        }

        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            //// validation
            ///
            //var forceDearu = GetBoolean("ForceDearu");

            //if (dearuCount > desumasuCount || forceDearu)
            //{
            //    DetectPattern(sentence, DesumasuPattern);
            //}
            //else
            //{
            //    DetectPattern(sentence, DearuPattern);
            //}

            //// TODO: MessageKey引数はErrorMessageにバリエーションがある場合にValidator内で条件判定して引数として与える。
            //result.Add(new ValidationError(
            //    ValidationType.XXX,
            //    this.Level,
            //    sentence,
            //    MessageArgs: new object[] { argsForMessageArg }));

            return result;
        }
    }
}
