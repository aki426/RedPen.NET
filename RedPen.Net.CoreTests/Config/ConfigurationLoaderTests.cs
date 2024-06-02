using System;
using FluentAssertions;
using RedPen.Net.Core.Globals;
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
    }
}
