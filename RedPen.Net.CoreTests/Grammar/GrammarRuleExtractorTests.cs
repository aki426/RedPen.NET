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
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Parser.Tests;
using RedPen.Net.CoreTests.Grammar;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Grammar.Tests
{
    public class GrammarRuleExtractorTests
    {
        private ITestOutputHelper output;

        public GrammarRuleExtractorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>ルール表現文字列からルールを抽出するテスト。</summary>
        [Fact]
        public void RunTest01()
        {
            GrammarRule rule = GrammarRuleExtractor.Run("This::n-noun + is::v");
            // MEMO: ルール文字列はマッチングのためすべて小文字に変換される。
            rule.ToSurface().Should().Be("thisis");

            foreach (TokenElement token in rule.Tokens)
            {
                output.WriteLine(token.ToString());
            }

            rule.Tokens[0].Surface.Should().Be("this");
            rule.Tokens[0].PartOfSpeech.Count.Should().Be(2);
            rule.Tokens[0].PartOfSpeech[0].Should().Be("n");
            rule.Tokens[0].PartOfSpeech[1].Should().Be("noun");
            rule.Tokens[1].Surface.Should().Be("is");
            rule.Tokens[1].PartOfSpeech.Count.Should().Be(1);
            rule.Tokens[1].PartOfSpeech[0].Should().Be("v");
        }

        [Fact]
        public void RunTest02()
        {
            var rule = GrammarRuleExtractor.Run("This::n + is::v");
            rule.Tokens.Count.Should().Be(2);

            rule.Pattern[0].Adjacent.Should().BeTrue();
            rule.Pattern[0].Token.Surface.Should().Be("this");
            rule.Pattern[0].Token.PartOfSpeech.Should().Contain("n");
            rule.Pattern[0].Token.Reading.Should().Be("");

            rule.Pattern[1].Adjacent.Should().BeTrue();
            rule.Pattern[1].Token.Surface.Should().Be("is");
            rule.Pattern[1].Token.PartOfSpeech.Should().Contain("v");
            rule.Pattern[1].Token.Reading.Should().Be("");

            rule = GrammarRuleExtractor.Run("僕:ボク:名詞 = は:ハ:助詞");
            rule.Tokens.Count.Should().Be(2);

            rule.Pattern[0].Adjacent.Should().BeTrue();
            rule.Pattern[0].Token.Surface.Should().Be("僕");
            rule.Pattern[0].Token.PartOfSpeech.Should().Contain("名詞");
            rule.Pattern[0].Token.Reading.Should().Be("ボク");

            rule.Pattern[1].Adjacent.Should().BeFalse();
            rule.Pattern[1].Token.Surface.Should().Be("は");
            rule.Pattern[1].Token.PartOfSpeech.Should().Contain("助詞");
            rule.Pattern[1].Token.Reading.Should().Be("ハ");

            // empty strings
            Action action = () => GrammarRuleExtractor.Run(null);
            action.Should().Throw<ArgumentException>();

            action = () => GrammarRuleExtractor.Run("");
            action.Should().Throw<ArgumentException>();

            action = () => GrammarRuleExtractor.Run("  ");
            action.Should().Throw<ArgumentException>();

            action = () => GrammarRuleExtractor.Run(" + ");
            action.Should().Throw<ArgumentException>();

            // aster '+' is missing
            action = () => GrammarRuleExtractor.Run("This::n + is::v + ");
            action.Should().Throw<ArgumentException>();

            // aster '=' is missing
            action = () => GrammarRuleExtractor.Run("This::n + is::v = ");
            action.Should().Throw<ArgumentException>();
        }

        /// <summary>
        /// ルール文字列を解釈する際に空白を無視することの確認。
        /// </summary>
        [Fact]
        public void ToStringTest()
        {
            // 元の入力文法ルール文字列にパターンマッチの結果は同じになるが不要な構成要素が有る場合、省略して出力する。
            GrammarRuleExtractor.Run("  This : :n :      + is: : v : * ").ToString().Should().Be("this::n + is::v:*");
            GrammarRuleExtractor.Run("  僕:  ボク :  名詞- =            は:       ハ :助詞").ToString().Should().Be("僕:ボク:名詞 = は:ハ:助詞");
            GrammarRuleExtractor.Run("  : ボク + : ハ").ToString().Should().Be(":ボク + :ハ");
        }

        /// <summary>ルール表現文字列から抽出したルールのマッチングテスト。</summary>
        [Theory]
        [InlineData("001", ":ナイ + :ト = :ナイ", "そんなことが無いとは言えない。", 1, "無い|と|は|言え|ない")]
        [InlineData("002", ":ナイ + :ト = :ナイ", "そんなことが無いとは言えません。", 0, "")]
        [InlineData("003", ":ナイ + :ト = :ナイ", "そんなことが無いとは限らない。", 1, "無い|と|は|限ら|ない")]
        [InlineData("004", ":ナイ + :ト = :ナイ", "そんなことが無いと断定することは出来ないだろう。", 1, "無い|と|断定|する|こと|は|出来|ない")]
        // NOTE: 品詞を指定しないと想定外のものにマッチしてしまう。
        [InlineData("005", ":ナイ + :ト = :ナイ", "水が無いとは想定内だ。", 1, "無い|と|は|想定|内")]
        // NOTE: 品詞を指定することで助動詞の「ない」にだけマッチする。
        [InlineData("006", ":ナイ + :ト = :ナイ:助動詞-特殊・ナイ", "水が無いとは想定内だ。", 0, "")]
        // ひらがな表現でも適切な品詞であればマッチする。
        [InlineData("007", ":ナイ + :ト = :ナイ", "そんなことがないとは言えない。", 1, "ない|と|は|言え|ない")]
        [InlineData("008", ":ナイ:形容詞-自立 + :ト = :ナイ", "そんなことがないとは言えない。", 1, "ない|と|は|言え|ない")]
        // 形容詞か助動詞かはケースバイケースなのでそれぞれ指定するしかない。
        [InlineData("009", ":ナイ + :ト = :ナイ", "食べないとは口が裂けても言えない状況。", 1, "ない|と|は|口|が|裂け|て|も|言え|ない")]
        [InlineData("010", ":ナイ:助動詞-特殊・ナイ + :ト = :ナイ:助動詞-特殊・ナイ", "食べないとは口が裂けても言えない状況。", 1,
            "ない|と|は|口|が|裂け|て|も|言え|ない")]
        [InlineData("011", ":ナイ + :ト = :ナイ", "水が無いとは想定外だし、食料も無い。", 1,
            "無い|と|は|想定|外|だ|し|、|食料|も|無い")]
        [InlineData("012", ":ナイ:形容詞-自立 + :ト = :ナイ:助動詞-特殊・ナイ", "水が無いとは想定外だし、食料も無い。", 0, "")]
        [InlineData("013", ":ナイ + :ト = :ナイ", "これは想定内とは言えない。", 1, "内|と|は|言え|ない")]
        [InlineData("014", ":ナイ:形容詞-自立 + :ト = :ナイ:助動詞-特殊・ナイ", "これは想定内とは言えない。", 0, "")]
        public void JapaneseMatchesExtendTest(string nouse1, string rule, string text, int matchCount, string expected)
        {
            GrammarRuleTestUtility.TestMatchExtend(rule, text, matchCount, expected, output);
        }

        /// <summary>ルール表現文字列から抽出したルールのマッチングテスト。</summary>
        [Theory]
        [InlineData("001", "This:n + is:v", "He said , This is a pen .", 1, "This|is")]
        [InlineData("002", "a + pen:n", "This is a pen and a pen.", 2, "a|pen|a|pen")]
        [InlineData("003", "a + pen:n", "This is a pen. He has a pen.", 2, "a|pen|a|pen")]
        public void MatchSurfaceTest(string nouse1, string rule, string text, int matchCount, string expected)
        {
            GrammarRule grammarRule = GrammarRuleExtractor.Run(rule);

            output.WriteLine("★Rule...");
            foreach (TokenElement token in grammarRule.Tokens)
            {
                output.WriteLine(token.ToString());
            }
            output.WriteLine("");

            List<ImmutableList<TokenElement>> matchedTokensList = new List<ImmutableList<TokenElement>>();

            Document doc = new PlainTextParserTests(output).GenerateDocument(text, "en-US");
            foreach (var sentence in doc.GetAllSentences())
            {
                output.WriteLine("★Sentence...");
                foreach (TokenElement token in sentence.Tokens)
                {
                    output.WriteLine(token.ToString());
                }
                output.WriteLine("");

                //(bool isMatch, List<TokenElement> tokens1) =
                (bool isMatch, List<ImmutableList<TokenElement>> tokens) value =
                    grammarRule.MatchesConsecutiveSurfaces(sentence.Tokens);

                matchedTokensList.AddRange(value.tokens);
            }

            matchedTokensList.Count.Should().Be(matchCount);

            // "|"ですべてのSurfaceを連結して比較する。
            string.Join("|", matchedTokensList.Select(lis => string.Join("|", lis.Select(t => t.Surface))))
                .Should().Contain(expected);
        }

        /// <summary>ルール表現文字列から抽出したルールのマッチングテスト。</summary>
        [Theory]
        [InlineData("001", "本日+は", "本日は晴天なり。", 1, "本日|は")]
        [InlineData("002", "本日+は", "本日は晴天なり、本日は曇天なり。", 2, "本日|は|本日|は")]
        public void JapaneseMatchSurfaceTest(string nouse1, string rule, string text, int matchCount, string expected)
        {
            GrammarRule grammarRule = GrammarRuleExtractor.Run(rule);

            output.WriteLine("★Rule...");
            foreach (TokenElement token in grammarRule.Tokens)
            {
                output.WriteLine(token.ToString());
            }
            output.WriteLine("");

            List<ImmutableList<TokenElement>> matchedTokensList = new List<ImmutableList<TokenElement>>();

            Document doc = new PlainTextParserTests(output).GenerateDocument(text, "ja-JP");
            foreach (var sentence in doc.GetAllSentences())
            {
                output.WriteLine("★Sentence...");
                foreach (TokenElement token in sentence.Tokens)
                {
                    output.WriteLine(token.ToString());
                }
                output.WriteLine("");

                //(bool isMatch, List<TokenElement> tokens1) =
                (bool isMatch, List<ImmutableList<TokenElement>> tokens) value =
                    grammarRule.MatchesConsecutiveSurfaces(sentence.Tokens);

                if (value.isMatch)
                {
                    matchedTokensList.AddRange(value.tokens);
                }
            }

            matchedTokensList.Count.Should().Be(matchCount);

            // "|"ですべてのSurfaceを連結して比較する。
            string.Join("|", matchedTokensList.Select(lis => string.Join("|", lis.Select(t => t.Surface))))
                .Should().Contain(expected);
        }

        /// <summary>ルール表現文字列から抽出したルールのマッチングテスト。</summary>
        [Theory]
        [InlineData("001", "*:名詞 + の:助詞 + *:名詞 + の:助詞 + *:名詞", "つまり弊社の方針の説明とはこうです。", 1, "弊社|の|方針|の|説明")]
        [InlineData("002", "*:名詞 + の:助詞 + *:名詞 + の:助詞 + *:名詞", "つまり弊社の方針とはこうです。", 0, "")]
        public void JapaneseMatchSurfaceAndTagsTest(string nouse1, string rule, string text, int matchCount, string expected)
        {
            GrammarRule grammarRule = GrammarRuleExtractor.Run(rule);

            output.WriteLine("★Rule...");
            foreach (TokenElement token in grammarRule.Tokens)
            {
                output.WriteLine(token.ToString());
            }
            output.WriteLine("");

            List<ImmutableList<TokenElement>> matchedTokensList = new List<ImmutableList<TokenElement>>();

            Document doc = new PlainTextParserTests(output).GenerateDocument(text, "ja-JP");
            foreach (var sentence in doc.GetAllSentences())
            {
                output.WriteLine("★Sentence...");
                foreach (TokenElement token in sentence.Tokens)
                {
                    output.WriteLine(token.ToString());
                }
                output.WriteLine("");

                //(bool isMatch, List<TokenElement> tokens1) =
                (bool isMatch, List<ImmutableList<TokenElement>> tokens) value =
                    grammarRule.MatchesConsecutiveSurfacesAndTags(sentence.Tokens);

                matchedTokensList.AddRange(value.tokens);
            }

            matchedTokensList.Count.Should().Be(matchCount);

            if (matchedTokensList.Count == 0)
            {
                return;
            }

            // "|"ですべてのSurfaceを連結して比較する。
            string.Join("|", matchedTokensList.Select(lis => string.Join("|", lis.Select(t => t.Surface))))
                .Should().Contain(expected);
        }
    }
}
