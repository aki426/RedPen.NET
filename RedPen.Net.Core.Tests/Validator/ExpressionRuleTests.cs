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
                    expressionRule.MatchSurfaces(sentence.Tokens);

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
                    expressionRule.MatchSurfaces(sentence.Tokens);

                matchedTokensList.AddRange(value.tokens);
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
                    expressionRule.MatchSurfacesAndTags(sentence.Tokens);

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
