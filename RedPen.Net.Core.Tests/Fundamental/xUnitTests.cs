using FluentAssertions;
using RedPen.Net.Core.Model;
using Xunit;

namespace RedPen.Net.Core.Tests.Fundamental
{
    public class xUnitTests
    {
        [Fact]
        public void FundamentalTest1()
        {
            (1 + 2).Should().Be(3);
        }

        [Fact]
        public void FundamentalTest2()
        {
            // new Sentence(1).Sum(2).Should().Be(3);
        }
    }
}
