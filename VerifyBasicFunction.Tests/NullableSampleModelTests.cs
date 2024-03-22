using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace VerifyBasicFunction.Tests
{
    /// <summary>
    /// .NET Standard 2.0プロジェクトを使う.Net Framework 4.8プロジェクト（このユニットテストプロジェクト）
    /// でNullableを使用するテスト
    /// </summary>
    public class NullableSampleModelTests
    {
        /// <summary>
        /// Nullableなライブラリのクラスを利用する際の動作検証。
        /// </summary>
        [Fact]
        public void UseNullableTest()
        {
            // ユニットテストプロジェクトはTargetFrameworkVersionがv4.8であるため
            // .Net Framework 4.8かつC#7.3でコンパイル＆動作している。
            // .NET Standard 2.0かつC#10.0のプロジェクトを参照していても、
            // Polyfillライブラリなどを適切に利用すれば影響はない。

            // CS0037	Null 非許容の値型であるため、Null を 'int' に変換できません
            //int normalNum = null;
            //normalNum.Should().BeNull();

            // OK
            int? nullableNum = null;
            nullableNum.Should().BeNull();

            // OK
            string normalStr = null;
            normalStr.Should().BeNull();

            // CS8370 機能 'Null 許容参照型' は C# 7.3 では使用できません。8.0 以上の言語バージョンをお使いください。
            //string? nullableStr = null;
            //nullableStr.Should().BeNull();

            // OK
            List<int> normalList = null;
            normalList.Should().BeNull();

            // CS8370 機能 'Null 許容参照型' は C# 7.3 では使用できません。8.0 以上の言語バージョンをお使いください。
            //List<int>? nullableList = null;
            //nullableList.Should().BeNull();
        }

        /// <summary>
        /// Nullableなライブラリのクラスを利用するテスト。
        /// </summary>
        [Fact]
        public void UseNullableLibraryTest()
        {
            // 一方、参照先のプロジェクトでNullableを使用することは問題ない。
            NullableSampleModel nullableModel = new NullableSampleModel();
            nullableModel.NormalNumber.Should().Be(0);
            nullableModel.NullableNumber.Should().BeNull();
            nullableModel.NormalString.Should().Be(string.Empty);
            nullableModel.NullableString.Should().BeNull();
            nullableModel.NormalList.Should().BeEmpty();
            nullableModel.NormalList.Should().NotBeNull();
            nullableModel.NullableList.Should().BeNull();

            // 明示的にint?へNullを代入。
            nullableModel.NullableNumber = null;
            nullableModel.NullableNumber.Should().BeNull();
            int? nullableNumber = nullableModel.NullableNumber;
            nullableNumber.Should().BeNull();

            // 明示的にintへ変換する。
            nullableModel.NullableNumber = 1;
            nullableModel.NullableNumber.Should().Be(1);
            nullableNumber = nullableModel.NullableNumber;
            nullableNumber.Should().Be(1);
            // エラーになる。
            //int num = nullableNumber;
            int num = nullableNumber ?? 0;
            num.Should().Be(1);

            // 明示的にstring?へNullを代入。
            nullableModel.NullableString = null;
            nullableModel.NullableString.Should().BeNull();
            // CS8370 機能 'Null 許容参照型' は C# 7.3 では使用できません。8.0 以上の言語バージョンをお使いください。
            //string? nullableString = nullableModel.NullableString;
            // C#のstringは値型ではなく組み込みの参照型という特性があるため、Nullable参照型を通常の参照型に変換することは可能。
            string str = nullableModel.NullableString ?? string.Empty;
            str.Should().Be(string.Empty);

            // 明示的にList<int>?へNullを代入。
            nullableModel.NullableList = null;
            nullableModel.NullableList.Should().BeNull();
            // CS8370 機能 'Null 許容参照型' は C# 7.3 では使用できません。8.0 以上の言語バージョンをお使いください。
            //List<int>? nullableList = nullableModel.NullableList;
            // C#のListはもちろん参照型なのでstringと同様にNullable参照型を通常の参照型に変換して使用することは可能。
            List<int> list = nullableModel.NullableList ?? new List<int>();
            list.Should().BeEmpty();
            list.Should().NotBeNull();
        }

        /// <summary>
        /// Nullableな値の計算を行う関数を呼び出すテスト。
        /// </summary>
        [Fact]
        public void NullSubstitutionTest()
        {
            Action act;

            // 関数の内部でNull参照例外が発生する。
            // MEMO: Nullableの本質は静的コード診断なので、警告を無視すればコンパイル可能であるし、
            // 実行時にNull参照例外を発生させることもできる（※発生させるべきではない）。
            act = () => NullableSampleModel.SubstituteNull();
            act.Should().Throw<NullReferenceException>();
        }
    }
}
