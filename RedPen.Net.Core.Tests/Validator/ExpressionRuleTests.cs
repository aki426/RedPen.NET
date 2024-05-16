﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tests.Parser;
using RedPen.Net.Core.Utility;
using RedPen.Net.Core.Validators;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validator
{
    public class ExpressionRuleTests
    {
        private ITestOutputHelper output;

        public ExpressionRuleTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void RunTest()
        {
            var rule = ExpressionRuleExtractor.Run("This:n + is:v");
            rule.Tokens.Count.Should().Be(2);

            rule.TokenPattern[0].direct.Should().BeTrue();
            rule.TokenPattern[0].token.Surface.Should().Be("this");
            rule.TokenPattern[0].token.Tags.Should().Contain("n");
            rule.TokenPattern[0].token.Reading.Should().Be("");

            rule.TokenPattern[1].direct.Should().BeTrue();
            rule.TokenPattern[1].token.Surface.Should().Be("is");
            rule.TokenPattern[1].token.Tags.Should().Contain("v");
            rule.TokenPattern[1].token.Reading.Should().Be("");

            rule = ExpressionRuleExtractor.Run("僕:名詞:ボク = は:助詞:ハ");
            rule.Tokens.Count.Should().Be(2);

            rule.TokenPattern[0].direct.Should().BeTrue();
            rule.TokenPattern[0].token.Surface.Should().Be("僕");
            rule.TokenPattern[0].token.Tags.Should().Contain("名詞");
            rule.TokenPattern[0].token.Reading.Should().Be("ボク");

            rule.TokenPattern[1].direct.Should().BeFalse();
            rule.TokenPattern[1].token.Surface.Should().Be("は");
            rule.TokenPattern[1].token.Tags.Should().Contain("助詞");
            rule.TokenPattern[1].token.Reading.Should().Be("ハ");

            // empty strings
            Action action = () => ExpressionRuleExtractor.Run(null);
            action.Should().Throw<ArgumentException>();

            action = () => ExpressionRuleExtractor.Run("");
            action.Should().Throw<ArgumentException>();

            action = () => ExpressionRuleExtractor.Run("  ");
            action.Should().Throw<ArgumentException>();

            action = () => ExpressionRuleExtractor.Run(" + ");
            action.Should().Throw<ArgumentException>();

            // aster '+' is missing
            action = () => ExpressionRuleExtractor.Run("This:n + is:v + ");
            action.Should().Throw<ArgumentException>();

            // aster '=' is missing
            action = () => ExpressionRuleExtractor.Run("This:n + is:v = ");
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ToStringTest()
        {
            ExpressionRuleExtractor.Run("  This : n      + is : v ").ToString().Should().Be("this:n: + is:v:");
            ExpressionRuleExtractor.Run("  僕:   名詞, : ボク =            は :助詞:     ハ").ToString().Should().Be("僕:名詞,:ボク = は:助詞:ハ");
        }

        /// <summary>ルール表現文字列から抽出したルールのマッチングテスト。</summary>
        [Theory]
        [InlineData("001", "::ナイ + ::ト = ::ナイ", "そんなことが無いとは言えない。", 1, "無い|と|は|言え|ない")]
        [InlineData("002", "::ナイ + ::ト = ::ナイ", "そんなことが無いとは言えません。", 0, "")]
        [InlineData("003", "::ナイ + ::ト = ::ナイ", "そんなことが無いとは限らない。", 1, "無い|と|は|限ら|ない")]
        [InlineData("004", "::ナイ + ::ト = ::ナイ", "そんなことが無いと断定することは出来ないだろう。", 1, "無い|と|断定|する|こと|は|出来|ない")]
        // NOTE: 品詞を指定しないと想定外のものにマッチしてしまう。
        [InlineData("005", "::ナイ + ::ト = ::ナイ", "水が無いとは想定内だ。", 1, "無い|と|は|想定|内")]
        // NOTE: 品詞を指定することで助動詞の「ない」にだけマッチする。
        [InlineData("006", "::ナイ + ::ト = :助動詞,特殊・ナイ:ナイ", "水が無いとは想定内だ。", 0, "")]
        // ひらがな表現でも適切な品詞であればマッチする。
        [InlineData("007", "::ナイ + ::ト = ::ナイ", "そんなことがないとは言えない。", 1, "ない|と|は|言え|ない")]
        [InlineData("008", ":形容詞,自立:ナイ + ::ト = ::ナイ", "そんなことがないとは言えない。", 1, "ない|と|は|言え|ない")]
        // 形容詞か助動詞かはケースバイケースなのでそれぞれ指定するしかない。
        [InlineData("009", "::ナイ + ::ト = ::ナイ", "食べないとは口が裂けても言えない状況。", 1, "ない|と|は|口|が|裂け|て|も|言え|ない")]
        [InlineData("010", ":助動詞,特殊・ナイ:ナイ + ::ト = :助動詞,特殊・ナイ:ナイ", "食べないとは口が裂けても言えない状況。", 1,
            "ない|と|は|口|が|裂け|て|も|言え|ない")]
        [InlineData("011", "::ナイ + ::ト = ::ナイ", "水が無いとは想定外だし、食料も無い。", 1,
            "無い|と|は|想定|外|だ|し|、|食料|も|無い")]
        [InlineData("012", ":形容詞,自立:ナイ + ::ト = :助動詞,特殊・ナイ:ナイ", "水が無いとは想定外だし、食料も無い。", 0, "")]
        [InlineData("013", "::ナイ + ::ト = ::ナイ", "これは想定内とは言えない。", 1, "内|と|は|言え|ない")]
        [InlineData("014", ":形容詞,自立:ナイ + ::ト = :助動詞,特殊・ナイ:ナイ", "これは想定内とは言えない。", 0, "")]
        public void JapaneseMatchesExtendTest(string nouse1, string rule, string text, int matchCount, string expected)
        {
            ExpressionRule expressionRule = ExpressionRuleExtractor.Run(rule);

            output.WriteLine("★Rule...");
            foreach (TokenElement token in expressionRule.Tokens)
            {
                output.WriteLine(token.ToString());
            }
            output.WriteLine("");

            List<ImmutableList<TokenElement>> matchedTokensList = new List<ImmutableList<TokenElement>>();

            // Sentenceごとにマッチングを取る。
            Document doc = new PlainTextParserTests(output).GenerateDocument(text, "ja-JP");
            foreach (var sentence in doc.GetAllSentences())
            {
                output.WriteLine("★Sentence...");
                foreach (TokenElement token in sentence.Tokens)
                {
                    output.WriteLine(token.ToString());
                }
                output.WriteLine("");

                matchedTokensList.AddRange(expressionRule.MatchExtend(sentence.Tokens));
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

        /// <summary>ルール表現文字列から抽出したルールのマッチングテスト。</summary>
        [Theory]
        [InlineData("001", "This:n + is:v", "He said , This is a pen .", 1, "This|is")]
        [InlineData("002", "a + pen:n", "This is a pen and a pen.", 2, "a|pen|a|pen")]
        [InlineData("003", "a + pen:n", "This is a pen. He has a pen.", 2, "a|pen|a|pen")]
        public void MatchSurfaceTest(string nouse1, string rule, string text, int matchCount, string expected)
        {
            ExpressionRule expressionRule = ExpressionRuleExtractor.Run(rule);

            output.WriteLine("★Rule...");
            foreach (TokenElement token in expressionRule.Tokens)
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
                    expressionRule.MatchesConsecutiveSurfaces(sentence.Tokens);

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
            ExpressionRule expressionRule = ExpressionRuleExtractor.Run(rule);

            output.WriteLine("★Rule...");
            foreach (TokenElement token in expressionRule.Tokens)
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
                    expressionRule.MatchesConsecutiveSurfaces(sentence.Tokens);

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
            ExpressionRule expressionRule = ExpressionRuleExtractor.Run(rule);

            output.WriteLine("★Rule...");
            foreach (TokenElement token in expressionRule.Tokens)
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
                    expressionRule.MatchesConsecutiveSurfacesAndTags(sentence.Tokens);

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
