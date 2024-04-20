using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using RedPen.Net.Core.Parser;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Parser
{
    public class BaseDocumentParserTests
    {
        private ITestOutputHelper output;

        public BaseDocumentParserTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void StringToMemoryStreamTest()
        {
            string content = @"This is a pen.
That is a orange.

However, pen is not oranges.
We need to be peisient.

Happy life.
Happy home.
Tama Home.
";

            content.ToCharArray().Where(c => c == '\r').Count().Should().Be(9);
            content.ToCharArray().Where(c => c == '\n').Count().Should().Be(9);

            /// <summary>BOM無しUTF-8エンコーディング</summary>
            UTF8Encoding utf8Encoding = new UTF8Encoding(false);

            // BOM無しUTF-8エンコーディング
            // UTF8Encoding utf8Encoding = new System.Text.UTF8Encoding(false);
            byte[] byteArray = utf8Encoding.GetBytes(content);
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                PreprocessingReader br = new PreprocessingReader(stream, new PlainTextParser());
                // StreamReader br = new StreamReader(stream);
                string line;
                // int linesRead = 0;
                int linesRead = 0;
                string paragraph = "";
                try
                {
                    while ((line = br.ReadLine()) != null)
                    {
                        linesRead++;
                        output.WriteLine($"{linesRead} : {line}");
                        // linesRead++;
                    }
                }
                catch (IOException e)
                {
                    throw new RedPenException(e);
                }
            }
        }
    }
}
