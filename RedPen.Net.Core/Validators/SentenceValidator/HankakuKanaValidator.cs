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
using System.Text.RegularExpressions;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>HankakuKanaのConfiguration</summary>
    public record HankakuKanaConfiguration : ValidatorConfiguration
    {
        public HankakuKanaConfiguration(ValidationLevel level) : base(level)
        {
        }
    }

    // TODO: Validation対象に応じて、IDocumentValidatable, ISectionValidatable, ISentenceValidatableを実装する。
    public class HankakuKanaValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public HankakuKanaConfiguration Config { get; init; }

        /// <summary></summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

        // TODO: コンストラクタの引数定義は共通にすること。
        public HankakuKanaValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            HankakuKanaConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;
        }

        /// <summary>半角カナのUnicode文字範囲指定パターン
        /// MEMO: 本来は\uFF61から\uFF9Fの範囲だが、"｡｢｣､"の4文字は半角だが非該当としている。
        /// 一方、"･ｰﾞﾟ"の4文字は半角カナの一部とみなしている。</summary>
        private static Regex hankakuKanaPattern = new Regex(@"[\uFF65-\uFF9F]+");

        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            // validation
            MatchCollection matches = hankakuKanaPattern.Matches(sentence.Content);
            foreach (Match match in matches)
            {
                // AddLocalizedError(sentence, content[match.Index]);
                // TODO: MessageKey引数はErrorMessageにバリエーションがある場合にValidator内で条件判定して引数として与える。
                result.Add(new ValidationError(
                    ValidationName,
                    this.Level,
                    sentence,
                    sentence.ConvertToLineOffset(match.Index),
                    sentence.ConvertToLineOffset(match.Index + match.Length - 1),
                    MessageArgs: new object[] { match.Value }));
            }

            return result;
        }
    }
}
