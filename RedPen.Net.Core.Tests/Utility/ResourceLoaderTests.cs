using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using RedPen.Net.Core.Utility;
using Xunit;

namespace RedPen.Net.Core.Tests.Utility
{
    public class ResourceLoaderTests
    {
        [Fact]
        public void ErrorMessageLoaderTest()
        {
            var resourceLoader = new ResourceLoader();

            // 英語のエラーメッセージ
            string englishMessage = resourceLoader.GetErrorMessage("SentenceLengthValidator", CultureInfo.InvariantCulture);
            englishMessage.Should().Be("The length of the sentence ({0}) exceeds the maximum of {1}.");

            // 日本語のエラーメッセージ
            string japaneseMessage = resourceLoader.GetErrorMessage("SentenceLengthValidator", new CultureInfo("ja-JP"));
            japaneseMessage.Should().Be("文長（\"{0}\"）が最大値 \"{1}\" を超えています。");
        }
    }
}
