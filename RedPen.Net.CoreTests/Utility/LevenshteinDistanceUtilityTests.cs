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
using Xunit;

namespace RedPen.Net.Core.Utility.Tests
{
    public class LevenshteinDistanceUtilityTests
    {
        /// <summary>
        /// Basics the test.
        /// </summary>
        /// <param name="nouse1">The nouse1.</param>
        /// <param name="a">The a.</param>
        /// <param name="b">The b.</param>
        /// <param name="ins">The ins.</param>
        /// <param name="del">The del.</param>
        /// <param name="subs">The subs.</param>
        /// <param name="expected">The expected.</param>
        [Theory]
        [InlineData("001", "ディスプレイ", "ディスプレー", 1, 1, 1, 1)]
        [InlineData("002", "ディスプレー", "ディスプレイ", 1, 1, 1, 1)]
        [InlineData("003", "インチディスプレイ", "ディスプレイ", 1, 1, 1, 3)]
        [InlineData("004", "ディスプレイ", "インチディスプレイ", 1, 1, 1, 3)]
        [InlineData("005", "インデックス", "インデクス", 1, 1, 1, 1)]
        [InlineData("006", "インデクス", "インデックス", 1, 1, 1, 1)]
        // 1文字置換
        [InlineData("007", "ライダー", "ライター", 1, 1, 1, 1)]
        [InlineData("008", "ライター", "ライダー", 1, 1, 1, 1)]
        // 1文字挿入（削除）
        [InlineData("009", "ライダー", "ライダ", 1, 1, 1, 1)]
        [InlineData("010", "ライダ", "ライダー", 1, 1, 1, 1)]
        // 1文字削除（挿入）
        [InlineData("011", "ラダー", "ライダー", 1, 1, 1, 1)]
        [InlineData("012", "ライダー", "ラダー", 1, 1, 1, 1)]
        // 各編集コストが同じなら、順番を入れ替えても結果は同じになる。
        [InlineData("013", "ダイダラボッチ", "ダイコンサラダ", 1, 1, 1, 5)]
        [InlineData("014", "ダイコンサラダ", "ダイダラボッチ", 1, 1, 1, 5)]
        [InlineData("015", "ダイコン", "サラダ", 1, 1, 1, 4)]
        [InlineData("016", "サラダ", "ダイコン", 1, 1, 1, 4)]
        [InlineData("017", "フォーマット", "形式", 1, 1, 1, 6)]
        [InlineData("018", "形式", "フォーマット", 1, 1, 1, 6)]
        // 各編集コストが異なる場合、順番を入れ替えると結果が異なる。計算ロジック的にもこれは正しい。
        [InlineData("019", "ダイダラボッチ", "ダイコンサラダ", 2, 3, 5, 20)]
        [InlineData("020", "ダイコンサラダ", "ダイダラボッチ", 2, 3, 5, 20)]
        [InlineData("021", "ダイコン", "サラダ", 2, 3, 5, 13)]
        [InlineData("022", "サラダ", "ダイコン", 2, 3, 5, 12)]
        [InlineData("023", "フォーマット", "形式", 2, 3, 5, 22)]
        [InlineData("024", "形式", "フォーマット", 2, 3, 5, 18)]
        public void BasicTest(string nouse1, string a, string b, int ins, int del, int subs, int expected)
        {
            var utility = new LevenshteinDistanceUtility(ins, del, subs);
            utility.GetDistance(a, b).Should().Be(expected);
        }

        /// <summary>
        /// LevenSteinDistanceのメモ化キャッシュ関数版のテスト。
        /// 3つの編集コストが同じ場合は与える文字列の順序によって結果の値は左右されず同一になる。
        /// </summary>
        [Fact]
        public void MemoizeTest()
        {
            var utility = new LevenshteinDistanceUtility();
            utility.Cache.Count.Should().Be(0);
            utility.GetDistanceMemoize("ダイダラボッチ", "ダイコンサラダ").Should().Be(5);
            utility.Cache.Count.Should().Be(1);
            utility.GetDistanceMemoize("ダイコンサラダ", "ダイダラボッチ").Should().Be(5);
            utility.Cache.Count.Should().Be(1);
            utility.GetDistanceMemoize("ダイコン", "サラダ").Should().Be(4);
            utility.Cache.Count.Should().Be(2);
            utility.GetDistanceMemoize("ダイコン", "サラダ").Should().Be(4);
            utility.GetDistanceMemoize("ダイコン", "サラダ").Should().Be(4);
            utility.GetDistanceMemoize("サラダ", "ダイコン").Should().Be(4);
            utility.Cache.Count.Should().Be(2);
        }

        /// <summary>
        /// LevenSteinDistanceは編集コストが異なると与える文字列の順序によって結果の値が異なる。
        /// そのためメモ化キャッシュも文字列の順序を考慮した個数必要になる。
        /// </summary>
        [Fact]
        public void CachingTest()
        {
            var utility = new LevenshteinDistanceUtility(2, 3, 5);
            utility.Cache.Count.Should().Be(0);
            utility.GetDistanceMemoize("ダイダラボッチ", "ダイコンサラダ").Should().Be(20);
            utility.Cache.Count.Should().Be(1);
            utility.GetDistanceMemoize("ダイコンサラダ", "ダイダラボッチ").Should().Be(20);
            utility.Cache.Count.Should().Be(2);
            utility.GetDistanceMemoize("ダイコン", "サラダ").Should().Be(13);
            utility.Cache.Count.Should().Be(3);
            utility.GetDistanceMemoize("ダイコン", "サラダ").Should().Be(13);
            utility.GetDistanceMemoize("ダイコン", "サラダ").Should().Be(13);
            utility.Cache.Count.Should().Be(3);
            utility.GetDistanceMemoize("サラダ", "ダイコン").Should().Be(12);
            utility.Cache.Count.Should().Be(4);
        }
    }
}
