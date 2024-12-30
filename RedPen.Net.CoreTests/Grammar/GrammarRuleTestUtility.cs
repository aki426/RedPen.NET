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
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Grammar;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Parser.Tests;
using Xunit.Abstractions;

namespace RedPen.Net.CoreTests.Grammar
{
    /// <summary>
    /// GrammarRuleのテストケース用便利クラス。
    /// </summary>
    internal static class GrammarRuleTestUtility
    {
        /// <summary>
        /// テスト対象のテキストとRuleについて、Ruleにマッチする箇所が何個あるか、マッチ文字列の一致をAssertする関数。
        /// マッチ関数にはMatchExtendを使用する。
        /// </summary>
        /// <param name="rule">ルール文字列</param>
        /// <param name="text">検証対象の文字列</param>
        /// <param name="expectedCount">ルールマッチが期待される回数</param>
        /// <param name="expectedStr">ルールマッチした結果の文字列</param>
        /// <param name="output">呼び出し元のユニットテストから出力先ITestOutputHelperを渡す</param>
        public static void TestMatchExtend(string rule, string text, int expectedCount, string expectedStr, ITestOutputHelper output)
        {
            // Rule
            GrammarRule grammarRule = GrammarRuleExtractor.Run(rule);

            output.WriteLine("★Rule...");
            foreach (TokenElement token in grammarRule.Tokens)
            {
                output.WriteLine(token.ToString());
            }
            output.WriteLine("");

            List<ImmutableList<TokenElement>> actual = new List<ImmutableList<TokenElement>>();

            // textをDocumentに変換し、全SentenceごとにRuleとのマッチングを取る。
            Document doc = new PlainTextParserTests(output).GenerateDocument(text, "ja-JP");
            foreach (var sentence in doc.GetAllSentences())
            {
                output.WriteLine("★Sentence...");
                foreach (TokenElement token in sentence.Tokens)
                {
                    output.WriteLine(token.ToString());
                }
                output.WriteLine("");

                actual.AddRange(grammarRule.MatchExtend(sentence.Tokens.ToImmutableList()));
            }

            // Assert
            actual.Count.Should().Be(expectedCount);

            if (actual.Count == 0)
            {
                return;
            }

            // "|"でマッチしたトークンのすべてのSurfaceを連結してexpectedと比較する。
            string.Join("|", actual.Select(lis => string.Join("|", lis.Select(t => t.Surface))))
                .Should().Contain(expectedStr);
        }
    }
}
