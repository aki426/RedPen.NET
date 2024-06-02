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
using FluentAssertions;
using Xunit;

namespace RedPen.Net.Core.Tests
{
    // TODO: ResourceFileLoaderクラスで代替可能であれば捨てる。

    /// <summary>
    /// The red pen utility tests.
    /// </summary>
    public class RedPenUtilityTests
    {
        /// <summary>
        /// RedPenの設定ファイルのmap形式プロパティ文字列をParseするテスト。
        /// </summary>
        [Fact]
        public void ParseMapTest()
        {
            Dictionary<string, string> dict;

            // 空文字列を渡した場合、空のDictionaryが返ることを確認する。
            dict = RedPenUtility.ParseMap("");
            dict.Count.Should().Be(0);

            // Parseする文字列がNULLの場合
            dict = RedPenUtility.ParseMap("{}");
            dict.Count.Should().Be(0);

            dict = RedPenUtility.ParseMap("{,}, {,}");
            dict.Count.Should().Be(1);
            dict[""].Should().Be("");

            // Parseする文字列が不正な場合の挙動はJAVA版のベタ移植から変更していない。
            Action act = () => RedPenUtility.ParseMap("{,}, {");
            act.Should().Throw<ArgumentOutOfRangeException>();

            // 一般的なケース。
            dict = RedPenUtility.ParseMap("{smart,スマート},{distributed,ディストリビューテッド}");
            dict.Count.Should().Be(2);
            dict["smart"].Should().Be("スマート");
            dict["distributed"].Should().Be("ディストリビューテッド");

            // 重複するKeyがある場合は後勝ち。
            dict = RedPenUtility.ParseMap("{smart,スマート},{distributed,ディストリビューテッド},{smart,スマッシュ}");
            dict.Count.Should().Be(2);
            dict["smart"].Should().Be("スマッシュ");
            dict["distributed"].Should().Be("ディストリビューテッド");
        }
    }
}
