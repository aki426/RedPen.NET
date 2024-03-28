using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace VerifyBasicFunction.Tests
{
    /// <summary>
    /// CultureInfoについて基本機能を確認するためのテスト。
    /// </summary>
    public class CultureInfoTests
    {
        [Fact]
        public void BasicFunctionsTest()
        {
            var jajp = new CultureInfo("ja-JP");
            var ja = new CultureInfo("ja");
            var enus = new CultureInfo("en-US");
            var en = new CultureInfo("en");
            var invariant = CultureInfo.InvariantCulture;

            // MEMO: CurrentCultureとCurrentUICultureはスレッドのカルチャで実行時に変更される可能性がある。
            // またこの2つの役割については次のページが詳しい。
            // https://smdn.jp/programming/netfx/locale/0_abstract/

            // 実行時にカルチャ固有のリソースを検索するためにリソース マネージャーで使用されるカルチャ。
            CultureInfo.CurrentUICulture.Should().Be(CultureInfo.InstalledUICulture);
            // 開発時のテスト環境は日本語環境のため、日本語が設定されている。
            // このため、DisplayNameは日本語カルチャが適用され日本語になる。
            CultureInfo.InstalledUICulture.Should().Be(jajp);

            // MEMO: 指定しなければスレッドノカルチャはNull？
            CultureInfo.DefaultThreadCurrentCulture.Should().BeNull();

            // Name - 通常この言語コードを使用するのが安全。
            // TODO: C#版の文字指定コードはこの形式に変更する。
            jajp.Name.Should().Be("ja-JP");
            ja.Name.Should().Be("ja");
            enus.Name.Should().Be("en-US");
            en.Name.Should().Be("en");
            invariant.Name.Should().Be("");

            // DisplayName
            jajp.DisplayName.Should().Be("日本語 (日本)");
            ja.DisplayName.Should().Be("日本語");
            enus.DisplayName.Should().Be("英語 (米国)");
            en.DisplayName.Should().Be("英語");
            invariant.DisplayName.Should().Be("ロケールに依存しない言語 (ロケールに依存しない国)");

            // EnglishName
            jajp.EnglishName.Should().Be("Japanese (Japan)");
            ja.EnglishName.Should().Be("Japanese");
            enus.EnglishName.Should().Be("English (United States)");
            en.EnglishName.Should().Be("English");
            invariant.EnglishName.Should().Be("Invariant Language (Invariant Country)");

            // TwoLetterISOLanguageName
            jajp.TwoLetterISOLanguageName.Should().Be("ja");
            ja.TwoLetterISOLanguageName.Should().Be("ja");
            enus.TwoLetterISOLanguageName.Should().Be("en");
            en.TwoLetterISOLanguageName.Should().Be("en");
            invariant.TwoLetterISOLanguageName.Should().Be("iv");

            // 言語レベルでの同一性の判定にはTwoLetterISOLanguageNameを使うのが妥当？
            // MEMO: LCIDを使った比較は成立しないので注意。
            jajp.TwoLetterISOLanguageName.Should().Be(ja.TwoLetterISOLanguageName);
            enus.TwoLetterISOLanguageName.Should().Be(en.TwoLetterISOLanguageName);
        }
    }
}
