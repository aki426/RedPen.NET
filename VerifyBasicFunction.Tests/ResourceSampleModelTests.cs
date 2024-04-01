using System.Globalization;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace VerifyBasicFunction.Tests
{
    public class ResourceSampleModelTests
    {
        private ITestOutputHelper output;

        public ResourceSampleModelTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void GetHelloMessageTest()
        {
            // デフォルトカルチャ
            // このソリューションではResXManagerのデフォルトカルチャをen-USとしているため。
            ResourceSampleModel.GetHelloMessage(CultureInfo.InvariantCulture)
                .Should().Be("Hello, world!");

            // enの指定方法
            ResourceSampleModel.GetHelloMessage(new CultureInfo("en-US"))
                .Should().Be("Hello, world!");
            // "en"と"en-US"は厳密には異なる。"en"の登録が無いのでデフォルトカルチャの結果が返るものと思われる。
            ResourceSampleModel.GetHelloMessage(new CultureInfo("en"))
                .Should().Be("Hello, world!");

            // jaの指定方法
            ResourceSampleModel.GetHelloMessage(new CultureInfo("ja-JP"))
                .Should().Be("こんにちは、世界。");
            // "ja"と"ja-JP"は厳密には異なる。
            ResourceSampleModel.GetHelloMessage(new CultureInfo("ja"))
                .Should().Be("ja指定のこんにちは、世界。");

            // 登録されていないカルチャの場合、デフォルトカルチャが返る
            ResourceSampleModel.GetHelloMessage(new CultureInfo("fr"))
                .Should().Be("Hello, world!");
            ResourceSampleModel.GetHelloMessage(new CultureInfo("fr-FR"))
                .Should().Be("Hello, world!");

            // 登録はあるが実体が無くresxファイルも作られていないカルチャの場合も、デフォルトカルチャが返る
            ResourceSampleModel.GetHelloMessage(new CultureInfo("zh"))
                .Should().Be("Hello, world!");
            ResourceSampleModel.GetHelloMessage(new CultureInfo("zh-TW"))
                .Should().Be("Hello, world!");
        }

        [Fact]
        public void GetMessageTest()
        {
            // 対応するプロパティがある場合
            ResourceSampleModel.GetMessage("HelloWorld", new CultureInfo("en-US"))
                .Should().Be("Hello, world!");

            // 対応するプロパティがない場合、nullが返る
            ResourceSampleModel.GetMessage("HogehogeWorld", new CultureInfo("en-US"))
                .Should().BeNull();
        }

        [Fact]
        public void GetEmbeddedResourceTest()
        {
            // テストプロジェクトのリソースファイルの内容を取得する
            var list = ResourceSampleModel.GetEmbeddedResource();

            list.Count.Should().Be(3);
            list[0].Should().Be("node\tノード");
            list[1].Should().Be("log\tログ");
        }

        [Fact]
        public void GetManifestResourcesTest()
        {
            // テストプロジェクトのリソースファイルの内容を取得する
            var list = ResourceSampleModel.GetManifestResources();

            foreach (var item in list)
            {
                output.WriteLine(item);
            }
            //list.Length.Should().Be(1);
            //list[0].Should().Be("VerifyBasicFunction.Resources.ParentDirectory.SampleText.ja.txt");

            /*
                 <None Remove="Resources\ParentDirectory\SampleText.ja.txt" />
                  </ItemGroup>
                  <ItemGroup>

             */
        }
    }
}
