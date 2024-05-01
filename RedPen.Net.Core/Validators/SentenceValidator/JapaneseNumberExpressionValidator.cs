using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>JapaneseNumberExpressionのConfiguration</summary>
    public record JapaneseNumberExpressionConfiguration : ValidatorConfiguration, INumberStyleConfigParameter
    {
        public NumberStyle NumberStyle { get; init; }

        public JapaneseNumberExpressionConfiguration(ValidationLevel level, NumberStyle numberStyle) : base(level)
        {
            this.NumberStyle = numberStyle;
        }
    }

    // TODO: Validation対象に応じて、IDocumentValidatable, ISectionValidatable, ISentenceValidatableを実装する。
    /// <summary>JapaneseNumberExpressionのValidator</summary>
    public class JapaneseNumberExpressionValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        // TODO: 専用のValidatorConfigurationを別途定義する。
        /// <summary>ValidatorConfiguration</summary>
        public JapaneseNumberExpressionConfiguration Config { get; init; }

        // TODO: サポート対象言語がANYではない場合overrideで再定義する。
        /// <summary></summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

        // TODO: コンストラクタの引数定義は共通にすること。
        public JapaneseNumberExpressionValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            JapaneseNumberExpressionConfiguration config) :
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
            foreach (var pat in currentStylePattern())
            {
                var m = pat.Matches(sentence.Content);
                foreach (Match match in m)
                {
                    result.Add(new ValidationError(
                        ValidationType.JapaneseNumberExpression,
                        this.Level,
                        sentence,
                        sentence.ConvertToLineOffset(match.Index),
                        sentence.ConvertToLineOffset(match.Index + match.Length - 1),
                        MessageArgs: new object[] { match.Value }));
                }
            }

            return result;
        }

        private readonly List<Regex> allowNumericHenkakuPattern = new List<Regex>
        {
            new Regex(@"(?<![\\u4e00-\\u9faf])[一二三四五六七八九０-９][一二三四五六七八九０-９.．〜、]*[つの]"),
            new Regex(@"(ひと|ふた|[みよむや]っ|いつ|ここの)つ")
        };

        private readonly List<Regex> allowNumericZenkakuPattern = new List<Regex>
        {
            new Regex(@"(?<![\\u4e00-\\u9faf])[一二三四五六七八九0-9][一二三四五六七八九0-9.．〜、]*[つの]"),
            new Regex(@"(ひと|ふた|[みよむや]っ|いつ|ここの)つ")
        };

        private readonly List<Regex> allowKansujiPattern = new List<Regex>
        {
            new Regex(@"[0-9０-９][0-9０-９.．〜、]*[つの]"),
            new Regex(@"(ひと|ふた|[みよむや]っ|いつ|ここの)つ")
        };

        private readonly List<Regex> allowHiraganaPattern = new List<Regex>
        {
            new Regex(@"(?<![\\u4e00-\\u9faf])[一二三四五六七八九0-9０-９][一二三四五六七八九0-9０-９.．〜、]*[つの]")
        };

        private List<Regex> currentStylePattern()
        {
            return Config.NumberStyle switch
            {
                NumberStyle.HankakuOnly => allowNumericHenkakuPattern,
                NumberStyle.ZenkakuOnly => allowNumericZenkakuPattern,
                NumberStyle.KansujiOnly => allowKansujiPattern,
                NumberStyle.HiraganaOnly => allowHiraganaPattern,
                // MEMO: JAVA版ではデフォルトは例外スローしていたが、C#ではデフォルトスタイルと判定する。
                _ => allowNumericHenkakuPattern
            };
        }
    }
}
