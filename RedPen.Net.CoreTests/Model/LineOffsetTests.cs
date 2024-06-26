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
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace RedPen.Net.Core.Model.Tests
{
    /// <summary>
    /// The line offset tests.
    /// </summary>
    public class LineOffsetTests
    {
        /// <summary>
        /// LineOffsetのMakeOffsetListによって改行が解決されることのテスト。
        /// </summary>
        [Fact]
        public void MakeOffsetListTest()
        {
            // 与えられた文字列を連続するLineOffsetとして返す。
            LineOffset.MakeOffsetList(1, 0, "おはよう。").SequenceEqual(new List<LineOffset>
            {
                new LineOffset(1, 0), // お
                new LineOffset(1, 1), // は
                new LineOffset(1, 2), // よ
                new LineOffset(1, 3), // う
                new LineOffset(1, 4), // 。
            }).Should().BeTrue();

            // 改行コードを解釈する。
            // TODO: 改行コードによるLineの解釈はParserで行うべきかもしれず、処理の責務が曖昧になる可能性がある点に注意する。
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
