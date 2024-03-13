using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace RedPen.Net.Core.Tests.Fundamental
{
    public class EnvironmantTests
    {
        [Fact]
        public void EnvironmentValiableTest()
        {
            Action act;

            Environment.GetEnvironmentVariable("ABSOLUTORY_NOT_EXIST_ENVIRONMENT_VARIABLE").Should().BeNull();
            Environment.GetEnvironmentVariable(null).Should().BeNull();

            act = () => new FileInfo(string.Empty);
            act.Should().Throw<ArgumentException>();
        }
    }
}
