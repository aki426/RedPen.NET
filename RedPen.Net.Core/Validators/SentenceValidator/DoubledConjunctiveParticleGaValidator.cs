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

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>DoubledConjunctiveParticleGaのConfiguration</summary>
    public record DoubledConjunctiveParticleGaConfiguration : ValidatorConfiguration
    {
        public DoubledConjunctiveParticleGaConfiguration(ValidationLevel level) : base(level)
        {
        }
    }

    // DoubledConjunctiveParticleGaValidator checks if an input Japanese sentence has
    // multiple conjunctive particles, "ga".
    //
    // Note: This validator is a port from textlint-rule-no-doubled-conjunctive-particle-ga written by takahashim
    // <https://github.com/textlint-ja/textlint-rule-no-doubled-conjunctive-particle-ga>
    // Note: this validator works only for Japanese texts.

    /// <summary>DoubledConjunctiveParticleGaのValidator</summary>
    public class DoubledConjunctiveParticleGaValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public DoubledConjunctiveParticleGaConfiguration Config { get; init; }

        /// <summary></summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

        public DoubledConjunctiveParticleGaValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            DoubledConjunctiveParticleGaConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;
        }

        /// <summary>
        /// 一文に二回以上、接続助詞の「が」が出現するとエラーを出力します。
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <returns>A list of ValidationErrors.</returns>
        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            // validation
            var gaList = sentence.Tokens.Where(t =>
                2 <= t.PartOfSpeech.Count
                && t.Surface == "が"
                && t.PartOfSpeech[0] == "助詞"
                && t.PartOfSpeech[1] == "接続助詞");

            if (gaList.Count() > 1)
            {
                // TODO: MessageKey引数はErrorMessageにバリエーションがある場合にValidator内で条件判定して引数として与える。
                result.Add(new ValidationError(
                    ValidationName,
                    this.Level,
                    sentence,
                    gaList.First().OffsetMap[0],
                    gaList.First().OffsetMap[^1],
                    MessageArgs: new object[] { gaList.First().Surface }));
            }

            return result;
        }
    }
}
