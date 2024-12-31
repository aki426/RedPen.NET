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

using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using FluentAssertions;
using RedPen.Net.Core.Grammar;
using RedPen.Net.Core.Inflection;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.CoreTests.NewFeatureResearch
{
    public class 常体敬体変換総引き当て表テスト
    {
        private readonly ITestOutputHelper output;

        public 常体敬体変換総引き当て表テスト(ITestOutputHelper output)
        {
            this.output = output;
        }

        public class TestData
        {
            private string _no;

            public string No
            {
                get
                {
                    if (int.TryParse(_no, out int number))
                    {
                        return number.ToString("D4");
                    }
                    return _no;  // 数値に変換できない場合は元の値を返す
                }
                set
                {
                    _no = value;
                }
            }

            public string 変換 { get; set; }
            public string 用法 { get; set; }

            public string 常体活用型 { get; set; }
            public string 常体基本形 { get; set; }

            public string 常体 { get; set; }
            public string 常体語尾 { get; set; }
            public string 常体活用形 { get; set; }
            public string 常体ルール { get; set; }

            public string 敬体活用型 { get; set; }
            public string 敬体基本形 { get; set; }

            public string 敬体 { get; set; }
            public string 敬体語尾 { get; set; }
            public string 敬体活用形 { get; set; }
            public string 敬体ルール { get; set; }
        }

        public class TestDataSource : TheoryData<string, TestData>  // 必要なパラメータの型に応じて調整
        {
            /// <summary>
            /// テストデータのCSVファイル読み込み。
            /// </summary>
            public TestDataSource()
            {
                using var reader = new StreamReader("NewFeatureResearch/DATA/常体敬体変換総引き当て表.csv", Encoding.UTF8);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Encoding = Encoding.UTF8
                });

                var records = csv.GetRecords<TestData>();
                foreach (var record in records)
                {
                    // テストメソッドに渡すパラメータを設定
                    Add(record.No, record);
                }
            }
        }

        [Theory]
        [ClassData(typeof(TestDataSource))]
        public void 常体敬体変換テスト(string No, TestData data)
        {
            output.WriteLine($"{No}, {data.用法}, {data.常体}, {data.敬体}");
            output.WriteLine("");

            // Tokenizeする
            var jotaiTokens = KuromojiController.Tokenize(new Sentence(data.常体, 1));
            var keitaiTokens = KuromojiController.Tokenize(new Sentence(data.敬体, 1));

            // Token結果目視
            output.WriteLine("★Token...");
            output.WriteLine("常体");
            foreach (var t in jotaiTokens) { output.WriteLine(t.ToString()); }
            output.WriteLine("敬体");
            foreach (var t in keitaiTokens) { output.WriteLine(t.ToString()); }
            output.WriteLine("");

            // 活用型と基本形の検証
            if (jotaiTokens[0].PartOfSpeech[0] == "動詞")
            {
                jotaiTokens[0].InflectionType.Should().Be(data.常体活用型);
                var (success5, surface5) = InflectionResolver.Resolve(jotaiTokens[0], "基本形");
                success5.Should().BeTrue();
                surface5.Should().Be(data.常体基本形);
            }
            else if (jotaiTokens[0].PartOfSpeech[0] == "名詞")
            {
                jotaiTokens[0].Surface.Should().Be(data.常体基本形);
            }
            if (keitaiTokens[0].PartOfSpeech[0] == "動詞")
            {
                keitaiTokens[0].InflectionType.Should().Be(data.敬体活用型);
                var (success6, surface6) = InflectionResolver.Resolve(keitaiTokens[0], "基本形");
                success6.Should().BeTrue();
                surface6.Should().Be(data.敬体基本形);
            }
            else if (keitaiTokens[0].PartOfSpeech[0] == "名詞")
            {
                keitaiTokens[0].Surface.Should().Be(data.敬体基本形);
            }

            // GrammarRuleを取得する
            output.WriteLine("★Rule...");
            output.WriteLine("常体");
            output.WriteLine(data.常体ルール);
            GrammarRule jotaiRule = GrammarRuleExtractor.Run(data.常体ルール);
            //jotaiRule.Pattern[0].Token.PartOfSpeech[0]
            output.WriteLine("敬体");
            output.WriteLine(data.敬体ルール);
            GrammarRule keitaiRule = GrammarRuleExtractor.Run(data.敬体ルール);

            // 想定通りの形態素解析結果かを検証する
            // テストケースに記号が含まれるケースがあるので記号除去する。
            var (success1, jotaiResultTokens) = jotaiRule.MatchExtendAtEnd(jotaiTokens.Where(x => x.PartOfSpeech[0] != "記号").ToImmutableList());
            success1.Should().BeTrue();
            var (success2, keitaiResultTokens) = keitaiRule.MatchExtendAtEnd(keitaiTokens.Where(x => x.PartOfSpeech[0] != "記号").ToImmutableList());
            success2.Should().BeTrue();

            ////////////////////////////////////////////////////////////////
            // 変換処理
            ////////////////////////////////////////////////////////////////

            if (data.変換.ToLower() == "skip")
            {
                output.WriteLine("SKIP");
                return;
            }

            // 【常体→敬体】
            output.WriteLine("【常体→敬体】");

            if (data.変換 != "<")
            {
                //// 活用解決
                //string head = "";
                //if (jotaiResultTokens[0].PartOfSpeech[0] == "動詞")
                //{
                //    var (success3, surface3) = InflectionResolver.Resolve(jotaiResultTokens[0], data.敬体活用形);
                //    success3.Should().BeTrue();
                //    head = surface3;
                //}
                //else if (jotaiResultTokens[0].PartOfSpeech[0] == "名詞")
                //{
                //    head = jotaiResultTokens[0].Surface;
                //}

                //// 語尾付与＋想定文言と合致するかどうかを検証
                //output.WriteLine("★Result...");
                //string resultStr = $"{head}{data.敬体語尾}";
                //output.WriteLine(resultStr);
                //resultStr.Should().Be(data.敬体);
            }
            else
            {
                output.WriteLine("★常体→敬体変換スキップ...");
                output.WriteLine("");
            }
        }
    }
}
