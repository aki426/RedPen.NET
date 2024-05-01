using System.Globalization;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Errors;
using Xunit;

namespace RedPen.Net.Core.Tests.Errors
{
    public class ErrorMessageManagerTests
    {
        [Fact]
        public void BasicTest()
        {
            var manager = ErrorMessageManager.GetInstance();

            manager.GetErrorMessage(
                ValidationType.SentenceLength, "",
                CultureInfo.GetCultureInfo("ja-JP"),
                new object[] { 30, 20 })
                    .Should().Be("文の長さ（30）が最大値（20）を超えています。");

            manager.GetErrorMessage(
                ValidationType.SpaceWithAlphabeticalExpression, "After",
                CultureInfo.GetCultureInfo("ja-JP"),
                new object[] { "hogehoge" })
                    .Should().Be("半角アルファベット表現 \"hogehoge\" の後にスペースが必要です。");
        }
    }
}
