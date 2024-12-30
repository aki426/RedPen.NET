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
using System.Data;
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Parser.Tests;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Grammar.Tests
{
    public class GrammarRuleTests
    {
        private readonly ITestOutputHelper output;

        public GrammarRuleTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// GrammarRule.ReversedPattern()を検証するテスト
        /// </summary>
        [Fact()]
        public void ReversedPatternTest()
        {
            // 基本的な検証
            GrammarRuleExtractor.Run("a + b = c").ToString()
                .Should().Be("a + b = c");
            // 文字列で検証
            new GrammarRule() { Pattern = GrammarRuleExtractor.Run("a + b = c").ReversedPattern }.ToString()
                .Should().Be("c = b + a");

            // ReversedPatternの検証
            GrammarRuleExtractor.Run("a + b = c").ReversedPattern
                .Should().BeEquivalentTo(
                    GrammarRuleExtractor.Run("c = b + a").Pattern,
                    options => options.WithStrictOrdering().ComparingByMembers<TokenElement>());

            GrammarRuleExtractor.Run("::動詞:未然形 + :ナイ").ReversedPattern
                .Should().BeEquivalentTo(
                    GrammarRuleExtractor.Run(":ナイ + ::動詞:未然形").Pattern,
                    options => options.WithStrictOrdering().ComparingByMembers<TokenElement>());

            // NGパターン
            GrammarRuleExtractor.Run("a + b = c").ReversedPattern
                .Should().NotBeEquivalentTo(
                    GrammarRuleExtractor.Run("c + b + a").Pattern,
                    options => options.WithStrictOrdering().ComparingByMembers<TokenElement>());

            // InflectionFormの有無を検知する。
            GrammarRuleExtractor.Run("::動詞:未然形 + :ナイ").ReversedPattern
                .Should().NotBeEquivalentTo(
                    GrammarRuleExtractor.Run(":ナイ + ::動詞:").Pattern,
                    options => options.WithStrictOrdering().ComparingByMembers<TokenElement>());

            // 隣接／非隣接を区別する。
            GrammarRuleExtractor.Run("::動詞:未然形 + :ナイ").ReversedPattern
                .Should().NotBeEquivalentTo(
                    GrammarRuleExtractor.Run(":ナイ = ::動詞:未然形").Pattern,
                    options => options.WithStrictOrdering().ComparingByMembers<TokenElement>());
        }

        /// <summary>
        /// センテンス末尾のマッチングのみ行うMatchExtendAtEndの動作確認テスト
        /// </summary>
        [Fact()]
        public void MatchExtendAtEndTest()
        {
            string rule = "::動詞 + :ナイ";
            string text = "見ない、言わない、聞かない。";
            int expectedCount = 1;
            string expectedStr = "聞か|ない";

            // Rule
            GrammarRule grammarRule = GrammarRuleExtractor.Run(rule);

            output.WriteLine("★Rule...");
            foreach (TokenElement token in grammarRule.Tokens)
            {
                output.WriteLine(token.ToString());
            }
            output.WriteLine("");

            // textをDocumentに変換し、全SentenceごとにRuleとのマッチングを取る。
            Document doc = new PlainTextParserTests(output).GenerateDocument(text, "ja-JP");
            List<ImmutableList<TokenElement>> actual = new List<ImmutableList<TokenElement>>();
            foreach (var sentence in doc.GetAllSentences())
            {
                output.WriteLine("★Sentence...");
                foreach (TokenElement token in sentence.Tokens)
                {
                    output.WriteLine(token.ToString());
                }
                output.WriteLine("");

                // 記号除去（末尾の「。」などを除去したいため）。
                var (success, tokens) =
                    grammarRule.MatchExtendAtEnd(sentence.Tokens.Where(x => x.PartOfSpeech[0] != "記号").ToImmutableList());
                if (success)
                {
                    actual.Add(tokens);
                }
            }

            // Assert
            actual.Count.Should().Be(expectedCount);

            if (actual.Count == 0)
            {
                return;
            }

            // "|"でマッチしたトークンのすべてのSurfaceを連結してexpectedと比較する。
            var actualStr = string.Join("|", actual.Select(lis => string.Join("|", lis.Select(t => t.Surface))));
            output.WriteLine("★Matched...");
            output.WriteLine(actualStr);

            actualStr.Should().Contain(expectedStr);
        }
    }
}
