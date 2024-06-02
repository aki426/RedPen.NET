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
                        ValidationName,
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
