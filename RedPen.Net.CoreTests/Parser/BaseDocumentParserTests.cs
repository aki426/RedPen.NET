//   Copyright (c) 2024 KANEDA Akihiro <taoist.aki@gmail.com>
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Parser.Tests
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
