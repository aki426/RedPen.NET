using System.Globalization;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Validators.DocumentValidator;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;

namespace RedPen.Net.Core.Tests.Config
{
    /// <summary>ConfigurationをJson形式文字列から読み取るLoaderのテスト。</summary>
    public class ConfigurationLoaderTest
    {
        /// <summary>
        /// Loads the from json string test.
        /// </summary>
        [Fact]
        public void LoadFromJsonStringTest()
        {
            string jsonString = @"{
    ""Lang"": ""ja-JP"",
    ""Variant"": ""zenkaku"",
    ""ValidatorConfigurations"": [
        {
            ""Name"": ""SentenceLength"",
            ""Level"" : ""ERROR"",
            ""MaxLength"": 120
        },
        {
            ""Name"": ""JapaneseExpressionVariation"",
            ""Level"" : ""INFO"",
            ""WordMap"": {
                ""node"": ""ノード"",
                ""edge"": ""エッジ"",
                ""graph"": ""グラフ"",
                ""vertex"": ""頂点""
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

            var configuration = ConfigurationLoader.Load(jsonString);
            configuration.Lang.Should().Be("ja-JP");
            configuration.CultureInfo.Should().Be(CultureInfo.GetCultureInfo("ja-JP"));
            configuration.Variant.Should().Be("zenkaku");

            configuration.ValidatorConfigurations.Count.Should().Be(2);

            configuration.ValidatorConfigurations[0].Should().BeOfType<SentenceLengthConfiguration>();
            var sentenceLengthConfig = configuration.ValidatorConfigurations[0] as SentenceLengthConfiguration;
            sentenceLengthConfig.Level.Should().Be(ValidationLevel.ERROR);
            sentenceLengthConfig.MaxLength.Should().Be(120);

            configuration.ValidatorConfigurations[1].Should().BeOfType<JapaneseExpressionVariationConfiguration>();
            var japaneseExpressionVariationConfig = configuration.ValidatorConfigurations[1] as JapaneseExpressionVariationConfiguration;
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
