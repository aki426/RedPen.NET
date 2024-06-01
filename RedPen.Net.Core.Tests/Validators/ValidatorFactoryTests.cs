using Xunit;
using Xunit.Abstractions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Validators;
using FluentAssertions;
using RedPen.Net.Core.Validators.SentenceValidator;
using System.Globalization;
using System.Collections.Generic;
using System.Reflection;
using System;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Tests.Validators
{
    public class ValidatorFactoryTests
    {
        private readonly ITestOutputHelper output;

        public ValidatorFactoryTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        // MEMO: ValidatorFactoryの実装として、ValidationTypeをSwitchで分岐させてインスタンスを生成する素朴なFactoryPatternも考えられる。
        // 一方、Validatorの定義と存在はValidatorTypeとValidationTypeExtendが知っているため、Activatorを用いて多相な生成も可能である。
        // 次の2つのテストケースは全く同じ効果を生む。

        ///// <summary>
        ///// Activatorを用いてValidationTypeから対応するValidatorインスタンスを取得するテスト。
        ///// </summary>
        //[Fact]
        //public void GetValidatorInstanceFromActivatorTest()
        //{
        //    var cultureInfo = CultureInfo.GetCultureInfo("ja-JP");
        //    ValidationType.SentenceLength.ToString().Should().Be("SentenceLength");

        //    var config = new SentenceLengthConfiguration(ValidationLevel.ERROR, 120);
        //    config.ValidationName.Should().Be("SentenceLength");

        //    Validator validator = GetValidatorInstanceFromActivator(
        //        cultureInfo,
        //        new SymbolTable(cultureInfo, "", new List<Symbol>()),
        //        config);

        //    validator.ValidationName.Should().Be("SentenceLength");
        //    (validator is ISentenceValidatable).Should().BeTrue();
        //}

        ///// <summary>
        ///// FactoryパターンでValidatorインスタンスを取得するテスト。
        ///// </summary>
        //[Fact]
        //public void GetValidatorInstanceFromFactoryPatternTest()
        //{
        //    var cultureInfo = CultureInfo.GetCultureInfo("ja-JP");
        //    ValidationType.SentenceLength.ToString().Should().Be("SentenceLength");

        //    var config = new SentenceLengthConfiguration(ValidationLevel.ERROR, 120);
        //    config.ValidationName.Should().Be("SentenceLength");

        //    Validator validator = GetValidatorInstanceFromFactory(
        //        cultureInfo,
        //        new SymbolTable(cultureInfo, "", new List<Symbol>()),
        //        config);

        //    validator.ValidationName.Should().Be("SentenceLength");
        //    (validator is ISentenceValidatable).Should().BeTrue();
        //}

        //private Validator GetValidatorInstanceFromActivator(
        //    CultureInfo cultureInfo,
        //    SymbolTable symbolTable,
        //    ValidatorConfiguration validatorConfiguration)
        //{
        //    var args = new object[] { cultureInfo, symbolTable, validatorConfiguration };

        //    object v = Activator.CreateInstance(
        //        validatorConfiguration.Type.GetTypeAsValidatorClass(),
        //        BindingFlags.CreateInstance,
        //        null,
        //        args,
        //        null);

        //    return v as Validator;
        //}

        //private Validator GetValidatorInstanceFromFactory(
        //    CultureInfo cultureInfo,
        //    SymbolTable symbolTable,
        //    ValidatorConfiguration validatorConfiguration)
        //{
        //    switch (validatorConfiguration.Type)
        //    {
        //        // case ValidationType.CommaNumber: など本来はすべてのケースを実装する必要がある。

        //        case ValidationType.SentenceLength:
        //            return new SentenceLengthValidator(
        //                cultureInfo,
        //                symbolTable,
        //                validatorConfiguration as SentenceLengthConfiguration);

        //        default:
        //            break;
        //    }

        //    return null;
        //}

        /// <summary>
        /// ValidatorFactoryを用いてValidatorConfigurationから対応するValidatorインスタンスを取得するテスト。
        /// </summary>
        [Fact]
        public void ValidatorFactoryGetValidatorTest()
        {
            ValidatorFactory factory = ValidatorFactory.GetInstance();

            var cultureInfo = CultureInfo.GetCultureInfo("ja-JP");
            //ValidationType.SentenceLength.ToString().Should().Be("SentenceLength");

            var config = new SentenceLengthConfiguration(ValidationLevel.ERROR, 120);
            config.ValidationName.Should().Be("SentenceLength");

            // MEMO: ValidatorFactory.GetValidatorはActivatorを用いてValidatorインスタンスを生成する。
            Validator validator = factory.GetValidator(
                cultureInfo,
                new SymbolTable(cultureInfo, "", new List<Symbol>()),
                config);

            validator.ValidationName.Should().Be("SentenceLength");
            (validator is SentenceLengthValidator).Should().BeTrue();
            (validator is ISentenceValidatable).Should().BeTrue();
        }

        /// <summary>ValidatorConfigurationからValidatorのフルネームを取得するテスト。</summary>
        [Fact()]
        public void GetValidatorFullNameTest()
        {
            var sentenceLengthConfig = new SentenceLengthConfiguration(ValidationLevel.ERROR, 120);

            ValidatorFactory.GetValidatorFullName(sentenceLengthConfig)
                .Should().Be("RedPen.Net.Core.Validators.SentenceValidator.SentenceLengthValidator");

            var testValidatorFactoryConfig = new TestValidatorFactoryConfiguration(ValidationLevel.ERROR);

            ValidatorFactory.GetValidatorFullName(testValidatorFactoryConfig)
                .Should().Be("RedPen.Net.Core.Tests.Validators.TestValidatorFactoryValidator");
        }
    }

    // NOTE: 以下、テスト用のどんがらValidatorクラスとValidatorConfigurationクラス。
    // NOTE: このためだけに.Net Framework 4.8のテストプロジェクトの言語バージョンを10.0に上げた。

    /// <summary>TestValidatorFactoryのConfiguration</summary>
    public record TestValidatorFactoryConfiguration : ValidatorConfiguration
    {
        public TestValidatorFactoryConfiguration(ValidationLevel level) : base(level)
        {
        }
    }

    /// <summary>TestValidatorFactoryのValidator</summary>
    public class TestValidatorFactoryValidator : Validator, ISentenceValidatable
    {
        /// <summary>ValidatorConfiguration</summary>
        public TestValidatorFactoryConfiguration Config { get; init; }

        // TODO: コンストラクタの引数定義は共通にすること。
        /// <summary>
        /// Initializes a new instance of the <see cref="TestValidatorFactoryValidator"/> class.
        /// </summary>
        /// <param name="documentLangForTest">The document lang for test.</param>
        /// <param name="symbolTable">The symbol table.</param>
        /// <param name="config">The config.</param>
        public TestValidatorFactoryValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            TestValidatorFactoryConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;
        }

        /// <summary>
        /// Validate.
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <returns>A list of ValidationErrors.</returns>
        public List<ValidationError> Validate(Sentence sentence)
        {
            return new List<ValidationError>();
        }
    }
}
