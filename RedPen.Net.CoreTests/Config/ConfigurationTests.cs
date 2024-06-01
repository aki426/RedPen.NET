using FluentAssertions;
using Xunit;

namespace RedPen.Net.Core.Config.Tests
{
    public class ConfigurationTests
    {
        [Fact()]
        public void ConfigurationTest()
        {
            // デフォルトコンストラクタで生成された後、オブジェクト初期化子で初期化されなかった場合すべてデフォルト値となる。
            var defaultConstructed = new Configuration();
            defaultConstructed.DocumentLang.Should().Be("ja-JP");
            defaultConstructed.Variant.Should().Be("");
            defaultConstructed.MessageLang.Should().Be("ja-JP");
            defaultConstructed.ValidatorConfigurations.Should().BeEmpty();
            defaultConstructed.Symbols.Should().BeEmpty();

            // デフォルトコンストラクタの場合、各初期値は他の値に影響を与えない。
            var withDocumentLang = new Configuration() { DocumentLang = "en-US" };
            withDocumentLang.DocumentLang.Should().Be("en-US");
            withDocumentLang.Variant.Should().Be("");
            withDocumentLang.MessageLang.Should().Be("ja-JP");

            var withVariant = new Configuration() { Variant = "zenkaku" };
            withVariant.DocumentLang.Should().Be("ja-JP");
            withVariant.Variant.Should().Be("zenkaku");
            withVariant.MessageLang.Should().Be("ja-JP");

            var withMessageLang = new Configuration() { MessageLang = "en-US" };
            withMessageLang.DocumentLang.Should().Be("ja-JP");
            withMessageLang.Variant.Should().Be("");
            withMessageLang.MessageLang.Should().Be("en-US");
        }
    }
}
