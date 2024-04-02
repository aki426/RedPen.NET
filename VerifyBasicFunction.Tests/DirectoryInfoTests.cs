using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace VerifyBasicFunction.Tests
{
    public class DirectoryInfoTests
    {
        private readonly ITestOutputHelper output;

        public DirectoryInfoTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void CurrentDirectoryTest()
        {
            output.WriteLine($"Directory.GetCurrentDirectory() : {Directory.GetCurrentDirectory()}");
            output.WriteLine($"new DirectoryInfo(@\".\\\") : {new DirectoryInfo(@".\")}");
        }
    }
}
