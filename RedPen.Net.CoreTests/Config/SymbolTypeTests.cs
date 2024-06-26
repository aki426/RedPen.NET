﻿//   Copyright (c) 2024 KANEDA Akihiro <taoist.aki@gmail.com>
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
using FluentAssertions;
using Xunit;

namespace RedPen.Net.Core.Config.Tests
{
    /// <summary>
    /// The symbol type tests.
    /// </summary>
    public class SymbolTypeTests
    {
        /// <summary>
        /// EnumであるSymbolTypeに対して基本関数のテスト。
        /// </summary>
        [Fact]
        public void ConvertSymbolTypeToNameTest()
        {
            // SymbolTypeの名前を取得する
            SymbolType.COMMA.ToString().Should().Be("COMMA");

            // 名前からSymbolTypeを取得する
            Array array = Enum.GetValues(typeof(SymbolType));
            SymbolType? symbolType = null;
            foreach (var value in array)
            {
                if (value.ToString() == "COMMA")
                {
                    symbolType = (SymbolType)value;
                    break;
                }
            }

            symbolType.Should().Be(SymbolType.COMMA);
            symbolType.Should().NotBeNull();
        }
    }
}
