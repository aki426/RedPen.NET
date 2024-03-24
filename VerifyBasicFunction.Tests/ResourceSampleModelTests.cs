using System.Globalization;
using FluentAssertions;
using Xunit;

namespace VerifyBasicFunction.Tests
{
    public class ResourceSampleModelTests
    {
        [Fact]
        public void GetHelloMessageTest()
        {
            var v = ResourceSampleModel.GetHelloMessage(CultureInfo.InvariantCulture);
            v.Should().Be("Hello, world!");

            v = ResourceSampleModel.GetHelloMessage(new CultureInfo("en-US"));
            v.Should().Be("Hello, world!");

            v = ResourceSampleModel.GetHelloMessage(new CultureInfo("en"));
            v.Should().Be("Hello, world!");

            v = ResourceSampleModel.GetHelloMessage(new CultureInfo("ja-JP"));
            v.Should().Be("こんにちは、世界。");
        }
    }
}
