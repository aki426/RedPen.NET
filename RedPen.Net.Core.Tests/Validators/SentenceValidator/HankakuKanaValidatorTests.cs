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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Lucene.Net.Util.Fst;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Errors;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validators.SentenceValidator
{
    public class HankakuKanaValidatorTests
    {
        private ITestOutputHelper output;

        public HankakuKanaValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("001", "岩の木陰にﾊﾅが咲いている。", 1, "ﾊﾅ")]
        [InlineData("002", "岩の木陰にハナが咲いている。", 0, "")]
        [InlineData("003", "岩の木陰に ハナ が咲いている。", 0, "")]
        [InlineData("004", "生成AI（ｾｲｾｲｴｰｱｲ）はﾌﾟﾛﾝﾌﾟﾄｴﾝｼﾞﾆｱﾘﾝｸﾞが重要といわれています。", 2, "ｾｲｾｲｴｰｱｲ,ﾌﾟﾛﾝﾌﾟﾄｴﾝｼﾞﾆｱﾘﾝｸﾞ")]
        [InlineData("005", "この文章は､｢｣など半角カナ記号を含んでいますがValidatorで検出されません｡", 0, "")]
        public void BasicTest(string nouse1, string text, int errorCount, string expected)

        {
            var validatorConfiguration = new HankakuKanaConfiguration(ValidationLevel.ERROR);
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validatorの生成。
            var validator = new HankakuKanaValidator(
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
