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
using System;
using System.Collections.Immutable;
using System.Globalization;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Globals;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Validators.Tests
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
            ValidatorFactory factory = new ValidatorFactory();

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
                .Should().Be("RedPen.Net.Core.Validators.Tests.TestValidatorFactoryValidator");
        }

        /// <summary>
        /// 応用アプリケーション側で新規追加するValidationのValidatorロードテスト。
        /// </summary>
        [Fact()]
        public void GetAddonValidatorTest()
        {
            string jsonString = @"{
    // コメントも書けます。
    ""DocumentLang"": ""ja-JP"",
    ""Variant"": ""zenkaku"",
    ""MessageLang"": ""ja-JP"",
    ""ValidatorConfigurations"": [
        {
            ""Name"": ""Test"",
            ""Level"" : ""ERROR""
        }
    ]
}";

            // Testバリデーションの情報をConfigurationとして読み込む。
            var jsonLoader = new ConfigurationLoader(
                DefaultValidationDefinition.ValidationNameToValidatorConfigurationTypeMap.Add("Test", typeof(TestConfiguration)));
            Configuration configuration = jsonLoader.Load(jsonString);

            // Testバリデーションの情報が正しく読み込まれていることを確認。
            configuration.ValidatorConfigurations[0].ValidationName.Should().Be("Test");
            ValidatorFactory.GetValidatorFullName(configuration.ValidatorConfigurations[0])
                .Should().Be("RedPen.Net.Core.Validators.Tests.TestValidator");

            // Coreプロジェクトからは応用アプリケーション側への参照が無いので、ValidatorFactoryで生成することはできない。
            // よって、事前に型情報を与える必要がある。
            // NOTE: Typeはクラス定義すべてを包含するらしい。よって参照元プロジェクトから参照先プロジェクトのActivatorでクラス生成も可能。
            ValidatorFactory factory = new ValidatorFactory(
                ImmutableDictionary<string, Type>.Empty.Add("Test", typeof(TestValidator)));

            Validator validator = factory.GetValidator(
                configuration.DocumentCultureInfo,
                configuration.SymbolTable,
                configuration.ValidatorConfigurations[0]);

            validator.ValidationName.Should().Be("Test");
            (validator is TestValidator).Should().BeTrue();
            (validator is ISentenceValidatable).Should().BeTrue();
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
