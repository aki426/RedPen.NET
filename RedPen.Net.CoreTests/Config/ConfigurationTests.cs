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

            //var defaultConstructedWithDocumentLang = new Configuration() { DocumentLang = "en-US" };
        }
    }
}
