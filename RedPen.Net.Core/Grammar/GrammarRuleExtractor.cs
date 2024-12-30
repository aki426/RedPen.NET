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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Grammar
{
    /// <summary>
    /// RedPenのルール表記文字列から文法ルールを抽出するクラス。
    /// </summary>
    public static class GrammarRuleExtractor
    {
        /// <summary>
        /// RuleExpressionを定義された複数行テキストからGrammarRuleのリストを読み込む。
        /// MEMO: 冒頭に#記号があるばあいはコメントとして無視される。
        /// </summary>
        /// <param name="ruleDefinition">テキストファイルの中身などの複数行テキスト</param>
        /// <returns>A list of GrammarRules.</returns>
        public static List<GrammarRule> LoadGrammarRules(string ruleDefinition)
        {
            return ruleDefinition.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => line != "" && line.Trim()[0] != '#')
                .Select(line => Run(line)).ToList();
        }

        /// <summary>
        /// 1つのTokenパターン文字列をTokenElementへ変換する関数。
        /// </summary>
        /// <param name="str">
        /// Sruface : Reading : Pos : InflectionForm : InflectionType : BaseForm
        /// Posは品詞-品詞細分類1-品詞細分類2-品詞細分類3の4つが格納されるスロットで区切り文字は"-"
        /// </param>
        /// <returns>A TokenElement.</returns>
        public static TokenElement ConvertToToken(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            else if (str.Trim() == "")
            {
                throw new ArgumentException("Expected TokenElement's text expression. But str is empty.", nameof(str));
            }

            // Surface:タグ,タグ,タグ,...:Readingを要素ごとに分割。
            var wordSegments = str.Split(':');

            var surface =
                wordSegments[0].Trim().ToLower();
            var reading =
                wordSegments.Length > 1 ? wordSegments[1].Trim() : "";
            var pos =
                wordSegments.Length > 2 ? wordSegments[2].Trim() : "";
            var infForm =
                wordSegments.Length > 3 ? wordSegments[3].Trim() : "";
            var infType =
                wordSegments.Length > 4 ? wordSegments[4].Trim() : "";
            var baseForm =
                wordSegments.Length > 5 ? wordSegments[5].Trim() : "";

            return new TokenElement(
                surface,
                reading,
                "",
                pos.Split('-').Select(t => t.Trim()).ToImmutableList(),
                baseForm,
                infType,
                infForm,
                ImmutableList<LineOffset>.Empty
            );
        }

        /// <summary>
        /// ルール文字列からGrammerRuleを作成する関数。
        /// 1つのTokenパターンは":"区切りで「Sruface : Reading : Pos : InflectionForm : InflectionType : BaseForm」と表現される。
        /// Tokenパターンをつなぐ際、「+」は隣接、「=」は非隣接を表す。
        /// </summary>
        /// <param name="line">Tokenパターンを1つ以上含むルール文字列</param>
        /// <returns></returns>
        public static GrammarRule Run(string line)
        {
            if (line == null || line.Trim() == "")
            {
                throw new ArgumentException("Invalid rule format. Rule expression is empty.", nameof(line));
            }

            var result = new List<(bool direct, TokenElement token)>();
            var sb = new StringBuilder();
            var tokenIsDirect = true; // 最初のトークンの直接接続フラグは処理の一貫性からTrueにしておく。

            foreach (var c in line)
            {
                if (c == '=')
                {
                    if (sb.Length == 0)
                    {
                        throw new ArgumentException("Invalid rule format. One token must exist before '='.", nameof(line));
                    }

                    result.Add((tokenIsDirect, ConvertToToken(sb.ToString())));

                    // iteration
                    // NOTE: '='の次のトークンは、離れた場所に出現しても良いルール。
                    tokenIsDirect = false;
                    sb.Clear();
                }
                else if (c == '+')
                {
                    if (sb.Length == 0)
                    {
                        throw new ArgumentException("Invalid rule format. One token must exist before '+'.", nameof(line));
                    }

                    result.Add((tokenIsDirect, ConvertToToken(sb.ToString())));

                    // iteration
                    // NOTE: '+'の次のトークンは、直前のトークンに続いて出現する必要があるルール。
                    tokenIsDirect = true;
                    sb.Clear();
                }
                else if (c == ' ')
                {
                    // MEMO: 空白文字は無視する。
                }
                else
                {
                    sb.Append(c);
                }
            }

            if (sb.Length == 0)
            {
                throw new ArgumentException("Invalid rule format. One token must exist after the last '+' or '='.", nameof(line));
            }

            result.Add((tokenIsDirect, ConvertToToken(sb.ToString())));

            return new GrammarRule()
            {
                Pattern = result.ToImmutableList()
            };
        }
    }
}
