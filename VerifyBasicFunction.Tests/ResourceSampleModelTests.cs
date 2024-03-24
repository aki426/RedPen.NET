using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Globalization;
using FluentAssertions;

namespace VerifyBasicFunction.Tests
{
    public class ResourceSampleModelTests
    {
        [Fact]
        public void GetHelloMessageTest()
        {
            var v = ResourceSampleModel.GetHelloMessage(CultureInfo.InvariantCulture);
            v.Should().Be("こんにちは、世界。");

            v = ResourceSampleModel.GetHelloMessage(new CultureInfo("en-US"));
            v.Should().Be("Hello, world!");

            v = ResourceSampleModel.GetHelloMessage(new CultureInfo("en"));
            v.Should().Be("Hello, world!");
        }
    }
}
