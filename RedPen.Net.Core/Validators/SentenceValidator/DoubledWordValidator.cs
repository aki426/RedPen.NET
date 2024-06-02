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
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>DoubledWordのConfiguration</summary>
    public record DoubledWordConfiguration : ValidatorConfiguration, IWordSetConfigParameter, IMinLengthConfigParameter
    {
        public HashSet<string> WordSet { get; init; }
        public int MinLength { get; init; }

        public DoubledWordConfiguration(ValidationLevel level, HashSet<string> wordSet, int minLength) : base(level)
        {
            this.WordSet = wordSet;
            this.MinLength = minLength;
        }
    }

    /// <summary>DoubledWordのValidator</summary>
    public class DoubledWordValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public DoubledWordConfiguration Config { get; init; }

        // TODO: コンストラクタの引数定義は共通にすること。
        public DoubledWordValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            DoubledWordConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;

            // TODO: ja-JPの場合のみMinLengthを1にすることが妥当かどうかはテストケースなどで要検討。
            if (documentLangForTest.Name == "ja-JP")
            {
                this.Config = config with { MinLength = 1 };
            }
        }

        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            // validation
            HashSet<string> surfaces = new HashSet<string>();
            foreach (TokenElement token in sentence.Tokens)
            {
                string word = token.Surface.ToLower();
                if ((Config.MinLength <= word.Length) // MinLengthを含み、MinLength以上の文字数の場合エラーとみなす。
                    && surfaces.Contains(word) // 2回以上出現したSurfaceをエラーとみなす。
                    && !Config.WordSet.Contains(word))
                {
                    result.Add(new ValidationError(
                        ValidationName,
                        this.Level,
                        sentence,
                        token.OffsetMap[0],
                        token.OffsetMap[^1],
                        MessageArgs: new object[] { token.Surface }));
                }
                surfaces.Add(word);
            }

            return result;
        }
    }
}
