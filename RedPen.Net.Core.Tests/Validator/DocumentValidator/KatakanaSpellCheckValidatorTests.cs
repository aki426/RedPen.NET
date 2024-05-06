using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Utility;
using RedPen.Net.Core.Validators.DocumentValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validator.DocumentValidator
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
        [InlineData("002", "フレーズ・アナライズにバグがある。バグのあるフェーズ・アナライシス。", 0.3, 5, "", 6,
            "フレーズ・アナライズ,フレーズ,アナライズ,フェーズ・アナライシス,フェーズ,アナライシス")]
        [InlineData("003", "あのミニマムサポートとこのミニマムサポータは違う。", 0.3, 5, "", 2, "ミニマムサポート,ミニマムサポータ")]
        // カタカナ語はどうやらすべて名詞として判定される。
        [InlineData("004", "ヒットです！　サイクルヒットです！　ヒットするです。ヒットる。ツラい。ヒドい。ヒットう。", 0.3, 5, "", 0, "")]
        [InlineData("005", "あのミニマムサポートとこのミニマムサポータはMinRatioの値が低いためエラーにならない。", 0.001, 5, "", 0, "")]
        [InlineData("006", "あのミニマムサポートとこのミニマムサポータはMinFreqの値が低いためエラーにならない。", 0.3, 0, "", 0, "")]
        [InlineData("007", "あのミニマムサポートとこのミニマムサポータはWordSetに該当するためエラーにならない。", 0.3, 5, "ミニマムサポート,ミニマムサポータ", 0, "")]
        [InlineData("008", "あのミニマムサポートとこのミニマムサポータはWordSetに片方が該当するためエラーは1つ。", 0.3, 5, "ミニマムサポート", 1, "ミニマムサポータ")]
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
