using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Errors;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Validators;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validator.SentenceValidator
{
    public class SuggestExpressionValidatorTests
    {
        private ITestOutputHelper output;

        public SuggestExpressionValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

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

        [Fact]
        public void TestSynonym()
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

        //[Fact]
        //public void TestSynonymSplitPlusWhiteSpace()
        //{
        //    var config = new Configuration
        //    {
        //        ValidatorConfigs = new List<ValidatorConfiguration>
        //        {
        //            new ValidatorConfiguration("SuggestExpression")
        //            {
        //                Properties = new Dictionary<string, string>
        //                {
        //                    { "map", "{like, such as}" }
        //                }
        //            }
        //        }
        //    };

        //    var validator = ValidatorFactory.GetInstance(config.ValidatorConfigs[0], config);
        //    var errors = new List<ValidationError>();
        //    validator.ErrorList = errors;
        //    validator.Validate(new Sentence("they like a piece of a cake.", 0));
        //    errors.Should().HaveCount(1);
        //}

        //[Fact]
        //public void TestWithoutSynonym()
        //{
        //    var config = new Configuration
        //    {
        //        ValidatorConfigs = new List<ValidatorConfiguration>
        //        {
        //            new ValidatorConfiguration("SuggestExpression")
        //            {
        //                Properties = new Dictionary<string, string>
        //                {
        //                    { "map", "{like,such as}, {info,information}" }
        //                }
        //            }
        //        }
        //    };

        //    var validator = ValidatorFactory.GetInstance(config.ValidatorConfigs[0], config);
        //    var errors = new List<ValidationError>();
        //    validator.ErrorList = errors;
        //    validator.Validate(new Sentence("it loves a piece of a cake.", 0));
        //    errors.Should().BeEmpty();
        //}

        // 他のテストメソッドも同様に書き換えられます。
    }
}
