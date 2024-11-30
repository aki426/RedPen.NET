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

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Errors;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Validators.SentenceValidator.Tests
{
    public class JapaneseStyleValidatorTests
    {
        private readonly ITestOutputHelper output;

        public JapaneseStyleValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("001", "これは山です。これは川だ。", JodoshiStyle.DesuMasu, 1, "だ")]
        [InlineData("002", "これは山です。これは川だ。", JodoshiStyle.DaDearu, 1, "です")]
        [InlineData("003", "今日はいい天気です。", JodoshiStyle.DesuMasu, 0, "")]
        [InlineData("004", "今日はいい天気です。", JodoshiStyle.DaDearu, 1, "です")]
        [InlineData("005", "昨日は雨だったが、持ち直しました。", JodoshiStyle.DesuMasu, 1, "だった")]
        [InlineData("006", "昨日は雨だったが、持ち直しました。", JodoshiStyle.DaDearu, 1, "ました")]
        [InlineData("007", "明日もいい天気だといいですね。", JodoshiStyle.DesuMasu, 0, "")]
        [InlineData("008", "明日もいい天気だといいですね。", JodoshiStyle.DaDearu, 1, "です")]
        [InlineData("009", "昨日は雨でしたが、持ち直しました。", JodoshiStyle.DesuMasu, 0, "")]
        [InlineData("010", "昨日は雨でしたが、持ち直しました。", JodoshiStyle.DaDearu, 2, "でした,ました")]
        [InlineData("011", "明日もいい天気だといいですね。", JodoshiStyle.DesuMasu, 0, "")]
        [InlineData("012", "明日もいい天気だといいですね。", JodoshiStyle.DaDearu, 1, "です")]
        [InlineData("013", "今日はいい天気である。", JodoshiStyle.DesuMasu, 1, "である")]
        [InlineData("014", "今日はいい天気である。", JodoshiStyle.DaDearu, 0, "")]
        [InlineData("015", "昨日は雨だったが、持ち直した。", JodoshiStyle.DesuMasu, 2, "だった,た")]
        [InlineData("016", "昨日は雨だったが、持ち直した。", JodoshiStyle.DaDearu, 0, "")]
        [InlineData("017", "昨日は雨であったが、持ち直した。", JodoshiStyle.DesuMasu, 2, "であった,た")]
        [InlineData("018", "昨日は雨であったが、持ち直した。", JodoshiStyle.DaDearu, 0, "")]
        [InlineData("019", "昨日は雨だったのであったが、持ち直した。", JodoshiStyle.DesuMasu, 2, "であった,た")]
        [InlineData("020", "昨日は雨だったのであったが、持ち直した。", JodoshiStyle.DaDearu, 0, "")]
        [InlineData("021", "明日もいい天気だといい。", JodoshiStyle.DesuMasu, 1, "いい")]
        [InlineData("022", "明日もいい天気だといい。", JodoshiStyle.DaDearu, 0, "")]
        [InlineData("023", "彼の今日のお昼の弁当はのり弁です。", JodoshiStyle.DesuMasu, 0, "")]
        [InlineData("024", "彼の今日のお昼の弁当はのり弁です。", JodoshiStyle.DaDearu, 1, "です")]
        [InlineData("025", "それは贅沢である。", JodoshiStyle.DesuMasu, 1, "である")]
        [InlineData("026", "それは贅沢である。", JodoshiStyle.DaDearu, 0, "")]
        [InlineData("027", "しかし彼には選択肢が無かったのである。", JodoshiStyle.DesuMasu, 1, "である")]
        [InlineData("028", "しかし彼には選択肢が無かったのである。", JodoshiStyle.DaDearu, 0, "")]
        [InlineData("027", "しかし彼には選択肢が無かったのです。", JodoshiStyle.DesuMasu, 0, "")]
        [InlineData("028", "しかし彼には選択肢が無かったのです。", JodoshiStyle.DaDearu, 1, "です")]
        [InlineData("029", "明日は良い天気だと思います。", JodoshiStyle.DesuMasu, 0, "")] // MEMO: 「～だと」という表現はです・ます調でも許容すべき。
        [InlineData("030", "明日は良い天気だと思います。", JodoshiStyle.DaDearu, 1, "ます")] // MEMO: 「～だと」という表現はです・ます調でも許容すべき。
        [InlineData("031", "明日もいい天気だといい。", JodoshiStyle.DesuMasu, 1, "いい")]
        [InlineData("032", "明日もいい天気だといい。", JodoshiStyle.DaDearu, 0, "")]
        [InlineData("033", "明日もいい天気だといいね。", JodoshiStyle.DesuMasu, 1, "いい")]
        [InlineData("034", "明日もいい天気だといいね。", JodoshiStyle.DaDearu, 0, "")]
        [InlineData("035", "昨日は雨だったのです。", JodoshiStyle.DesuMasu, 0, "")] // MEMO: 「～だったの」という表現はです・ます調でも許容すべき。
        [InlineData("036", "昨日は雨だったのです。", JodoshiStyle.DaDearu, 1, "です")]
        [InlineData("037", "弁当を食べたのです。", JodoshiStyle.DesuMasu, 0, "")] // MEMO: 「～たの」という表現はです・ます調でも許容すべき。
        [InlineData("038", "弁当を食べたのです。", JodoshiStyle.DaDearu, 1, "です")]
        [InlineData("039", "明日は雨だろう。", JodoshiStyle.DesuMasu, 1, "だろう")]
        [InlineData("040", "明日は雨だろう。", JodoshiStyle.DaDearu, 0, "")]
        [InlineData("041", "明日は雨だろうか。", JodoshiStyle.DesuMasu, 1, "だろう")]
        [InlineData("042", "明日は雨だろうか。", JodoshiStyle.DaDearu, 0, "")]
        [InlineData("043", "明日は雨であろう。", JodoshiStyle.DesuMasu, 1, "であろう")]
        [InlineData("044", "明日は雨であろう。", JodoshiStyle.DaDearu, 0, "")]
        [InlineData("045", "明日は雨であろうか。", JodoshiStyle.DesuMasu, 1, "であろう")]
        [InlineData("046", "明日は雨であろうか。", JodoshiStyle.DaDearu, 0, "")]
        [InlineData("047", "明日は雨でしょう。", JodoshiStyle.DesuMasu, 0, "")]
        [InlineData("048", "明日は雨でしょう。", JodoshiStyle.DaDearu, 1, "でしょう")]
        [InlineData("049", "明日は雨でしょうか。", JodoshiStyle.DesuMasu, 0, "")]
        [InlineData("050", "明日は雨でしょうか。", JodoshiStyle.DaDearu, 1, "でしょう")]
        [InlineData("051", "明日は雨でしょうね。", JodoshiStyle.DesuMasu, 0, "")]
        [InlineData("052", "明日は雨でしょうね。", JodoshiStyle.DaDearu, 1, "でしょう")]
        [InlineData("053", "明日は雨だろうな。", JodoshiStyle.DesuMasu, 1, "だろう")]
        [InlineData("054", "明日は雨だろうな。", JodoshiStyle.DaDearu, 0, "")]
        [InlineData("055", "明日は雨だろうね。", JodoshiStyle.DesuMasu, 1, "だろう")]
        [InlineData("056", "明日は雨だろうね。", JodoshiStyle.DaDearu, 0, "")]
        [InlineData("057", "明日は雨だろうかね。", JodoshiStyle.DesuMasu, 1, "だろう")]
        [InlineData("058", "明日は雨だろうかね。", JodoshiStyle.DaDearu, 0, "")]
        [InlineData("059", "明日は雨であろうね。", JodoshiStyle.DesuMasu, 1, "であろう")]
        [InlineData("060", "明日は雨であろうね。", JodoshiStyle.DaDearu, 0, "")]
        [InlineData("061", "明日は雨でしょうかね。", JodoshiStyle.DesuMasu, 0, "")]
        [InlineData("062", "明日は雨でしょうかね。", JodoshiStyle.DaDearu, 1, "でしょう")]
        [InlineData("063", "明日は雨だろ。", JodoshiStyle.DesuMasu, 1, "だろ")]
        [InlineData("064", "明日は雨だろ。", JodoshiStyle.DaDearu, 0, "")]
        [InlineData("065", "明日は雨だろが。", JodoshiStyle.DesuMasu, 1, "だろ")]
        [InlineData("066", "明日は雨だろが。", JodoshiStyle.DaDearu, 0, "")]
        [InlineData("067", "明日は遠足に行かないのです。", JodoshiStyle.DesuMasu, 0, "")]
        [InlineData("068", "明日は遠足に行かないのです。", JodoshiStyle.DaDearu, 1, "です")]
        [InlineData("069", "明日は遠足に行かないです。", JodoshiStyle.DesuMasu, 0, "")]
        [InlineData("070", "明日は遠足に行かないです。", JodoshiStyle.DaDearu, 1, "ないです")]
        public void BasicTest(string nouse1, string text, JodoshiStyle jodoshiStyle, int errorCount, string expected)
        {
            var validatorConfiguration = new JapaneseStyleConfiguration(ValidationLevel.ERROR, jodoshiStyle);
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validatorの生成。
            var validator = new JapaneseStyleValidator(
                documentLang,
                symbolTable,
                validatorConfiguration);

            // Tokenizeが走るので大仰でもDocumentを生成する。
            Document document = Document.Builder(RedPenTokenizerFactory.CreateTokenizer(documentLang))
                    .AddSection(1)
                    .AddParagraph()
                    .AddSentence(new Sentence(text, 1))
                    .Build(); // TokenizeをBuild時に実行する。

            var sentence = document.Sections[0].Paragraphs[0].Sentences[0];

            output.WriteLine("★全Token:");
            foreach (var token in sentence.Tokens)
            {
                output.WriteLine(token.ToString());
            }

            output.WriteLine("");
            output.WriteLine("★複合助動詞Token:");
            foreach (var token in JapaneseStyleValidator.GetCompoundJodoshi(sentence))
            {
                output.WriteLine(token.ToString());
            }

            // Validation
            var errors = validator.Validate(sentence);

            errors.Should().HaveCount(errorCount);

            if (errors.Any())
            {
                var manager = new ErrorMessageManager();

                output.WriteLine("");
                output.WriteLine("★Errors:");
                foreach (var error in errors)
                {
                    output.WriteLine(error.ToString());
                    output.WriteLine(manager.GetErrorMessage(error, CultureInfo.GetCultureInfo("ja-JP")));
                }

                string.Join(",", errors.Select(e => e.MessageArgs[0].ToString())).Should().Be(expected);
            }
        }
    }
}
