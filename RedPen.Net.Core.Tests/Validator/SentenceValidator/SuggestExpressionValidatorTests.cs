using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Errors;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validator.SentenceValidator
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

            validator.PreValidate(sentence);
            var errors = validator.Validate(sentence);

            errors.Count.Should().Be(1);

            // 7. エラーメッセージを生成する。
            var manager = ErrorMessageManager.GetInstance();

            output.WriteLine(manager.GetErrorMessage(errors[0], CultureInfo.GetCultureInfo("ja-JP")));

            manager.GetErrorMessage(errors[0], CultureInfo.GetCultureInfo("ja-JP")).Should()
                .Be("不正な単語 \"like\" が見つかりました。代替表現 \"such as\" を使用してください。");
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
            var manager = ErrorMessageManager.GetInstance();
            manager.GetErrorMessage(errors[0], CultureInfo.GetCultureInfo("en-US")).Should()
                .Be("Found invalid word \"like\". Use the synonym \"such as\" instead.");
            manager.GetErrorMessage(errors[1], CultureInfo.GetCultureInfo("en-US")).Should()
                .Be("Found invalid word \"like\". Use the synonym \"such as\" instead.");

            errors = validator.Validate(new Sentence("it like a info.", 1));
            errors.Count.Should().Be(2);

            manager.GetErrorMessage(errors[0], CultureInfo.GetCultureInfo("en-US")).Should()
                .Be("Found invalid word \"like\". Use the synonym \"such as\" instead.");
            manager.GetErrorMessage(errors[1], CultureInfo.GetCultureInfo("en-US")).Should()
                .Be("Found invalid word \"info\". Use the synonym \"information\" instead.");
        }

        /// <summary>日本語文のテスト。</summary>
        [Fact]
        public void JapaneseSentenceTest()
        {
            var suggestExpressionConfiguration = new SuggestExpressionConfiguration(
                ValidationLevel.ERROR,
                new Dictionary<string, string>
                {
                    { "おはよう", "おはようございます" }
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
            var manager = ErrorMessageManager.GetInstance();
            manager.GetErrorMessage(errors[0], CultureInfo.GetCultureInfo("ja-JP")).Should()
                .Be("不正な単語 \"おはよう\" が見つかりました。代替表現 \"おはようございます\" を使用してください。");

            // TODO: 誤表現が正表現の一部である場合、正表現が使用されているにもかかわらず誤表現が検出されてしまう。要検討。
            errors = validator.Validate(new Sentence("おはようございます日本。", 1));
            errors.Count.Should().Be(1);

            manager.GetErrorMessage(errors[0], CultureInfo.GetCultureInfo("ja-JP")).Should()
                .Be("不正な単語 \"おはよう\" が見つかりました。代替表現 \"おはようございます\" を使用してください。");
        }
    }
}
