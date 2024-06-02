//   Copyright (c) 2024 KANEDA Akihiro <taoist.aki@gmail.com>
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

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

        private List<GrammarRule> invalidExpressions;
        private HashSet<string> negativeWords;
        private bool isJapaneseMatchAsReading = false;

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
                invalidExpressions = GrammarRuleExtractor.LoadGrammarRules(DefaultResources.DoubleNegativeExpression_ja);
                negativeWords = ResourceFileLoader.LoadWordSet(DefaultResources.DoubleNegativeWord_ja);

                // MEMO: 日本語の場合は漢字／ひらがな表記のゆれがあるので、Readingでのマッチングを行う。
                isJapaneseMatchAsReading = true;
            }
            else
            {
                invalidExpressions = GrammarRuleExtractor.LoadGrammarRules(DefaultResources.DoubleNegativeExpression_en);
                negativeWords = ResourceFileLoader.LoadWordSet(DefaultResources.DoubleNegativeWord_en);
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

            // GrammarRuleを用いた判定を行う。
            foreach (GrammarRule rule in invalidExpressions)
            {
                // 設定によってReadingでマッチするかSurfaceでマッチするかを切り替える。
                var (isMatch, matchedPhrases) = isJapaneseMatchAsReading ?
                    rule.MatchesConsecutiveReadings(sentence.Tokens.Where(t =>
                        // MEMO: 日本語の場合、「、」や半角全角スペースは無視してマッチングする。
                        // Token列にムダな記号が入ることでマッチングが外れることがあるため。
                        t.Surface != SymbolTable.GetValueOrFallbackToDefault(SymbolType.COMMA).ToString()
                        && t.Surface != " "
                        && t.Surface != "　").ToList())
                    : rule.MatchesConsecutiveSurfaces(sentence.Tokens);

                if (isMatch)
                {
                    foreach (ImmutableList<TokenElement> errorPhrase in matchedPhrases)
                    {
                        result.Add(new ValidationError(
                            ValidationName,
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

            // GrammarRuleにより二重否定表現が見つかった場合はその時点で判定を終了する。
            if (result.Any())
            {
                return result;
            }

            // GrammarRuleで二重否定表現が見つからなかった場合は、否定語の出現回数をカウントし2回以上出現していた場合はエラーとみなす。
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
                        ValidationName,
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
