using System;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;

namespace RedPen.Net.Core.Tests.Config
{
    /// <summary>
    /// Enum ValidationTypeのテスト。
    /// </summary>
    public class ValidationTypeTests
    {
        /// <summary>
        /// ToStringでEnumの列挙要素名そのものを取得できることのテスト。
        /// </summary>
        [Fact]
        public void ToStringTest()
        {
            ValidationType.SentenceLength.ToString().Should().Be("SentenceLength");
        }

        /// <summary>
        /// ValidationTypeExtendの動作確認テスト。
        /// </summary>
        [Fact]
        public void ExtendMethodTest()
        {
            ValidationType type = ValidationType.SentenceLength;
            type.ValidationName().Should().Be("SentenceLength");
            type.ValidatorName().Should().Be("SentenceLengthValidator");
            type.ConfigurationName().Should().Be("SentenceLengthConfiguration");

            ValidationTypeExtend.ConvertFrom("SentenceLength").Should().Be(ValidationType.SentenceLength);

            //ValidationTypeExtend.ConvertFrom("HogeHoge").Should().Be(ValidationType.SentenceLength);
            Action act = () => ValidationTypeExtend.ConvertFrom("HogeHoge");
            act.Should().Throw<ArgumentException>()
                .WithMessage("No such a validation name as HogeHoge");
        }

        /// <summary>
        /// ValidationTypeに対応するTypeを取得するテスト。
        /// </summary>
        [Fact]
        public void GetTypeTest()
        {
            ValidationType type = ValidationType.SentenceLength;
            type.TypeOfConfigurationClass().Should().Be(typeof(SentenceLengthConfiguration));
            type.TypeOfConfigurationClass().FullName.Should().Be("RedPen.Net.Core.Validators.SentenceValidator.SentenceLengthConfiguration");

            type.TypeOfValidatorClass().Should().Be(typeof(SentenceLengthValidator));
            type.TypeOfValidatorClass().FullName.Should().Be("RedPen.Net.Core.Validators.SentenceValidator.SentenceLengthValidator");

            // MEMO: このテストケース実装時にUnexpandedAcronymのValidatorもConfigurationも未実装のため、
            // Typeを取得しようとするとこれに対応するValidatorConfigurationを実装した具象クラスは無いため、Nullが返る。
            // TODO: UnexpandedAcronymを実装後は次のテストはNullが返らず失敗するようになるので、
            // その際は書き換えるかこのテスト自体不要とするか要検討。
            ValidationType nothingType = ValidationType.UnexpandedAcronym;

            Action act = () => nothingType.TypeOfConfigurationClass();
            act.Should().Throw<InvalidOperationException>();

            act = () => nothingType.TypeOfValidatorClass();
            act.Should().Throw<InvalidOperationException>();
        }
    }
}
