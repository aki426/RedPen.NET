using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using RedPen.Net.Core.Model;
using Xunit;

namespace RedPen.Net.Core.Tests.Model
{
    public class LineOffsetTests
    {
        [Fact]
        public void MakeOffsetListTest()
        {
            LineOffset.MakeOffsetList(1, 0, "おはよう。").SequenceEqual(new List<LineOffset>
            {
                new LineOffset(1, 0), // お
                new LineOffset(1, 1), // は
                new LineOffset(1, 2), // よ
                new LineOffset(1, 3), // う
                new LineOffset(1, 4), // 。
            }).Should().BeTrue();

            LineOffset.MakeOffsetList(1, 0, "おは\nよう。").SequenceEqual(new List<LineOffset>
            {
                new LineOffset(1, 0), // お
                new LineOffset(1, 1), // は
                new LineOffset(1, 2), // \n
                new LineOffset(2, 0), // よ
                new LineOffset(2, 1), // う
                new LineOffset(2, 2), // 。
            }).Should().BeTrue();

            LineOffset.MakeOffsetList(1, 0, "\nおは\nよう。").SequenceEqual(new List<LineOffset>
            {
                new LineOffset(1, 0), // \n
                new LineOffset(2, 0), // お
                new LineOffset(2, 1), // は
                new LineOffset(2, 2), // \n
                new LineOffset(3, 0), // よ
                new LineOffset(3, 1), // う
                new LineOffset(3, 2), // 。
            }).Should().BeTrue();
        }
    }
}
