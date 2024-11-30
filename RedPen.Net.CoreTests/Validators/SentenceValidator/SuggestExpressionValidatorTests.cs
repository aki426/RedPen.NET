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
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Errors;
using RedPen.Net.Core.Model;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Validators.SentenceValidator.Tests
{
    /// <summary>SuggestExpressionValidatorのテスト。</summary>
    public class SuggestExpressionValidatorTests
    {
        private ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="SuggestExpressionValidatorTests"/> class.
        /// </summary>
        /// <param name="output">The output.</param>
        public SuggestExpressionValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>char.IsLetterのテスト。</summary>
        [Fact]
        public void FrameworkBasicTest()
        {
            // 文字が文字(アルファベット)かどうかを判定する。
            char.IsLetter('a').Should().BeTrue();
            char.IsLetter('1').Should().BeFalse();
            char.IsLetter('\0').Should().BeFalse();

            // 文字列の各文字を判定する。
            "Hello今日は雨".ToList().ForEach(c =>
            {
                char.IsLetter(c).Should().BeTrue();
            });
            // 記号系は期待通り文字として判定されない。
            " 123。「　』".ToList().ForEach(c =>
            {
                char.IsLetter(c).Should().BeFalse();
            });
        }

        /// <summary>最も単純な基本形のテスト。</summary>
        [Fact]
        public void BasicTest()
        {
            var suggestExpressionConfiguration = new SuggestExpressionConfiguration(
                ValidationLevel.ERROR,
                new Dictionary<string, string>
                {
                    { "like", "such as" },
                    { "info", "information" }
                }
            );

            var validator = new SuggestExpressionValidator(
                new CultureInfo("en-US"),
                new SymbolTable(new CultureInfo("en-US"), "", new List<Symbol>()),
                suggestExpressionConfiguration
                );

            var sentence = new Sentence("they like a piece of a cake.", 1);

            var errors = validator.Validate(sentence);

            errors.Count.Should().Be(1);

            // 7. エラーメッセージを生成する。
            var manager = new ErrorMessageManager();

            output.WriteLine(manager.GetErrorMessage(errors[0], CultureInfo.GetCultureInfo("ja-JP")));

            manager.GetErrorMessage(errors[0], CultureInfo.GetCultureInfo("ja-JP")).Should()
                .Be("不正な表現 \"like\" が見つかりました。代替表現 \"such as\" の使用を検討してください。");
        }

        /// <summary>英語文のテスト。</summary>
        [Fact]
        public void EnglishSentenceTest()
        {
            var suggestExpressionConfiguration = new SuggestExpressionConfiguration(
                ValidationLevel.ERROR,
                new Dictionary<string, string>
                {
                    { "like", "such as" },
                    { "info", "information" }
                }
            );

            var validator = new SuggestExpressionValidator(
                new CultureInfo("en-US"),
                new SymbolTable(new CultureInfo("en-US"), "", new List<Symbol>()),
                suggestExpressionConfiguration
                );

            var errors = validator.Validate(new Sentence("", 1));
            errors.Count.Should().Be(0);

            errors = validator.Validate(new Sentence("it loves a piece of a cake.", 1));
            errors.Count.Should().Be(0);

            errors = validator.Validate(new Sentence("it likes a piece of a cake.", 1));
            errors.Count.Should().Be(0);

            errors = validator.Validate(new Sentence("I like a pen like this.", 1));
            errors.Count.Should().Be(2);

            // 7. エラーメッセージを生成する。
            var manager = new ErrorMessageManager();
            manager.GetErrorMessage(errors[0], CultureInfo.GetCultureInfo("en-US")).Should()
                .Be("An invalid expression \"like\" was found. Consider using the alternative expression \"such as\".");
            manager.GetErrorMessage(errors[1], CultureInfo.GetCultureInfo("en-US")).Should()
                .Be("An invalid expression \"like\" was found. Consider using the alternative expression \"such as\".");

            errors = validator.Validate(new Sentence("it like a info.", 1));
            errors.Count.Should().Be(2);

            manager.GetErrorMessage(errors[0], CultureInfo.GetCultureInfo("en-US")).Should()
                .Be("An invalid expression \"like\" was found. Consider using the alternative expression \"such as\".");
            manager.GetErrorMessage(errors[1], CultureInfo.GetCultureInfo("en-US")).Should()
                .Be("An invalid expression \"info\" was found. Consider using the alternative expression \"information\".");
        }

        /// <summary>日本語文のテスト。</summary>
        [Fact]
        public void JapaneseSentenceTest()
        {
            var suggestExpressionConfiguration = new SuggestExpressionConfiguration(
                ValidationLevel.ERROR,
                new Dictionary<string, string>
                {
                    { "おはよう", "おはようございます" },
                    { "おはおは", "朝の挨拶" }
                }
            );

            var validator = new SuggestExpressionValidator(
                new CultureInfo("ja-JP"),
                new SymbolTable(new CultureInfo("ja-JP"), "", new List<Symbol>()),
                suggestExpressionConfiguration
                );

            var errors = validator.Validate(new Sentence("", 1));
            errors.Count.Should().Be(0);

            errors = validator.Validate(new Sentence("おはよう日本。", 1));
            errors.Count.Should().Be(1);

            // 7. エラーメッセージを生成する。
            var manager = new ErrorMessageManager();
            manager.GetErrorMessage(errors[0], CultureInfo.GetCultureInfo("ja-JP")).Should()
                .Be("不正な表現 \"おはよう\" が見つかりました。代替表現 \"おはようございます\" の使用を検討してください。");

            // MEMO: 誤表現が正表現の一部である場合、正表現が使用されている場合は誤表現を検出しない。
            // MEMO: ただし、誤表現のスキップは1組の正誤の中でだけの話で、他の正誤の組み合わせに対して検出キャンセルはしない。
            errors = validator.Validate(new Sentence("おはおはようございます日本。", 1));
            errors.Count.Should().Be(1);

            // 「おはよう」は「おはようございます」の一部であるため、エラーとして検出されない。
            // 一方、「おはおは」は「おはようございます」の一部ではあるが、それは「おはおは」に対する正表現ではないため、エラーとして検出される。
            manager.GetErrorMessage(errors[0], CultureInfo.GetCultureInfo("ja-JP")).Should()
                .Be("不正な表現 \"おはおは\" が見つかりました。代替表現 \"朝の挨拶\" の使用を検討してください。");

            suggestExpressionConfiguration = new SuggestExpressionConfiguration(
                ValidationLevel.ERROR,
                new Dictionary<string, string>
                {
                    { "おはおは", "おはようございます" }
                }
             );

            validator = new SuggestExpressionValidator(
                new CultureInfo("ja-JP"),
                new SymbolTable(new CultureInfo("ja-JP"), "", new List<Symbol>()),
                suggestExpressionConfiguration
                );

            errors = validator.Validate(new Sentence("おはおは日本。", 1));
            errors.Count.Should().Be(1);
            manager.GetErrorMessage(errors[0], CultureInfo.GetCultureInfo("ja-JP")).Should()
                .Be("不正な表現 \"おはおは\" が見つかりました。代替表現 \"おはようございます\" の使用を検討してください。");

            // MEMO: 一方、正表現が誤表現の一部をマスクするような関係の場合、期待する誤表現が検出されなくなってしまう。
            errors = validator.Validate(new Sentence("おはおはようございます日本。", 1));
            errors.Count.Should().Be(0);

            // 単純に誤表現を検出したいだけならInvalidExpressionがあるので、SuggestExpressionでは
            // 正表現が使われている場合の誤表現は検出しない方がより使用者の意図にかなう、という考えでこの仕様とする。
        }

        /// <summary>GetNullMaskedの動作確認テスト。</summary>
        [Fact]
        public void GetNullMaskedTest()
        {
            // MEMO: 誤表現と正表現が一部重複するようなケースでは、正表現を除去した文字列内のどこに誤表現が含まれるかを検出すればよい。
            // content内の正表現出現箇所をNUL文字で置換した文字列であれば、文字列の長さや位置関係が変わらないため、
            // 誤表現の検出方法および位置の特定もシンプルな実装にできる。

            string content = "おはようございます日本、おはようございます世界。";
            string invalid = "おはよう";
            string valid = "おはようございます";

            string replace = new string('\0', valid.Length);
            string newContent = Regex.Replace(content, Regex.Escape(valid), replace);

            newContent.Should().Be("\0\0\0\0\0\0\0\0\0日本、\0\0\0\0\0\0\0\0\0世界。");
            newContent.Contains(invalid).Should().BeFalse();

            SuggestExpressionValidator.GetNullMasked(content, valid)
                .Should().Be("\0\0\0\0\0\0\0\0\0日本、\0\0\0\0\0\0\0\0\0世界。");
            SuggestExpressionValidator.GetNullMasked(content, valid).Contains(invalid).Should().BeFalse();
        }
    }
}
