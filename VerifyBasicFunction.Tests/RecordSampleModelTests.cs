using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace VerifyBasicFunction.Tests
{
    /// <summary>
    /// Recordを.Net Framework 4.8で検証するためのテストクラス。
    /// </summary>
    public class RecordSampleModelTests
    {
        /// <summary>
        /// RecordとImmutableCollectionの不変性に関してテストする。
        /// </summary>
        [Fact]
        public void CreateAndUpdateRecordTest()
        {
            // Arrange
            var model = new RecordSampleModel(
                "Taro",
                20,
                new List<int> { 2010, 2014, 2018 },
                new List<int>() { 2022, 2020, 2024 }.ToImmutableList());

            // Assert
            model.Age.Should().Be(20);
            model.Name.Should().Be("Taro");
            model.GraduateYear.Should().BeEquivalentTo(new List<int> { 2010, 2014, 2018 });

            // initなプロパティを更新しようとするとエラーになる。
            // エラー CS8852  init 専用プロパティまたはインデクサー 'RecordSampleModel.Age' を割り当てることができるのは、
            // オブジェクト初期化子の中か、インスタンス コンストラクターまたは 'init' アクセサーの 'this' か 'base' 上のみです。
            // model.Age = 30;
            // model.GraduateYear = new List<int> { 2012, 2016, 2020 };
            // model.ImmutableGraduateYear = new List<int> { 2012, 2016, 2020 }.ToImmutableList();

            // ただし、List<T>の要素は更新できる。
            model.GraduateYear[1] = 2015;
            model.GraduateYear.Should().BeEquivalentTo(new List<int> { 2010, 2015, 2018 });

            // ImmutableList<T>の要素は更新できない。
            // エラー CS0200  プロパティまたはインデクサー 'ImmutableList<int>.this[int]' は読み取り専用であるため、割り当てることはできません
            // model.ImmutableGraduateYear[1] = 2023;

            // ImmutableList<T>の要素をソートして新しいImmutableList<T>を作成することはできる。
            // 元のImmutableList<T>は変更されない。
            // NuGetでSystem.Collections.Immutableパッケージを入れる必要がある。
            ImmutableList<int> temp = model.ImmutableGraduateYear.Sort();
            temp.SequenceEqual(new List<int> { 2020, 2022, 2024 }.ToImmutableList()).Should().BeTrue();
            temp.SequenceEqual(model.ImmutableGraduateYear).Should().BeFalse();
        }

        /// <summary>
        /// With式を用いた一部プロパティ更新のサンプル。
        /// </summary>
        [Fact]
        public void WithExpressionTest()
        {
            // With式はC#7.3では利用できないので間接的に呼び出してテストケースとする。
            var model = new RecordSampleModel(
                "Taro",
                20,
                new List<int> { 2010, 2014, 2018 },
                new List<int>() { 2022, 2020, 2024 }.ToImmutableList());

            var newInstance = RecordSampleModel.GetNewInstanceWith(model, 30);
            newInstance.Age.Should().Be(30); // With式でAgeプロパティが更新されていることを確認
            newInstance.Name.Should().Be("Taro"); // 他のプロパティは変更されていないことを確認
        }
    }
}
