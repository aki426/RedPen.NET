using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Utility;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validator.SentenceValidator
{
    public class KatakanaSpellCheckValidatorTests
    {
        private ITestOutputHelper output;

        public KatakanaSpellCheckValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        // インデックスもインデクスもデフォルト辞書に入っているので許容されてしまう。
        [InlineData("001", "ハロー、ハロ。\nあのインデクスとこのインデックス", 0.3, 5, "", 0, "インデクス,インデックス")]
        // デフォルト辞書にないのでエラーとして検出される。
        [InlineData("002", "フレーズ・アナライズにバグがある。バグのあるフェーズ・アナライシス。", 0.3, 5, "", 4, "フレーズ,アナライズ,フェーズ,アナライシス")]
        public void BasicTest(string nouse1, string text, double minRatio, int minFreq, string skipWords, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // ValidatorConfiguration
            var validatorConfiguration = new KatakanaSpellCheckConfiguration(
                ValidationLevel.ERROR,
                minRatio,
                minFreq,
                skipWords.Split(',').ToHashSet()
            );

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new KatakanaSpellCheckValidator(
                documentLang,
                symbolTable,
                validatorConfiguration);

            ValidatorTestsUtility.CommonDocumentWithPlainTextParseErrorPatternTest(
                validator,
                text,
                documentLang,
                errorCount,
                expected,
                output);
        }

        [Fact]
        public void LevenshteinDistanceTest()
        {
            string katakana = "インデクス";
            string other = "インデックス";

            output.WriteLine($"Distance threshold: {(int)Math.Round(katakana.Length * 0.3)} from {katakana}");
            output.WriteLine($"Distance threshold: {(int)Math.Round(other.Length * 0.3)} from {other}");
            output.WriteLine("LevenshteinDistance: " + LevenshteinDistanceUtility.GetDistance(other, katakana));
        }
    }
}
