//   Copyright (c) 2025 KANEDA Akihiro <taoist.aki@gmail.com>
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

namespace RedPen.Net.Core.Grammar
{
    /// <summary>
    /// 日本語の常体敬体表現の変換情報を管理するためのクラス
    /// </summary>
    public class PolitenessPattern
    {
        /// <summary>日本語文法の何に対応するかを表す説明的文章。</summary>
        public string GrammaticalUsage { get; init; }

        /// <summary>常体のルール</summary>
        public GrammarRule JotaiRule { get; init; }

        // NOTE: Jotai/KeitaiSuffixは、ルールにひらなが表現を含まない方がマッチングしやすいため別に用意した。

        /// <summary>標準的な常体の語尾表現。NOTE: 「笑わない」における「ない」。</summary>
        public string JotaiSuffix { get; init; }

        /// <summary>敬体のルール</summary>
        public GrammarRule KeitaiRule { get; init; }

        /// <summary>標準的な敬体の語尾表現。NOTE: 「笑いません」における「ません」</summary>
        public string KeitaiSuffix { get; init; }

        /// <summary>デフォルトコンストラクタ</summary>
        public PolitenessPattern(
            string grammaticalUsage,
            GrammarRule jotaiRule,
            string jotaiSuffix,
            GrammarRule keitaiRule,
            string keitaiSuffix
    )
        {
            GrammaticalUsage = grammaticalUsage;
            JotaiRule = jotaiRule;
            JotaiSuffix = jotaiSuffix;
            KeitaiRule = keitaiRule;
            KeitaiSuffix = keitaiSuffix;
        }

        /// <summary>テキスト情報からロードする際のコンストラクタ</summary>
        public PolitenessPattern(
            string grammaticalUsage,
            string jotaiRuleStr,
            string jotaiSuffix,
            string keitaiRuleStr,
            string keitaiSuffix
        ) : this(
            grammaticalUsage,
            GrammarRuleExtractor.Run(jotaiRuleStr),
            jotaiSuffix,
            GrammarRuleExtractor.Run(keitaiRuleStr),
            keitaiSuffix
        )
        { }
    }
}
