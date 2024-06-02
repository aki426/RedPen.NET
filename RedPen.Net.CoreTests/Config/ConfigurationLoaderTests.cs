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
using System.Globalization;
using FluentAssertions;
using RedPen.Net.Core.Globals;
using RedPen.Net.Core.Validators.DocumentValidator;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;

namespace RedPen.Net.Core.Config.Tests
{
    public class ConfigurationLoaderTests
    {
        [Fact()]
        public void LoadLangSettingTest()
        {
            // ライブラリ実装済みのValidatorConfiguration定義を与える。その定義にあるValidatorConfigurationのみロードされる。
            var jsonLoader = new ConfigurationLoader(DefaultValidationDefinition.ValidatorConfTypeDefinitions);

            // Void test
            Action action;
            action = () => jsonLoader.Load(@"");
            action.Should().Throw<System.Text.Json.JsonException>();

            // Null object
            action = () => jsonLoader.Load(@"{}");
            action.Should().NotThrow<System.Text.Json.JsonException>();

            Configuration conf;
            conf = jsonLoader.Load(@"{}");
            conf.DocumentLang.Should().Be("ja-JP");
            conf.Variant.Should().Be("");
            conf.MessageLang.Should().Be("ja-JP");
            conf.ValidatorConfigurations.Should().BeEmpty();
            conf.Symbols.Should().BeEmpty();

            // only DocumentLang
            conf = jsonLoader.Load(@"{
    ""DocumentLang"": ""en-US""
}");
            conf.DocumentLang.Should().Be("en-US");
            conf.Variant.Should().Be("");
            // DocumentLangのみの設定の場合、MessageLangはDocumentLangを用いる。
            conf.MessageLang.Should().Be("en-US");
            conf.ValidatorConfigurations.Should().BeEmpty();
            conf.Symbols.Should().BeEmpty();

            // only Variant
            conf = jsonLoader.Load(@"{
    ""Variant"": ""zenkaku""
}");
            conf.DocumentLang.Should().Be("ja-JP");
            conf.Variant.Should().Be("zenkaku");
            conf.MessageLang.Should().Be("ja-JP");
            conf.ValidatorConfigurations.Should().BeEmpty();
            conf.Symbols.Should().BeEmpty();

            // only MessageLang
            conf = jsonLoader.Load(@"{
    ""MessageLang"": ""en-US""
}");
            conf.DocumentLang.Should().Be("ja-JP");
            conf.Variant.Should().Be("");
            // MessageLangのみの設定の場合、DocumentLangはデフォルト値のまま。
            conf.MessageLang.Should().Be("en-US");
            conf.ValidatorConfigurations.Should().BeEmpty();
            conf.Symbols.Should().BeEmpty();

            // ドキュメントは日本語だが、メッセージは英語で出力したい場合。
            conf = jsonLoader.Load(@"{
    ""DocumentLang"": ""ja-JP"",
    ""Variant"": ""zenkaku"",
    ""MessageLang"": ""en-US""
}");
            conf.DocumentLang.Should().Be("ja-JP");
            conf.Variant.Should().Be("zenkaku");
            conf.MessageLang.Should().Be("en-US");
            conf.ValidatorConfigurations.Should().BeEmpty();
            conf.Symbols.Should().BeEmpty();

            // ドキュメントは日本語で、メッセージも特に指定せず日本語で出力したい場合。
            conf = jsonLoader.Load(@"{
    ""DocumentLang"": ""ja-JP"",
    ""Variant"": ""zenkaku""
}");
            conf.DocumentLang.Should().Be("ja-JP");
            conf.Variant.Should().Be("zenkaku");
            conf.MessageLang.Should().Be("ja-JP");
            conf.ValidatorConfigurations.Should().BeEmpty();
            conf.Symbols.Should().BeEmpty();
        }

        /// <summary>
        /// Loads the from json string test.
        /// </summary>
        [Fact]
        public void LoadValidatorConfigurationAndSymbolsTest()
        {
            string jsonString = @"{
    // コメントも書けます。
    ""DocumentLang"": ""ja-JP"",
    ""Variant"": ""zenkaku"",
    ""ValidatorConfigurations"": [
        {
            ""Name"": ""SentenceLength"",
            ""Level"" : ""ERROR"",
            ""MinLength"": 120
        },
        {
            ""Name"": ""JapaneseWordVariation"",
            ""Level"" : ""INFO"",
            ""WordMap"": {
                ""node"": ""ノード"",
                ""edge"": ""エッジ"",
                ""graph"": ""グラフ"",
                ""vertex"": ""バーテックス""
            }
        }
    ],
    ""Symbols"": [
        {
            ""Type"": ""COMMA"",
            ""Value"": ""、"",
            ""InvalidCharsStr"": "",，"",
            ""NeedBeforeSpace"": false,
            ""NeedAfterSpace"": true
        },
        {
            ""Type"": ""FULL_STOP"",
            ""Value"": ""。"",
            ""InvalidCharsStr"": ""．"",
            ""NeedBeforeSpace"": true,
            ""NeedAfterSpace"": true
        }
    ]
}";

            // ライブラリ実装済みのValidatorConfiguration定義を与える。その定義にあるValidatorConfigurationのみロードされる。
            var jsonLoader = new ConfigurationLoader(DefaultValidationDefinition.ValidatorConfTypeDefinitions);
            var configuration = jsonLoader.Load(jsonString);

            configuration.DocumentLang.Should().Be("ja-JP");
            configuration.DocumentCultureInfo.Should().Be(CultureInfo.GetCultureInfo("ja-JP"));
            configuration.Variant.Should().Be("zenkaku");

            configuration.ValidatorConfigurations.Count.Should().Be(2);

            configuration.ValidatorConfigurations[0].Should().BeOfType<SentenceLengthConfiguration>();
            var sentenceLengthConfig = configuration.ValidatorConfigurations[0] as SentenceLengthConfiguration;
            sentenceLengthConfig.Level.Should().Be(ValidationLevel.ERROR);
            sentenceLengthConfig.MinLength.Should().Be(120);

            configuration.ValidatorConfigurations[1].Should().BeOfType<JapaneseWordVariationConfiguration>();
            var japaneseExpressionVariationConfig = configuration.ValidatorConfigurations[1] as JapaneseWordVariationConfiguration;
            japaneseExpressionVariationConfig.Level.Should().Be(ValidationLevel.INFO);
            japaneseExpressionVariationConfig.WordMap.Count.Should().Be(4);
            japaneseExpressionVariationConfig.WordMap["node"].Should().Be("ノード");

            configuration.Symbols.Count.Should().Be(2);
            configuration.Symbols[0].Type.Should().Be(SymbolType.COMMA);
            configuration.Symbols[0].Value.Should().Be('、');
            configuration.Symbols[0].InvalidChars[0].Should().Be(',');
            configuration.Symbols[0].InvalidChars[1].Should().Be('，');
            configuration.Symbols[0].InvalidCharsStr.Should().Be(",，");
            configuration.Symbols[0].NeedBeforeSpace.Should().BeFalse();
            configuration.Symbols[0].NeedAfterSpace.Should().BeTrue();

            configuration.Symbols[1].Type.Should().Be(SymbolType.FULL_STOP);
            configuration.Symbols[1].Value.Should().Be('。');
            configuration.Symbols[1].InvalidChars[0].Should().Be('．');
            configuration.Symbols[1].InvalidCharsStr.Should().Be("．");
            configuration.Symbols[1].NeedBeforeSpace.Should().BeTrue();
            configuration.Symbols[1].NeedAfterSpace.Should().BeTrue();
        }
    }
}
