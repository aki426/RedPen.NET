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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Model;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Parser
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

            new List<LineOffset> {
                new LineOffset(2, 1),
                new LineOffset(3, 2),
                new LineOffset(1, 2),
                new LineOffset(2, 2),
                new LineOffset(1, 1),
            }
                .OrderBy(x => x).ToList().Should().Equal(new List<LineOffset> {
                    new LineOffset(1, 1),
                    new LineOffset(1, 2),
                    new LineOffset(2, 1),
                    new LineOffset(2, 2),
                    new LineOffset(3, 2),
                });
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

        [Fact]
        public void FirstAndFirstOrDefaultTest()
        {
            ImmutableList.Create<LineOffset>().FirstOrDefault().Should().BeNull(); // null

            Action act = () => ImmutableList.Create<LineOffset>().First();
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("シーケンスに要素が含まれていません"); // Exception

            ImmutableList<LineOffset>.Builder builder = ImmutableList.CreateBuilder<LineOffset>();
            builder.Add(new LineOffset(1, 1));
            builder.Add(new LineOffset(2, 2));
            var immutable = builder.ToImmutable();

            immutable.First().Should().Be(new LineOffset(1, 1));
            immutable.FirstOrDefault().Should().Be(new LineOffset(1, 1));
        }
    }
}
