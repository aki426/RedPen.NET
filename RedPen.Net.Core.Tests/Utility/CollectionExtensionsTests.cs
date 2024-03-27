using System;
using System.Collections.Generic;
using FluentAssertions;
using RedPen.Net.Core.Utility;
using Xunit;

namespace RedPen.Net.Core.Tests.Utility
{
    /// <summary>
    /// Test for CollectionExtensions
    /// </summary>
    public class CollectionExtensionsTests
    {
        /// <summary>
        /// Test GetValueOrDefault
        /// </summary>
        [Fact]
        public void GetValueOrDefaultTest()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>
            {
                ["apple"] = 1,
                ["banana"] = 2
            };

            dict.GetValueOrDefault("apple").Should().Be(1);
            dict.GetValueOrDefault("banana").Should().Be(2);
            dict.GetValueOrDefault("blueberry").Should().Be(0); // defaulet(int)
            dict.GetValueOrDefault("blueberry", 100).Should().Be(100); // no side effect
            dict.GetValueOrDefault("blueberry", 10).Should().Be(10); // no side effect
        }

        /// <summary>
        /// JAVAのComputeIf*のC#移植版のテスト。
        /// </summary>
        [Fact]
        public void ComputeIfTest()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>
            {
                ["apple"] = 1,
                ["banana"] = 2
            };

            // ComputeIfAbsentは値が存在しなかった場合にのみ副作用を伴う。
            dict.ComputeIfAbsent("apple", k => 42).Should().Be(1);
            dict.ComputeIfAbsent("apple", k => 43).Should().Be(1);
            dict["apple"].Should().Be(1); // no side effect

            dict.ContainsKey("blueberry").Should().BeFalse();
            dict.ComputeIfAbsent("blueberry", k => 3).Should().Be(3);
            dict.ContainsKey("blueberry").Should().BeTrue();
            dict.ComputeIfAbsent("blueberry", k => 4).Should().Be(3);
            dict["blueberry"].Should().Be(3); // side effect

            // MEMO: ComputeIfPresentはJAVA版の挙動を再現できないため不使用とする。
            //// ComputeIfPresentは値が存在した場合にのみ副作用を伴う。
            //dict.ContainsKey("blueberry").Should().BeTrue();
            //dict["blueberry"].Should().Be(3); // side effect
            //dict.ComputeIfPresent("blueberry", (k, v) => 4).Should().Be((true, 4));
            //dict["blueberry"].Should().Be(4); // side effect

            //dict.ContainsKey("orange").Should().BeFalse();
            //dict.ComputeIfPresent("orange", (k, v) => 5).Should().Be((false, 0)); // default(int)
            //dict.ContainsKey("orange").Should().BeFalse(); // no side effect

            //// ComputeIfPresentはデリゲートの返す値がnullの場合、要素を削除する。
            //dict.ContainsKey("blueberry").Should().BeTrue();
            //dict.ComputeIfPresent("blueberry", (k, v) => null).Should().Be((false, 0));
            //dict.ContainsKey("blueberry").Should().BeFalse(); // side effect
        }
    }
}
