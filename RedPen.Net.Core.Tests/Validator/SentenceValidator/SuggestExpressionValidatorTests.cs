using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validator.SentenceValidator
{
    public class SuggestExpressionValidatorTests
    {
        private ITestOutputHelper output;

        public SuggestExpressionValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void FrameworkBasicTest()
        {
            // 文字が文字(アルファベット)かどうかを判定する。
            char.IsLetter('a').Should().BeTrue();
            char.IsLetter('1').Should().BeFalse();

            // 文字列の各文字を判定する。
            "Hello今日は雨".ToList().ForEach(c =>
            {
                char.IsLetter(c).Should().BeTrue();
            });
            // 記号系は期待通り文字として判定されない。
            " 123。「　』".ToList().ForEach(c =>
            {
                char.IsLetter(c).Should().BeFalse();
            });
        }
    }
}
