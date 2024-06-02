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
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Errors;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Validators;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validators.SentenceValidator
{
    /// <summary>
    /// The sentence length validator tests.
    /// </summary>
    public class SentenceLengthValidatorTests
    {
        // テスト間で共通の設定を使う場合は、クラス変数を用いる。

        private readonly ITestOutputHelper output;

        // あるValidatorを実行するために必要な最低限の設定は次の1~の手順。

        // 1. 基本パラメータ
        private CultureInfo cultureInfo = new CultureInfo("en-US");

        private string variant = "";
        private ValidationLevel level = ValidationLevel.ERROR;
        private SentenceLengthValidator validator;
        private string messageKey = "";

        /// <summary>
        /// Test SetUp.
        /// </summary>
        public SentenceLengthValidatorTests(ITestOutputHelper output)
        {
            this.output = output;

            // 2. ValidatorConfiguration
            SentenceLengthConfiguration validatorConfiguration = new SentenceLengthConfiguration(level, 30);

            // 3. SymbolTable
            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(cultureInfo, variant, new List<Symbol>());

            // 4. validator
            validator = new SentenceLengthValidator(
                cultureInfo,
                //ValidationMessage.ResourceManager,
                symbolTable,
                validatorConfiguration);
        }

        /// <summary>
        /// tests the with long sentence.
        /// </summary>
        [Fact]
        public void testWithLongSentence()
        {
            validator.Config.MinLength.Should().Be(30);
            validator.ValidationName.Should().Be("SentenceLength");

            // 5. テスト対象のオブジェクトを生成する。
            Sentence str = new Sentence(
                "this is a very long long long long long long long long long long long long sentence.",
                0);

            // 6. Validateを実行する。
            List<ValidationError> errors = validator.Validate(str);

            errors.Count.Should().Be(1);
            errors[0].ValidationName.Should().Be("SentenceLength");

            // 7. エラーメッセージを生成する。
            var manager = new ErrorMessageManager();

            manager.GetErrorMessage(
                errors[0],
                CultureInfo.GetCultureInfo("en-US"))
                    .Should().Be("The sentence length (84 characters) exceeded the specified value (30 characters).");

            manager.GetErrorMessage(
                errors[0],
                CultureInfo.GetCultureInfo("ja-JP"))
                    .Should().Be("文の長さ（84文字）が規定値（30文字）以上でした。");
        }

        /// <summary>
        /// エラーにならない場合のテスト
        /// </summary>
        [Fact]
        public void WithShortSentenceTest()
        {
            Sentence str = new Sentence("this is a sentence.", 0);

            List<ValidationError> errors = validator.Validate(str);

            errors.Count.Should().Be(0);
        }

        /// <summary>
        /// 空文字列に対してエラーを検出しないことのテスト
        /// </summary>
        [Fact]
        public void WithZeroLengthSentenceTest()
        {
            Sentence str = new Sentence("", 0);

            List<ValidationError> errors = validator.Validate(str);

            errors.Count.Should().Be(0);
        }
    }
}
