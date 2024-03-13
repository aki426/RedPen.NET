using FluentAssertions;
using Xunit;

namespace VerifyBasicFunction.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void TestMethod1()
        {
            (1 + 1).Should().Be(2);
        }
    }
}
