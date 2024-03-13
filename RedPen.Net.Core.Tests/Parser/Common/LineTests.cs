using FluentAssertions;
using RedPen.Net.Core.Parser.Common;
using Xunit;

namespace RedPen.Net.Core.Tests.Parser.Common
{
    public class LineTests
    {
        [Fact]
        public void LineTest()
        {
            Line line = new Line("test", 1);
            line.Should().NotBeNull();
            line.StartsWith("tes").Should().BeTrue();
        }
    }
}