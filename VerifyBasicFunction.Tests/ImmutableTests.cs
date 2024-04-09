using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace VerifyBasicFunction.Tests
{
    /// <summary>
    /// System.Collections.Immutableの基本的な使い方
    /// </summary>
    public class ImmutableTests
    {
        private ITestOutputHelper _output;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmutableTests"/> class.
        /// </summary>
        /// <param name="output">The output.</param>
        public ImmutableTests(ITestOutputHelper output)
        {
            _output = output;
        }

        /// <summary>
        /// ImmutableListの基本的な使い方
        /// </summary>
        [Fact]
        public void ListTest()
        {
            // Create instance
            var list1 = ImmutableList.Create<int>(1, 2, 3); // (C# 11 以前)

            _output.WriteLine("list1 : 1 2 3");
            list1.ForEach(i => _output.WriteLine(i.ToString()));
            _output.WriteLine(string.Empty);

            // Add
            var list2 = list1.Add(4);  // listとは別のインスタンスが返る

            _output.WriteLine("list2 : 1 2 3 4");
            list2.ForEach(i => _output.WriteLine(i.ToString()));
            _output.WriteLine(string.Empty);

            // Create with Builder
            var builder = ImmutableList.CreateBuilder<int>(); // 空のBuilderを作成
            builder.Add(1);
            builder.Add(2);
            builder.Add(3);
            builder.RemoveAt(1);
            builder.Add(4);
            var list3 = builder.ToImmutable();

            _output.WriteLine("list3 : 1 3 4");
            list3.ForEach(i => _output.WriteLine(i.ToString()));
            _output.WriteLine(string.Empty);

            // Edit with Builder
            var list4 = ImmutableList.Create<int>(1, 2, 3);

            _output.WriteLine("list4 : 1 2 3");
            list4.ForEach(i => _output.WriteLine(i.ToString()));
            _output.WriteLine(string.Empty);

            builder = list4.ToBuilder(); // 既存のImmutableListからBuilderを作成
            builder.Add(4);
            builder.Remove(1); // Removeで直接値を指定して削除
            var list5 = builder.ToImmutable(); // 変更後、ToImmutable()で、Immutableに。

            _output.WriteLine("list5 : 2 3 4");
            list5.ForEach(i => _output.WriteLine(i.ToString()));
            _output.WriteLine(string.Empty);

            list1.Should().Equal(1, 2, 3);
            list2.Should().Equal(1, 2, 3, 4);
            list3.Should().Equal(1, 3, 4);
            list4.Should().Equal(1, 2, 3);
            list5.Should().Equal(2, 3, 4);
        }

        /// <summary>
        /// ImmutableDictionaryの基本的な使い方
        /// </summary>
        [Fact]
        public void DictionaryTest()
        {
            // ImmutableListのようにImmutableDictionary.Createで作成することはできない？
            var voidDictionary = ImmutableDictionary.Create<string, string>();

            // Create instance
            var builder = ImmutableDictionary.CreateBuilder<string, string>();
            builder.Add("key1", "value1");
            builder.Add("key2", "value2");
            var dict1 = builder.ToImmutable();

            dict1.Should().Equal(new Dictionary<string, string> {
                { "key1", "value1" },
                { "key2", "value2" } });
            dict1.Should().NotEqual(new Dictionary<string, string> {
                { "key2", "value1" } }); // 内容が違うのでNotEqual
            dict1.Should().NotEqual(new Dictionary<string, string> {
                { "key1", "value2" },
                { "key2", "value1" } }); // 内容が違うのでNotEqual
            dict1.Should().Equal(new Dictionary<string, string> {
                { "key2", "value2" },
                { "key1", "value1" } }); // Should().EqualはちゃんとDictionaryの内容をチェックできる。

            // Add
            var dict2 = dict1.Add("key3", "value3");

            dict2.Should().Equal(new Dictionary<string, string> {
                { "key1", "value1" },
                { "key2", "value2" },
                { "key3", "value3" } });

            // Edit with Builder
            var builder2 = dict2.ToBuilder();
            builder2.Add("key4", "value4");
            builder2.Remove("key1");
            var dict3 = builder2.ToImmutable();

            dict3.Should().Equal(new Dictionary<string, string> {
                { "key2", "value2" },
                { "key3", "value3" },
                { "key4", "value4" } });

            // AddRange
            var dict4 = dict3.AddRange(new Dictionary<string, string> {
                { "key5", "value5" },
                { "key6", "value6" } });

            dict4.Should().Equal(new Dictionary<string, string> {
                { "key2", "value2" },
                { "key3", "value3" },
                { "key4", "value4" },
                { "key5", "value5" },
                { "key6", "value6" } });

            // 既存のImmutableDictionaryに影響ないことの確認。
            dict2.Should().Equal(new Dictionary<string, string> {
                { "key1", "value1" },
                { "key2", "value2" },
                { "key3", "value3" } });

            dict3.Should().Equal(new Dictionary<string, string> {
                { "key2", "value2" },
                { "key3", "value3" },
                { "key4", "value4" } });
        }
    }
}
