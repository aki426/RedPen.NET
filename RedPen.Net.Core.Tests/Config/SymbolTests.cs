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
using RedPen.Net.Core.Config;
using Xunit;

namespace RedPen.Net.Core.Tests.Config
{
    /// <summary>
    /// Symbolのテスト
    /// </summary>
    public class SymbolTests
    {
        /// <summary>
        /// 基本的なテストケース。
        /// </summary>
        [Fact]
        public void BasicTest()
        {
            var symbol1 = new Symbol(SymbolType.SPACE, ' ');

            symbol1.Type.Should().Be(SymbolType.SPACE);
            symbol1.Value.Should().Be(' ');
            symbol1.InvalidChars.Should().BeEmpty();
            symbol1.NeedBeforeSpace.Should().BeFalse();
            symbol1.NeedAfterSpace.Should().BeFalse();

            var symbol2 = new Symbol(SymbolType.SPACE, ' ', "abc", true, true);

            symbol2.Type.Should().Be(SymbolType.SPACE);
            symbol2.Value.Should().Be(' ');
            symbol2.InvalidChars.Should().Equal('a', 'b', 'c');
            symbol2.NeedBeforeSpace.Should().BeTrue();
            symbol2.NeedAfterSpace.Should().BeTrue();
        }
    }
}
