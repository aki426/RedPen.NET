using System;
using FluentAssertions;
using RedPen.Net.Core.Config;
using Xunit;

namespace RedPen.Net.Core.Tests.Config
{
    public class SymbolTypeTests
    {
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
