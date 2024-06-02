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

using FluentAssertions;
using RedPen.Net.Core.Parser.Common;
using Xunit;

namespace RedPen.Net.Core.Tests.Parser.Common
{
    public class LineTests
    {
        [Fact]
        public void LineTest()
        {
            Line line = new Line("test", 1);
            line.Should().NotBeNull();
            line.StartsWith("tes").Should().BeTrue();
        }
    }
}
