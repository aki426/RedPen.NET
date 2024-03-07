﻿using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace redpen_core.parser.Tests
{
    public class LineOffsetTests
    {
        private readonly ITestOutputHelper output;

        public LineOffsetTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact()]
        public void CompareToTest()
        {
            // Offset比較。
            LineOffset lineOneOffsetOne = new LineOffset(1, 1);
            LineOffset lineOneOffsetTwo = new LineOffset(1, 2);

            // 自分が先の場合0以下。
            lineOneOffsetOne.CompareTo(lineOneOffsetTwo).Should().BeLessThan(0);
            lineOneOffsetTwo.CompareTo(lineOneOffsetOne).Should().BeGreaterThan(0);

            // Line比較。
            LineOffset lineTwoOffsetOne = new LineOffset(2, 1);
            lineOneOffsetOne.CompareTo(lineTwoOffsetOne).Should().BeLessThan(0);
            lineTwoOffsetOne.CompareTo(lineOneOffsetOne).Should().BeGreaterThan(0);

            // 同じ場合は0。
            LineOffset oneOne = new LineOffset(1, 1);
            lineOneOffsetOne.CompareTo(oneOne).Should().Be(0);
        }

        [Fact()]
        public void CompareToNullTest()
        {
            LineOffset lineOneOffsetOne = new LineOffset(1, 1);
            lineOneOffsetOne.CompareTo(null).Should().BeGreaterThan(0);
        }

        [Fact()]
        public void LineOffsetBasicTest()
        {
            LineOffset lineOneOffsetOne = new LineOffset(1, 1);
            LineOffset lineOneOffsetTwo = new LineOffset(1, 2);
            LineOffset oneOne = new LineOffset(1, 1);

            (lineOneOffsetOne == lineOneOffsetTwo).Should().BeFalse();
            (lineOneOffsetOne == oneOne).Should().BeTrue();

            lineOneOffsetOne.Equals(lineOneOffsetTwo).Should().BeFalse();
            lineOneOffsetOne.Equals(oneOne).Should().BeTrue();

            lineOneOffsetTwo.ToString().Should().Be("LineOffset { LineNum = 1, Offset = 2 }");
            output.WriteLine(lineOneOffsetTwo.ToString());
        }
    }
}
