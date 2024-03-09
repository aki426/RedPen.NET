using System.Collections;
using System.Collections.Specialized;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace redpen_core.config.Tests
{
    /// <summary>OrderedDictionary(JAVAのLinedHashMapに相当)の基本機能確認用テストケース</summary>
    public class OrderedDictionaryTests
    {
        private readonly ITestOutputHelper output;

        public OrderedDictionaryTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>OrderedDictionaryの基本機能確認用テストケース</summary>
        [Fact()]
        public void OrderedDictionaryTest()
        {
            // SetUp
            OrderedDictionary od = new OrderedDictionary();
            od.Add(SymbolType.COMMA, ",");
            od.Add(SymbolType.FULL_STOP, ".");
            od.Add(SymbolType.AT_MARK, "@");

            // Assertion
            SymbolType[] keyArray = new SymbolType[od.Count];
            System.Collections.ICollection keys = od.Keys;
            keys.CopyTo(keyArray, 0);

            keyArray[0].Should().Be(SymbolType.COMMA);
            keyArray[1].Should().Be(SymbolType.FULL_STOP);
            keyArray[2].Should().Be(SymbolType.AT_MARK);

            od[SymbolType.COMMA].Should().Be(",");
            od[SymbolType.FULL_STOP].Should().Be(".");
            od[SymbolType.AT_MARK].Should().Be("@");

            // Arrange
            // od.Add(SymbolType.FULL_STOP, "。"); // これはエラーになる。
            od.Contains(SymbolType.COMMA).Should().BeTrue();
            od[SymbolType.FULL_STOP] = "。";

            // Assertion
            keyArray = new SymbolType[od.Count];
            keys = od.Keys;
            keys.CopyTo(keyArray, 0);

            keyArray[0].Should().Be(SymbolType.COMMA);
            keyArray[1].Should().Be(SymbolType.FULL_STOP);
            keyArray[2].Should().Be(SymbolType.AT_MARK);

            od[SymbolType.COMMA].Should().Be(",");
            od[SymbolType.FULL_STOP].Should().Be("。"); // 順番は変わらずValueだけ変わったことの確認。
            od[SymbolType.AT_MARK].Should().Be("@");

            // varを使うとobjectになってしまうので、DictionaryEntryを指定する。
            // https://kan-kikuchi.hatenablog.com/entry/SortedDictionary_OrderedDictionary
            foreach (DictionaryEntry pair in od)
            {
                output.WriteLine($"Key: {pair.Key}, Value: {pair.Value}");
            }
        }

        [Fact]
        public void OrderedDictionaryAsIDictionaryTest()
        {
            // SetUp
            OrderedDictionary od = new OrderedDictionary();
            od.Add(SymbolType.COMMA, ",");
            od.Add(SymbolType.FULL_STOP, ".");
            od.Add(SymbolType.AT_MARK, "@");

            // OrderedDictionaryは古い実装のため融通が効かないのでDictionaryへの変換時も手間がかかる。
            Dictionary<SymbolType, string> dict = new Dictionary<SymbolType, string>();
            foreach (DictionaryEntry pair in od)
            {
                dict[(SymbolType)pair.Key] = (string)pair.Value;
            }

            // Assertion
            dict[SymbolType.COMMA].Should().Be(",");
            dict[SymbolType.FULL_STOP].Should().Be(".");
            dict[SymbolType.AT_MARK].Should().Be("@");
        }

        [Fact]
        public void DictionaryOverwriteTest()
        {
            // SetUp
            Dictionary<SymbolType, string> dict = new Dictionary<SymbolType, string>();
            dict.Add(SymbolType.COMMA, ",");
            dict.Add(SymbolType.FULL_STOP, ".");
            dict.Add(SymbolType.AT_MARK, "@");
            dict[SymbolType.ASTERISK] = "*";

            foreach (KeyValuePair<SymbolType, string> pair in dict)
            {
                output.WriteLine($"Key: {pair.Key}, Value: {pair.Value}");
            }

            // Arrange
            dict[SymbolType.COMMA] = "、";
            dict[SymbolType.FULL_STOP] = "。";
            dict.TryAdd(SymbolType.COMMA, "アットマーク");
            dict[SymbolType.ASTERISK] = "アスタリスク";

            foreach (KeyValuePair<SymbolType, string> pair in dict)
            {
                output.WriteLine($"Key: {pair.Key}, Value: {pair.Value}");
            }
        }
    }
}
