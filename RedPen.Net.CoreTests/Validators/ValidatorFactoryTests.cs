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
using System.Collections.Immutable;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Globals;
using Xunit;

namespace RedPen.Net.Core.Validators.Tests
{
    public class ValidatorFactoryTests
    {
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
}
