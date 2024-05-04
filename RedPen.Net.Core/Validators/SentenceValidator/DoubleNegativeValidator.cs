using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Utility;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>DoubleNegativeのConfiguration</summary>
    public record DoubleNegativeConfiguration : ValidatorConfiguration
    {
        public DoubleNegativeConfiguration(ValidationLevel level) : base(level)
        {
        }
    }

    /// <summary>DoubleNegativeのValidator</summary>
    public class DoubleNegativeValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public DoubleNegativeConfiguration Config { get; init; }

        // TODO: サポート対象言語がANYではない場合overrideで再定義する。
        /// <summary></summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP", "en-US" };

        private List<ExpressionRule> invalidExpressions;
        private HashSet<string> negativeWords;
        private bool matchAsReading = false;

        // TODO: コンストラクタの引数定義は共通にすること。

        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleNegativeValidator"/> class.
        /// </summary>
        /// <param name="documentLangForTest">The document lang for test.</param>
        /// <param name="symbolTable">The symbol table.</param>
        /// <param name="config">The config.</param>
        public DoubleNegativeValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            DoubleNegativeConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;

            // MEMO: JAVA版を踏襲し、Configurationによる設定ではなく、デフォルトリソースのみを利用する。
            // TODO: Configurableにした方が良いかどうかはユースケース考えて検討する。
            if (documentLangForTest.Name == "ja-JP")
            {
                invalidExpressions = DefaultResources.DoubleNegativeExpression_ja
                    .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => ExpressionRuleExtractor.Run(line)).ToList();

                negativeWords = new HashSet<string>(DefaultResources.DoubleNegativeWord_ja
                    .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Trim().ToLower()));

                // MEMO: 日本語の場合は漢字／ひらがな表記のゆれがあるので、Readingでのマッチングを行う。
                matchAsReading = true;
            }
            else
            {
                invalidExpressions = DefaultResources.DoubleNegativeExpression_en
                    .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => ExpressionRuleExtractor.Run(line)).ToList();

                negativeWords = new HashSet<string>(DefaultResources.DoubleNegativeWord_en
                    .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Trim().ToLower()));
            }
        }

        /// <summary>
        /// 二重否定表現の検出。
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <returns>A list of ValidationErrors.</returns>
        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            // ExpressionRuleを用いた判定を行う。
            foreach (ExpressionRule rule in invalidExpressions)
            {
                // 設定によってReadingでマッチするかSurfaceでマッチするかを切り替える。
                var (isMatch, matchedPhrases) = matchAsReading ?
                    rule.MatchReadings(sentence.Tokens) : rule.MatchSurfaces(sentence.Tokens);

                if (isMatch)
                {
                    foreach (ImmutableList<TokenElement> errorPhrase in matchedPhrases)
                    {
                        result.Add(new ValidationError(
                            ValidationType.DoubleNegative,
                            this.Level,
                            sentence,
                            errorPhrase.First().OffsetMap[0],
                            errorPhrase.Last().OffsetMap[^1],
                            MessageArgs: new object[] { string.Join("", errorPhrase.Select(t => t.Surface)) }
                        ));
                    }

                    //return;
                }
            }

            // ExpressionRuleにより二重否定表現が見つかった場合はその時点で判定を終了する。
            if (result.Any())
            {
                return result;
            }

            // ExpressionRuleで二重否定表現が見つからなかった場合は、否定語の出現回数をカウントし2回以上出現していた場合はエラーとみなす。
            // MEMO: 現時点では否定語の出現回数判定は英語のみを想定している。
            List<TokenElement> negativeTokens = new List<TokenElement>();
            foreach (TokenElement token in sentence.Tokens)
            {
                if (negativeWords.Contains(token.Surface.ToLower()))
                {
                    negativeTokens.Add(token);
                }

                if (negativeTokens.Count >= 2)
                {
                    result.Add(new ValidationError(
                        ValidationType.DoubleNegative,
                        this.Level,
                        sentence,
                        negativeTokens.First().OffsetMap[0],
                        negativeTokens.Last().OffsetMap[^1],
                        MessageArgs: new object[] { string.Join(" + ", negativeTokens.Select(t => t.Surface)) }
                    ));

                    // 否定語が2回以上出現していた場合はその時点で判定を打ち切る。
                    return result;
                }
            }

            return result;
        }
    }
}
