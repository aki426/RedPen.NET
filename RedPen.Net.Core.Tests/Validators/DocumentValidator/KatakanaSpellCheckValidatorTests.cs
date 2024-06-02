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
using System.Globalization;
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Utility;
using RedPen.Net.Core.Validators.DocumentValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validators.DocumentValidator
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
        [InlineData("001", "ハロー、ハロ。\nあのインデクスとこのインデックス", 0.3, 5, "", true, 0, "")]
        // デフォルト辞書にないのでエラーとして検出される。
        [InlineData("002", "フレーズ・アナライズにバグがある。バグのあるフェーズ・アナライシス。", 0.3, 5, "", true, 6,
            "フレーズ・アナライズ,フレーズ,アナライズ,フェーズ・アナライシス,フェーズ,アナライシス")]
        [InlineData("003", "あのミニマムサポートとこのミニマムサポータは違う。", 0.3, 5, "", true, 2, "ミニマムサポート,ミニマムサポータ")]
        // カタカナ語はどうやらすべて名詞として判定される。
        [InlineData("004", "ヒットです！　サイクルヒットです！　ヒットするです。ヒットる。ツラい。ヒドい。ヒットう。", 0.3, 5, "", true, 0, "")]
        [InlineData("005", "あのミニマムサポートとこのミニマムサポータはMinRatioの値が低いためエラーにならない。", 0.001, 5, "", true, 0, "")]
        [InlineData("006", "あのミニマムサポートとこのミニマムサポータはMinFreqの値が低いためエラーにならない。", 0.3, 0, "", true, 0, "")]
        [InlineData("007", "あのミニマムサポートとこのミニマムサポータはWordSetに該当するためエラーにならない。", 0.3, 5, "ミニマムサポート,ミニマムサポータ", true, 0, "")]
        [InlineData("008", "あのミニマムサポートとこのミニマムサポータはWordSetに片方が該当するためエラーは1つ。", 0.3, 5, "ミニマムサポート", true, 1, "ミニマムサポータ")]
        // デフォルト辞書を使用しないオプションにすればすべてのカタカナ語はValidation対象となる。
        [InlineData("009", "ハロー、ハロ。\nあのインデクスとこのインデックス", 0.3, 5, "", false, 2, "インデクス,インデックス")]
        // 「仮面ライダー」は固有名詞として1Tokenで分割されるが、さらにカタカナ語として分割する処理を入れているのでライダーとライターが表記ゆれ判定される。
        [InlineData("010", "仮面ライダーはKuromojiで固有名詞としてToken化されるがカタカナ語分割するので仮面ライターと表記ゆれ判定される。",
            0.3, 5, "", false, 2, "ライダー,ライター")]
        // 「インチ」「ディスプレイ」の連結である「インチディスプレイ」は自身を構成する「ディスプレイ」と表記ゆれ判定されてしまう。
        // これを避けるためにLevenStein距離で判定された後に、OffsetMapをOverlap関数で判定し出現位置が同じだった場合はエラーとして扱わないロジックを入れている。
        [InlineData("011", "32インチディスプレイは分割されたTokenと連結されたTokenという自分自身同士で表記ゆれ判定しないように、OffsetMapで位置を共有するTokenは判定から除外している。\nディスプレーを設置する。",
            0.3, 5, "", false, 2, "ディスプレイ,ディスプレー")]
        public void BasicTest(string nouse1, string text, double minRatio, int minFreq, string skipWords, bool enableDefaultDict, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // ValidatorConfiguration
            var validatorConfiguration = new KatakanaSpellCheckConfiguration(
                ValidationLevel.ERROR,
                minRatio,
                minFreq,
                skipWords.Split(',').ToHashSet(),
                enableDefaultDict
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
            LevenshteinDistanceUtility utility = new LevenshteinDistanceUtility();
            string katakana = "インデクス";
            string other = "インデックス";

            output.WriteLine($"Distance threshold: {(int)Math.Round(katakana.Length * 0.3)} from {katakana}");
            output.WriteLine($"Distance threshold: {(int)Math.Round(other.Length * 0.3)} from {other}");
            output.WriteLine("LevenshteinDistance: " + utility.GetDistance(other, katakana));

            utility.GetDistance(other, katakana).Should().Be(utility.GetDistanceMemoize(other, katakana));
        }
    }
}
