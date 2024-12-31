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
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using FluentAssertions;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Inflection.Tests
{
    public class InflectionResolverTests
    {
        private readonly ITestOutputHelper output;

        public InflectionResolverTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// 活用形Resolveのテスト。
        /// </summary>
        [Fact()]
        public void ResolveTest()
        {
            TokenElement token = new TokenElement(
                "笑わ",
                "ワラワ",
                "ワラワ",
                ImmutableList.Create("動詞", "自立"),
                "笑う",
                "五段・ワ行促音便",
                "未然形",
                ImmutableList.Create(new LineOffset(1, 0))
            );

            InflectionResolver.Resolve(token, "基本形").surface.Should().Be("笑う");
            InflectionResolver.Resolve(token, "未然形").surface.Should().Be("笑わ");
            InflectionResolver.Resolve(token, "未然ウ接続").surface.Should().Be("笑お");
            InflectionResolver.Resolve(token, "連用形").surface.Should().Be("笑い");
            InflectionResolver.Resolve(token, "連用タ接続").surface.Should().Be("笑っ");
            InflectionResolver.Resolve(token, "仮定形").surface.Should().Be("笑え");
            InflectionResolver.Resolve(token, "命令ｅ").surface.Should().Be("笑え");

            (bool success, string surface) result = InflectionResolver.Resolve(token, "存在しない活用形");
            result.success.Should().BeFalse();

            // TokenElement { S= "笑う",	R= "ワラウ(ワラウ)",	P= [動詞-自立],	I= 基本形/五段・ワ行促音便,	B= ,	O= (L1,0)-(L1,1)}
            var token2 = KuromojiController.Tokenize(new Sentence("笑うらしい", 1)).FirstOrDefault();
            InflectionResolver.Resolve(token2, "基本形").surface.Should().Be("笑う");
            InflectionResolver.Resolve(token2, "未然形").surface.Should().Be("笑わ");
            InflectionResolver.Resolve(token2, "未然ウ接続").surface.Should().Be("笑お");
            InflectionResolver.Resolve(token2, "連用形").surface.Should().Be("笑い");
            InflectionResolver.Resolve(token2, "連用タ接続").surface.Should().Be("笑っ");
            InflectionResolver.Resolve(token2, "仮定形").surface.Should().Be("笑え");
            InflectionResolver.Resolve(token2, "命令ｅ").surface.Should().Be("笑え");
        }

        [Theory]
        // 10_完了.csv
        // 現在時制
        [InlineData("001", "::動詞:連用タ接続:五段・ワ行促音便", "笑った")]

        // 過去時制
        [InlineData("002", "::動詞:連用形:五段・ワ行促音便", "笑いました")]

        // 13_推定.csv
        // 現在時制
        [InlineData("003", "::動詞:基本形:五段・ワ行促音便", "笑うらしい")]
        [InlineData("004", "::動詞:基本形:五段・ワ行促音便", "笑うらしかった")]

        // 過去時制
        [InlineData("005", "::動詞:基本形:五段・ワ行促音便", "笑うらしいです")]
        [InlineData("006", "::動詞:基本形:五段・ワ行促音便", "笑うらしかったです")]

        // 14_断定.csv
        // 現在時制
        //[InlineData("007", "::動詞:名詞:五段・ワ行促音便", "笑いだ")]
        //[InlineData("008", "::動詞:名詞:五段・ワ行促音便", "笑いだった")]
        //[InlineData("009", "::動詞:名詞:五段・ワ行促音便", "笑いである")]
        //[InlineData("010", "::動詞:名詞:五段・ワ行促音便", "笑いであった")]
        [InlineData("007", "::名詞", "笑いだ")]
        [InlineData("008", "::名詞", "笑いだった")]
        [InlineData("009", "::名詞", "笑いである")]
        [InlineData("010", "::名詞", "笑いであった")]

        // 過去時制
        //[InlineData("011", "::動詞:名詞:五段・ワ行促音便", "笑いです")]
        //[InlineData("012", "::動詞:名詞:五段・ワ行促音便", "笑いでした")]
        //[InlineData("013", "::動詞:名詞:五段・ワ行促音便", "笑いです")]
        //[InlineData("014", "::動詞:名詞:五段・ワ行促音便", "笑いでした")]
        [InlineData("011", "::名詞", "笑いです")]
        [InlineData("012", "::名詞", "笑いでした")]
        [InlineData("013", "::名詞", "笑いです")]
        [InlineData("014", "::名詞", "笑いでした")]

        // 3_受け身可能自発尊敬.csv
        // 現在時制
        [InlineData("015", "::動詞:未然形:五段・ワ行促音便", "笑われる")]
        [InlineData("016", "::動詞:未然形:五段・ワ行促音便", "笑われた")]

        // 過去時制
        [InlineData("017", "::動詞:未然形:五段・ワ行促音便", "笑われます")]
        [InlineData("018", "::動詞:未然形:五段・ワ行促音便", "笑われました")]

        // 4_使役.csv
        // 現在時制
        [InlineData("019", "::動詞:基本形:一段", "笑わせる")]
        [InlineData("020", "::動詞:連用形:一段", "笑わせた")]

        // 過去時制
        [InlineData("021", "::動詞:連用形:一段", "笑わせます")]
        [InlineData("022", "::動詞:連用形:一段", "笑わせました")]

        // 5_打ち消し.csv
        // 現在時制
        [InlineData("023", "::動詞:未然形:カ変・クル", "こない")]
        [InlineData("024", "::動詞:未然形:カ変・クル", "こなかった")]
        [InlineData("027", "::動詞:未然形:カ変・来ル", "来ない")]
        [InlineData("028", "::動詞:未然形:カ変・来ル", "来なかった")]
        [InlineData("031", "::動詞:未然形:サ変・スル", "しない")]
        [InlineData("032", "::動詞:未然形:サ変・スル", "しなかった")]

        //[InlineData("035", "::動詞:未然形:サ変・－スル", "察しない")]
        //[InlineData("036", "::動詞:未然形:サ変・－スル", "察しなかった")]
        //[InlineData("039", "::動詞:未然形:サ変・－ズル", "生じない")]
        //[InlineData("040", "::動詞:未然形:サ変・－ズル", "生じなかった")]
        [InlineData("035", "::名詞", "察しない")]
        [InlineData("036", "::名詞", "察しなかった")]
        [InlineData("039", "::動詞:未然形:一段", "生じない")]
        [InlineData("040", "::動詞:未然形:一段", "生じなかった")]
        [InlineData("043", "::動詞:未然形:一段", "受けない")]
        [InlineData("044", "::動詞:未然形:一段", "受けなかった")]
        [InlineData("047", "::動詞:未然形:一段・クレル", "呉れない")]
        [InlineData("048", "::動詞:未然形:一段・クレル", "呉れなかった")]

        //[InlineData("051", "::動詞:未然形:下二・カ行", "助けない")]
        //[InlineData("052", "::動詞:未然形:下二・カ行", "助けなかった")]
        //[InlineData("055", "::動詞:未然形:下二・ガ行", "見上げない")]
        //[InlineData("056", "::動詞:未然形:下二・ガ行", "見上げなかった")]
        [InlineData("051", "::動詞:未然形:一段", "助けない")]
        [InlineData("052", "::動詞:未然形:一段", "助けなかった")]
        [InlineData("055", "::動詞:未然形:一段", "見上げない")]
        [InlineData("056", "::動詞:未然形:一段", "見上げなかった")]
        [InlineData("059", "::動詞:未然形:下二・ダ行", "いでない")]
        [InlineData("060", "::動詞:未然形:下二・ダ行", "いでなかった")]
        [InlineData("063", "::動詞:未然形:下二・ハ行", "憂へない")]
        [InlineData("064", "::動詞:未然形:下二・ハ行", "憂へなかった")]

        //[InlineData("067", "::動詞:未然形:下二・マ行", "改めない")]
        //[InlineData("068", "::動詞:未然形:下二・マ行", "改めなかった")]
        //[InlineData("071", "::動詞:未然形:下二・得", "得ない")]
        //[InlineData("072", "::動詞:未然形:下二・得", "得なかった")]
        [InlineData("067", "::動詞:未然形:一段", "改めない")]
        [InlineData("068", "::動詞:未然形:一段", "改めなかった")]
        [InlineData("071", "::動詞:未然形:一段", "得ない")]
        [InlineData("072", "::動詞:未然形:一段", "得なかった")]
        [InlineData("075", "::動詞:未然形:五段・ガ行", "防がない")]
        [InlineData("076", "::動詞:未然形:五段・ガ行", "防がなかった")]
        [InlineData("079", "::動詞:未然形:五段・カ行イ音便", "履かない")]
        [InlineData("080", "::動詞:未然形:五段・カ行イ音便", "履かなかった")]
        [InlineData("083", "::動詞:未然形:五段・カ行促音便", "行かない")]
        [InlineData("084", "::動詞:未然形:五段・カ行促音便", "行かなかった")]
        [InlineData("087", "::動詞:未然形:五段・カ行促音便ユク", "過ぎ行かない")]
        [InlineData("088", "::動詞:未然形:五段・カ行促音便ユク", "過ぎ行かなかった")]
        [InlineData("091", "::動詞:未然形:五段・サ行", "冷やさない")]
        [InlineData("092", "::動詞:未然形:五段・サ行", "冷やさなかった")]
        [InlineData("095", "::動詞:未然形:五段・タ行", "打たない")]
        [InlineData("096", "::動詞:未然形:五段・タ行", "打たなかった")]
        [InlineData("099", "::動詞:未然形:五段・ナ行", "死なない")]
        [InlineData("100", "::動詞:未然形:五段・ナ行", "死ななかった")]
        [InlineData("103", "::動詞:未然形:五段・バ行", "呼ばない")]
        [InlineData("104", "::動詞:未然形:五段・バ行", "呼ばなかった")]
        [InlineData("107", "::動詞:未然形:五段・マ行", "取り込まない")]
        [InlineData("108", "::動詞:未然形:五段・マ行", "取り込まなかった")]
        [InlineData("111", "::動詞:未然形:五段・ラ行", "見送らない")]
        [InlineData("112", "::動詞:未然形:五段・ラ行", "見送らなかった")]
        [InlineData("115", "::動詞:未然形:五段・ラ行特殊", "下さらない")]
        [InlineData("116", "::動詞:未然形:五段・ラ行特殊", "下さらなかった")]

        //[InlineData("119", "::動詞:未然形:五段・ワ行ウ音便", "言わない")]
        //[InlineData("120", "::動詞:未然形:五段・ワ行ウ音便", "言わなかった")]
        [InlineData("119", "::動詞:未然形:五段・ワ行促音便", "言わない")]
        [InlineData("120", "::動詞:未然形:五段・ワ行促音便", "言わなかった")]
        [InlineData("123", "::動詞:未然形:五段・ワ行促音便", "笑わない")]
        [InlineData("124", "::動詞:未然形:五段・ワ行促音便", "笑わなかった")]
        [InlineData("127", "::動詞:未然形:四段・サ行", "天てらさない")]
        [InlineData("128", "::動詞:未然形:四段・サ行", "天てらさなかった")]
        [InlineData("131", "::動詞:未然形:四段・タ行", "群だたない")]
        [InlineData("132", "::動詞:未然形:四段・タ行", "群だたなかった")]
        [InlineData("135", "::動詞:未然形:四段・ハ行", "思はない")]
        [InlineData("136", "::動詞:未然形:四段・ハ行", "思はなかった")]
        [InlineData("139", "::動詞:未然形:四段・バ行", "たばない")]
        [InlineData("140", "::動詞:未然形:四段・バ行", "たばなかった")]
        [InlineData("143", "::動詞:未然形:上二・ダ行", "恥ぢない")]
        [InlineData("144", "::動詞:未然形:上二・ダ行", "恥ぢなかった")]
        [InlineData("147", "::動詞:未然形:上二・ハ行", "憂ひない")]
        [InlineData("148", "::動詞:未然形:上二・ハ行", "憂ひなかった")]

        //[InlineData("151", "::動詞:未然形:検証用追加1", "得ない")]
        //[InlineData("152", "::動詞:未然形:検証用追加1", "得なかった")]
        //[InlineData("155", "::動詞:未然形:検証用追加2", "ない")]
        //[InlineData("156", "::動詞:未然形:検証用追加2", "なかった")]
        [InlineData("151", "::動詞:未然形:一段", "得ない")]
        [InlineData("152", "::動詞:未然形:一段", "得なかった")]
        [InlineData("155", "::形容詞:基本形:形容詞・アウオ段", "ない")]
        [InlineData("156", "::形容詞:連用タ接続:形容詞・アウオ段", "なかった")]

        // 過去時制
        [InlineData("025", "::動詞:連用形:カ変・クル", "きません")]
        [InlineData("026", "::動詞:連用形:カ変・クル", "きませんでした")]
        [InlineData("029", "::動詞:連用形:カ変・来ル", "来ません")]
        [InlineData("030", "::動詞:連用形:カ変・来ル", "来ませんでした")]
        [InlineData("033", "::動詞:連用形:サ変・スル", "しません")]
        [InlineData("034", "::動詞:連用形:サ変・スル", "しませんでした")]

        //[InlineData("037", "::動詞:連用形:サ変・－スル", "察しません")]
        //[InlineData("038", "::動詞:連用形:サ変・－スル", "察しませんでした")]
        //[InlineData("041", "::動詞:連用形:サ変・－ズル", "生じません")]
        //[InlineData("042", "::動詞:連用形:サ変・－ズル", "生じませんでした")]
        [InlineData("037", "::動詞:連用形:五段・サ変", "察しません")]
        [InlineData("038", "::動詞:連用形:五段・サ変", "察しませんでした")]
        [InlineData("041", "::動詞:連用形:一段", "生じません")]
        [InlineData("042", "::動詞:連用形:一段", "生じませんでした")]
        [InlineData("045", "::動詞:連用形:一段", "受けません")]
        [InlineData("046", "::動詞:連用形:一段", "受けませんでした")]
        [InlineData("049", "::動詞:連用形:一段・クレル", "呉れません")]
        [InlineData("050", "::動詞:連用形:一段・クレル", "呉れませんでした")]

        //[InlineData("053", "::動詞:連用形:下二・カ行", "助けません")]
        //[InlineData("054", "::動詞:連用形:下二・カ行", "助けませんでした")]
        //[InlineData("057", "::動詞:連用形:下二・ガ行", "見上げません")]
        //[InlineData("058", "::動詞:連用形:下二・ガ行", "見上げませんでした")]
        [InlineData("053", "::動詞:連用形:一段", "助けません")]
        [InlineData("054", "::動詞:連用形:一段", "助けませんでした")]
        [InlineData("057", "::動詞:連用形:一段", "見上げません")]
        [InlineData("058", "::動詞:連用形:一段", "見上げませんでした")]
        [InlineData("061", "::動詞:連用形:下二・ダ行", "いでません")]
        [InlineData("062", "::動詞:連用形:下二・ダ行", "いでませんでした")]
        [InlineData("065", "::動詞:連用形:下二・ハ行", "憂へません")]
        [InlineData("066", "::動詞:連用形:下二・ハ行", "憂へませんでした")]

        //[InlineData("069", "::動詞:連用形:下二・マ行", "改めません")]
        //[InlineData("070", "::動詞:連用形:下二・マ行", "改めませんでした")]
        //[InlineData("073", "::動詞:連用形:下二・得", "得ません")]
        //[InlineData("074", "::動詞:連用形:下二・得", "得ませんでした")]
        [InlineData("069", "::動詞:連用形:一段", "改めません")]
        [InlineData("070", "::動詞:連用形:一段", "改めませんでした")]
        [InlineData("073", "::動詞:連用形:一段", "得ません")]
        [InlineData("074", "::動詞:連用形:一段", "得ませんでした")]
        [InlineData("077", "::動詞:連用形:五段・ガ行", "防ぎません")]
        [InlineData("078", "::動詞:連用形:五段・ガ行", "防ぎませんでした")]
        [InlineData("081", "::動詞:連用形:五段・カ行イ音便", "履きません")]
        [InlineData("082", "::動詞:連用形:五段・カ行イ音便", "履きませんでした")]
        [InlineData("085", "::動詞:連用形:五段・カ行促音便", "行きません")]
        [InlineData("086", "::動詞:連用形:五段・カ行促音便", "行きませんでした")]
        [InlineData("089", "::動詞:連用形:五段・カ行促音便ユク", "過ぎ行きません")]
        [InlineData("090", "::動詞:連用形:五段・カ行促音便ユク", "過ぎ行きませんでした")]
        [InlineData("093", "::動詞:連用形:五段・サ行", "冷やしません")]
        [InlineData("094", "::動詞:連用形:五段・サ行", "冷やしませんでした")]
        [InlineData("097", "::動詞:連用形:五段・タ行", "打ちません")]
        [InlineData("098", "::動詞:連用形:五段・タ行", "打ちませんでした")]
        [InlineData("101", "::動詞:連用形:五段・ナ行", "死にません")]
        [InlineData("102", "::動詞:連用形:五段・ナ行", "死にませんでした")]
        [InlineData("105", "::動詞:連用形:五段・バ行", "呼びません")]
        [InlineData("106", "::動詞:連用形:五段・バ行", "呼びませんでした")]
        [InlineData("109", "::動詞:連用形:五段・マ行", "取り込みません")]
        [InlineData("110", "::動詞:連用形:五段・マ行", "取り込みませんでした")]
        [InlineData("113", "::動詞:連用形:五段・ラ行", "見送りません")]
        [InlineData("114", "::動詞:連用形:五段・ラ行", "見送りませんでした")]
        [InlineData("117", "::動詞:連用形:五段・ラ行特殊", "下さいません")]
        [InlineData("118", "::動詞:連用形:五段・ラ行特殊", "下さいませんでした")]

        //[InlineData("121", "::動詞:連用形:五段・ワ行ウ音便", "言いません")]
        //[InlineData("122", "::動詞:連用形:五段・ワ行ウ音便", "言いませんでした")]
        [InlineData("121", "::動詞:連用形:五段・ワ行促音便", "言いません")]
        [InlineData("122", "::動詞:連用形:五段・ワ行促音便", "言いませんでした")]
        [InlineData("125", "::動詞:連用形:五段・ワ行促音便", "笑いません")]
        [InlineData("126", "::動詞:連用形:五段・ワ行促音便", "笑いませんでした")]
        [InlineData("129", "::動詞:連用形:四段・サ行", "天てらしません")]
        [InlineData("130", "::動詞:連用形:四段・サ行", "天てらしませんでした")]
        [InlineData("133", "::動詞:連用形:四段・タ行", "群だちません")]
        [InlineData("134", "::動詞:連用形:四段・タ行", "群だちませんでした")]
        [InlineData("137", "::動詞:連用形:四段・ハ行", "思ひません")]
        [InlineData("138", "::動詞:連用形:四段・ハ行", "思ひませんでした")]
        [InlineData("141", "::動詞:連用形:四段・バ行", "たびません")]
        [InlineData("142", "::動詞:連用形:四段・バ行", "たびませんでした")]
        [InlineData("145", "::動詞:連用形:上二・ダ行", "恥ぢません")]
        [InlineData("146", "::動詞:連用形:上二・ダ行", "恥ぢませんでした")]
        [InlineData("149", "::動詞:連用形:上二・ハ行", "憂ひません")]
        [InlineData("150", "::動詞:連用形:上二・ハ行", "憂ひませんでした")]

        //[InlineData("153", "::動詞:連用形:検証用追加1", "得ません")]
        //[InlineData("154", "::動詞:連用形:検証用追加1", "得ませんでした")]
        //[InlineData("157", "::動詞:連用形:検証用追加2", "ありません")]
        //[InlineData("158", "::動詞:連用形:検証用追加2", "ありませんでした")]
        [InlineData("153", "::動詞:連用形:一段", "得ません")]
        [InlineData("154", "::動詞:連用形:一段", "得ませんでした")]
        [InlineData("157", "::動詞:連用形:五段・ラ行", "ありません")]
        [InlineData("158", "::動詞:連用形:五段・ラ行", "ありませんでした")]

        // 6_意思推量.csv
        // 現在時制
        //[InlineData("159", "::動詞:基本形:カ変・クル", "くる")]
        [InlineData("159", "::動詞:基本形:五段・ラ行", "くる")]
        [InlineData("160", "::動詞:未然ウ接続:カ変・クル", "こよう")]
        [InlineData("163", "::動詞:基本形:カ変・来ル", "来る")]
        [InlineData("164", "::動詞:未然ウ接続:カ変・来ル", "来よう")]

        //[InlineData("167", "::動詞:基本形:サ変・スル", "する")]
        [InlineData("167", "::動詞:基本形:五段・ラ行", "する")]
        [InlineData("168", "::動詞:未然ウ接続:サ変・スル", "しょう")]

        //[InlineData("171", "::動詞:基本形:サ変・－スル", "察する")]
        //[InlineData("172", "::動詞:未然ウ接続:サ変・－スル", "察しょう")]
        //[InlineData("175", "::動詞:基本形:サ変・－ズル", "生ずる")]
        //[InlineData("176", "::動詞:未然ウ接続:サ変・－ズル", "生ぜよう")]
        [InlineData("171", "::動詞:基本形:サ変・−スル", "察する")]
        [InlineData("172", "::名詞", "察しょう")]
        [InlineData("175", "::動詞:基本形:サ変・−ズル", "生ずる")]
        [InlineData("176", "::動詞:未然ウ接続:サ変・−ズル", "生ぜよう")]
        [InlineData("179", "::動詞:基本形:一段", "受ける")]
        [InlineData("180", "::動詞:未然ウ接続:一段", "受けよう")]
        [InlineData("183", "::動詞:基本形:一段・クレル", "呉れる")]
        [InlineData("184", "::動詞:未然ウ接続:一段・クレル", "呉れよう")]

        //[InlineData("187", "::動詞:基本形:下二・ダ行", "いでる")]
        //[InlineData("188", "::動詞:未然ウ接続:下二・ダ行", "いでよう")]
        //[InlineData("191", "::動詞:基本形:下二・ハ行", "憂へる")]
        //[InlineData("192", "::動詞:未然ウ接続:下二・ハ行", "憂へよう")]

        [InlineData("187", "::動詞:連用形:一段", "いでる")]
        [InlineData("188", "::動詞:連用形:一段", "いでよう")]
        [InlineData("191", "::動詞:連用形:下二・ハ行", "憂へる")]
        [InlineData("192", "::動詞:連用形:下二・ハ行", "憂へよう")]
        [InlineData("195", "::動詞:基本形:五段・ガ行", "防ぐ")]
        [InlineData("196", "::動詞:未然ウ接続:五段・ガ行", "防ごう")]
        [InlineData("199", "::動詞:基本形:五段・カ行イ音便", "履く")]
        [InlineData("200", "::動詞:未然ウ接続:五段・カ行イ音便", "履こう")]
        [InlineData("203", "::動詞:基本形:五段・カ行促音便", "行く")]
        [InlineData("204", "::動詞:未然ウ接続:五段・カ行促音便", "行こう")]
        [InlineData("207", "::動詞:基本形:五段・カ行促音便ユク", "過ぎ行く")]
        [InlineData("208", "::動詞:未然ウ接続:五段・カ行促音便ユク", "過ぎ行こう")]
        [InlineData("211", "::動詞:基本形:五段・サ行", "冷やす")]
        [InlineData("212", "::動詞:未然ウ接続:五段・サ行", "冷やそう")]
        [InlineData("215", "::動詞:基本形:五段・タ行", "打つ")]
        [InlineData("216", "::動詞:未然ウ接続:五段・タ行", "打とう")]
        [InlineData("219", "::動詞:基本形:五段・ナ行", "死ぬ")]
        [InlineData("220", "::動詞:未然ウ接続:五段・ナ行", "死のう")]
        [InlineData("223", "::動詞:基本形:五段・バ行", "呼ぶ")]
        [InlineData("224", "::動詞:未然ウ接続:五段・バ行", "呼ぼう")]
        [InlineData("227", "::動詞:基本形:五段・マ行", "取り込む")]
        [InlineData("228", "::動詞:未然ウ接続:五段・マ行", "取り込もう")]
        [InlineData("231", "::動詞:基本形:五段・ラ行", "見送る")]
        [InlineData("232", "::動詞:未然ウ接続:五段・ラ行", "見送ろう")]
        [InlineData("235", "::動詞:基本形:五段・ラ行特殊", "下さる")]
        [InlineData("236", "::動詞:未然ウ接続:五段・ラ行特殊", "下さろう")]

        //[InlineData("239", "::動詞:基本形:五段・ワ行ウ音便", "言う")]
        //[InlineData("240", "::動詞:未然ウ接続:五段・ワ行ウ音便", "言おう")]

        [InlineData("239", "::動詞:基本形:五段・ワ行促音便", "言う")]
        [InlineData("240", "::動詞:未然ウ接続:五段・ワ行促音便", "言おう")]
        [InlineData("243", "::動詞:基本形:五段・ワ行促音便", "笑う")]
        [InlineData("244", "::動詞:未然ウ接続:五段・ワ行促音便", "笑おう")]
        [InlineData("247", "::動詞:基本形:四段・サ行", "天てらす")]

        //[InlineData("248", "::動詞:未然ウ接続:四段・サ行", "天てらそう")]
        [InlineData("248", "::名詞", "天てらそう")]
        [InlineData("251", "::動詞:基本形:四段・タ行", "群だつ")]
        [InlineData("254", "::動詞:基本形:四段・ハ行", "思ふ")]
        [InlineData("257", "::動詞:基本形:四段・バ行", "たぶ")]
        [InlineData("260", "::動詞:基本形:上二・ダ行", "恥づ")]

        //[InlineData("263", "::動詞:基本形:上二・ハ行", "憂ふ")]
        [InlineData("263", "::動詞:基本形:下二・ハ行", "憂ふ")]
        [InlineData("266", "::動詞:基本形:一段", "助ける")]
        [InlineData("267", "::動詞:未然ウ接続:一段", "助けよう")]
        [InlineData("270", "::動詞:基本形:一段", "見上げる")]
        [InlineData("271", "::動詞:未然ウ接続:一段", "見上げよう")]
        [InlineData("274", "::動詞:基本形:一段", "改める")]
        [InlineData("275", "::動詞:未然ウ接続:一段", "改めよう")]
        [InlineData("278", "::動詞:基本形:一段", "得る")]
        [InlineData("279", "::動詞:未然ウ接続:一段", "得よう")]

        // 過去時制
        [InlineData("161", "::動詞:連用形:カ変・クル", "きます")]
        [InlineData("162", "::動詞:連用形:カ変・クル", "きましょう")]
        [InlineData("165", "::動詞:連用形:カ変・来ル", "来ます")]
        [InlineData("166", "::動詞:連用形:カ変・来ル", "来ましょう")]
        [InlineData("169", "::動詞:連用形:サ変・スル", "します")]
        [InlineData("170", "::動詞:連用形:サ変・スル", "しましょう")]

        //[InlineData("173", "::動詞:連用形:サ変・－スル", "察します")]
        //[InlineData("174", "::動詞:連用形:サ変・－スル", "察しましょう")]
        //[InlineData("177", "::動詞:連用形:サ変・－ズル", "生じます")]
        //[InlineData("178", "::動詞:連用形:サ変・－ズル", "生じましょう")]

        [InlineData("173", "::動詞:連用形:五段・サ行", "察します")]
        [InlineData("174", "::動詞:連用形:五段・サ行", "察しましょう")]
        [InlineData("177", "::動詞:連用形:一段", "生じます")]
        [InlineData("178", "::動詞:連用形:一段", "生じましょう")]
        [InlineData("181", "::動詞:連用形:一段", "受けます")]
        [InlineData("182", "::動詞:連用形:一段", "受けましょう")]
        [InlineData("185", "::動詞:連用形:一段・クレル", "呉れます")]
        [InlineData("186", "::動詞:連用形:一段・クレル", "呉れましょう")]
        [InlineData("189", "::動詞:連用形:下二・ダ行", "いでます")]
        [InlineData("190", "::動詞:連用形:下二・ダ行", "いでましょう")]
        [InlineData("193", "::動詞:連用形:下二・ハ行", "憂へます")]
        [InlineData("194", "::動詞:連用形:下二・ハ行", "憂へましょう")]
        [InlineData("197", "::動詞:連用形:五段・ガ行", "防ぎます")]
        [InlineData("198", "::動詞:連用形:五段・ガ行", "防ぎましょう")]
        [InlineData("201", "::動詞:連用形:五段・カ行イ音便", "履きます")]
        [InlineData("202", "::動詞:連用形:五段・カ行イ音便", "履きましょう")]
        [InlineData("205", "::動詞:連用形:五段・カ行促音便", "行きます")]
        [InlineData("206", "::動詞:連用形:五段・カ行促音便", "行きましょう")]
        [InlineData("209", "::動詞:連用形:五段・カ行促音便ユク", "過ぎ行きます")]
        [InlineData("210", "::動詞:連用形:五段・カ行促音便ユク", "過ぎ行きましょう")]
        [InlineData("213", "::動詞:連用形:五段・サ行", "冷やします")]
        [InlineData("214", "::動詞:連用形:五段・サ行", "冷やしましょう")]
        [InlineData("217", "::動詞:連用形:五段・タ行", "打ちます")]
        [InlineData("218", "::動詞:連用形:五段・タ行", "打ちましょう")]
        [InlineData("221", "::動詞:連用形:五段・ナ行", "死にます")]
        [InlineData("222", "::動詞:連用形:五段・ナ行", "死にましょう")]
        [InlineData("225", "::動詞:連用形:五段・バ行", "呼びます")]
        [InlineData("226", "::動詞:連用形:五段・バ行", "呼びましょう")]
        [InlineData("229", "::動詞:連用形:五段・マ行", "取り込みます")]
        [InlineData("230", "::動詞:連用形:五段・マ行", "取り込みましょう")]
        [InlineData("233", "::動詞:連用形:五段・ラ行", "見送ります")]
        [InlineData("234", "::動詞:連用形:五段・ラ行", "見送りましょう")]
        [InlineData("237", "::動詞:連用形:五段・ラ行特殊", "下さいます")]
        [InlineData("238", "::動詞:連用形:五段・ラ行特殊", "下さいましょう")]

        //[InlineData("241", "::動詞:連用形:五段・ワ行ウ音便", "言います")]
        //[InlineData("242", "::動詞:連用形:五段・ワ行ウ音便", "言いましょう")]
        [InlineData("241", "::動詞:連用形:五段・ワ行促音便", "言います")]
        [InlineData("242", "::動詞:連用形:五段・ワ行促音便", "言いましょう")]
        [InlineData("245", "::動詞:連用形:五段・ワ行促音便", "笑います")]
        [InlineData("246", "::動詞:連用形:五段・ワ行促音便", "笑いましょう")]
        [InlineData("249", "::動詞:連用形:四段・サ行", "天てらします")]
        [InlineData("250", "::動詞:連用形:四段・サ行", "天てらしましょう")]
        [InlineData("252", "::動詞:連用形:四段・タ行", "群だちます")]
        [InlineData("253", "::動詞:連用形:四段・タ行", "群だちましょう")]
        [InlineData("255", "::動詞:連用形:四段・ハ行", "思ひます")]
        [InlineData("256", "::動詞:連用形:四段・ハ行", "思ひましょう")]
        [InlineData("258", "::動詞:連用形:四段・バ行", "たびます")]
        [InlineData("259", "::動詞:連用形:四段・バ行", "たびましょう")]
        [InlineData("261", "::動詞:連用形:上二・ダ行", "恥ぢます")]
        [InlineData("262", "::動詞:連用形:上二・ダ行", "恥ぢましょう")]
        [InlineData("264", "::動詞:連用形:上二・ハ行", "憂ひます")]
        [InlineData("265", "::動詞:連用形:上二・ハ行", "憂ひましょう")]
        [InlineData("268", "::動詞:連用形:一段", "助けます")]
        [InlineData("269", "::動詞:連用形:一段", "助けましょう")]
        [InlineData("272", "::動詞:連用形:一段", "見上げます")]
        [InlineData("273", "::動詞:連用形:一段", "見上げましょう")]
        [InlineData("276", "::動詞:連用形:一段", "改めます")]
        [InlineData("277", "::動詞:連用形:一段", "改めましょう")]
        [InlineData("280", "::動詞:連用形:一段", "得ます")]
        [InlineData("281", "::動詞:連用形:一段", "得ましょう")]

        // 8_希望.csv
        // 現在時制
        [InlineData("282", "::動詞:連用形:五段・ワ行促音便", "笑いたい")]
        [InlineData("283", "::動詞:連用形:五段・ワ行促音便", "笑いたかった")]
        [InlineData("284", "::動詞:連用形:五段・ワ行促音便", "笑いたがる")]
        [InlineData("285", "::動詞:連用形:五段・ワ行促音便", "笑いたがった")]

        // 過去時制
        [InlineData("286", "::動詞:連用形:五段・ワ行促音便", "笑いたいです")]
        [InlineData("287", "::動詞:連用形:五段・ワ行促音便", "笑いたかったです")]
        [InlineData("288", "::動詞:連用形:五段・ワ行促音便", "笑いたがります")]
        [InlineData("289", "::動詞:連用形:五段・ワ行促音便", "笑いたがりました")]

        // 可能.csv
        // 現在時制
        [InlineData("290", "::動詞:基本形:一段", "笑える")]
        [InlineData("291", "::動詞:連用形:一段", "笑えた")]

        // 過去時制
        [InlineData("292", "::動詞:連用形:一段", "笑えます")]
        [InlineData("293", "::動詞:連用形:一段", "笑えました")]

        // 命令.csv
        // 現在時制
        //[InlineData("294", "::動詞:命令ｅ:五段・ワ行促音便", "笑え")]
        //[InlineData("295", "::動詞:命令ｙｏ:五段・ワ行促音便", "笑えよ")]
        [InlineData("294", "::動詞:連用形:一段", "笑え")]
        [InlineData("295", "::動詞:命令ｅ:五段・ワ行促音便", "笑えよ")]

        // 過去時制
        [InlineData("296", "::動詞:連用形:五段・ワ行促音便", "笑いなさい")]
        [InlineData("297", "::動詞:連用形:五段・ワ行促音便", "笑いなさいよ")]
        public void 動詞用法のテスト(string nouse, string rule, string text)
        {
            var tokenElements = KuromojiController.Tokenize(new Sentence(text, 1));

            // 目視
            output.WriteLine("★Token...");
            foreach (var token in tokenElements)
            {
                output.WriteLine(token.ToString());
            }
            output.WriteLine("");

            output.WriteLine("★As Rule...");
            foreach (var token in tokenElements)
            {
                output.WriteLine(token.ToGrammarRuleString());
            }
            output.WriteLine("");

            //GrammarRuleExtractor.Run(rule).Pattern[0].Token.MatchAll(tokenElements[0]);
        }

        /// <summary>
        /// 活用変化を目視確認するためのデータセットテストケース。
        /// </summary>
        [Fact]
        public void WatchTokenizedString()
        {
            string text = @"
笑わない
笑おう
笑う
笑いだ
笑いである
笑うとき
笑えば
笑え
笑えよ
笑われる
笑える
笑わせる
笑わさせる
笑わぬ
笑わん
笑おう
笑うまい
笑いたい
笑いたがる
笑いそうだ
笑うようだ
笑うらしい
笑っている
笑わなかった
笑った
笑いだった
笑いであった
笑ったとき
笑われた
笑えた
笑わせた
笑わさせた
笑いたかった
笑いたがった
笑いそうだった
笑うようだった
笑うらしかった
笑っていた
笑いません
笑いましょう
笑います
笑いです
笑いです
ｘ
ｘ
笑いなさい
笑いなさいよ
笑われます
笑えます
笑わせます
笑わさせます
笑いませぬ
笑いません
笑いましょう
笑いますまい
笑いたいです
笑いたがります
笑いそうです
笑うようです
笑うらしいです
笑っています
笑いませんでした
笑いました
笑いでした
笑いでした
ｘ
笑われました
笑えました
笑わせました
笑わさせました
笑いたかったです
笑いたがりました
笑いそうでした
笑うようでした
笑うらしかったです
笑っていました
";

            // 1行ごとにSentenceに変換する。
            var sentences = text.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            ).Select((item, index) => new Sentence(item, index));

            List<List<TokenElement>> listlist = sentences.Select(i => KuromojiController.Tokenize(i)).ToList();

            foreach (var tokens in listlist)
            {
                // 元の文
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Join("", tokens.Select(i => i.Surface)));
                sb.Append('\t');

                // 品詞情報
                List<string> tokenString = new List<string>();
                foreach (var token in tokens)
                {
                    tokenString.Add($"{token.Surface}({token.Reading})/{token.BaseForm}[{token.PartOfSpeech[0]}]{token.InflectionType}/{token.InflectionForm}");
                }

                sb.Append(string.Join(" - ", tokenString));
                sb.Append('\t');

                // Surfaceのみの区切り表示
                sb.Append(string.Join(" - ", tokens.Select(i => i.Surface)));
                sb.Append('\t');

                // Surfaceのみの区切り表示の逆順（活用語尾で確認したいため）
                sb.Append(string.Join(" - ", tokens.Select(i => i.Surface).Reverse()));

                output.WriteLine(sb.ToString());
            }
        }
    }
}
