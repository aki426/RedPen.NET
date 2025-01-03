﻿//   Copyright (c) 2024 KANEDA Akihiro <taoist.aki@gmail.com>
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
using System.Globalization;
using System.Linq;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>DoubledJoshiのConfiguration</summary>
    public record DoubledJoshiConfiguration : ValidatorConfiguration, IMaxIntervalConfigParameter, IWordSetConfigParameter
    {
        public int MaxInterval { get; init; }

        public HashSet<string> WordSet { get; init; }

        public DoubledJoshiConfiguration(
            ValidationLevel level,
            int maxInterval,
            HashSet<string> wordSet) : base(level)
        {
            this.MaxInterval = maxInterval;
            this.WordSet = wordSet;
        }
    }

    /// <summary>DoubledJoshiのValidator</summary>
    public class DoubledJoshiValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public DoubledJoshiConfiguration Config { get; init; }

        /// <summary></summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

        public DoubledJoshiValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            DoubledJoshiConfiguration config) :
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
            Dictionary<string, List<TokenElement>> joshiCache = new Dictionary<string, List<TokenElement>>();
            foreach (TokenElement token in sentence.Tokens)
            {
                if (token.PartOfSpeech[0] == "助詞" &&
                    !Config.WordSet.Contains(token.Surface))
                {
                    if (!joshiCache.ContainsKey(token.Surface))
                    {
                        joshiCache[token.Surface] = new List<TokenElement>();
                    }

                    joshiCache[token.Surface].Add(token);
                }
            }

            // create errors
            foreach (string joshi in joshiCache.Keys)
            {
                // 助詞の出現回数が1以下の場合はDoubledではないのでスキップ。
                if (joshiCache[joshi].Count < 2)
                {
                    continue;
                }

                // 現在問題にしている助詞について、2つの出現箇所の距離を計測して基準値以下であればエラーとする。
                // 2つ目の要素から初めて末尾までたどる。
                TokenElement prev = joshiCache[joshi][0];
                foreach (TokenElement token in joshiCache[joshi].Skip(1))
                {
                    // 2つの助詞のIndex距離を計算する。
                    // TODO: これは単語数をカウントしているので、ユーザにとって文字数の方が自然なら仕様変更した方が良いかもしれない。
                    int prevIndex = sentence.Tokens.IndexOf(prev);
                    int currIndex = sentence.Tokens.IndexOf(token);

                    prev = token;

                    // 存在しないはずはないが念のため。
                    if (prevIndex == -1 || currIndex == -1)
                    {
                        continue;
                    }

                    // インターバル以下の距離で出現した場合はエラー。
                    if (Math.Abs(currIndex - prevIndex) - 1 <= Config.MaxInterval)
                    {
                        result.Add(new ValidationError(
                            ValidationName,
                            this.Level,
                            sentence,
                            token.OffsetMap[0],
                            token.OffsetMap[^1],
                            MessageArgs: new object[] { token.Surface }));
                    }
                }
            }

            return result;
        }
    }
}
