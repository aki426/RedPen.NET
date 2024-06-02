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
using FluentAssertions;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;
using Xunit;

namespace RedPen.Net.Core.Tests.Model
{
    public class DocumentTests
    {
        [Fact()]
        public void BuildDocumentTest()
        {
            // 中身が無く、ファイル名のみのDocumentを生成する。
            Document document;
            document = Document.Builder().SetFileName("Foobar").Build();
            document.Sections.Count.Should().Be(0);
        }

        [Fact()]
        public void BuildDocumentWithListTest()
        {
            Document doc = Document.Builder()
                .SetFileName("Foobar")
                .AddSection(0)
                .AddSectionHeader("baz")
                .AddParagraph()
                .AddSentence(new Sentence("sentence0", 1))
                .AddSentence(new Sentence("sentence1", 2))
                .AddListBlock()
                .AddListElement(0, "list0")
                .AddListElement(0, "list1")
                .AddListElement(1, "list2")
                .Build();

            doc.Sections.Count.Should().Be(1);
            doc.Sections[0].Level.Should().Be(0);
            doc.FileName.Should().NotBeNull(); // Optional<string>からの移植のため。
            doc.FileName.Should().Be("Foobar");
            doc.Sections[0].HeaderSentences[0].Content.Should().Be("baz");
            doc.Sections[0].Paragraphs.Count.Should().Be(1);
            doc.Sections[0].Paragraphs[0].Sentences.Count.Should().Be(2);
            doc.Sections[0].Paragraphs[0].Sentences[0].Content.Should().Be("sentence0");
            doc.Sections[0].Paragraphs[0].Sentences[0].IsFirstSentence.Should().BeTrue();
            doc.Sections[0].Paragraphs[0].Sentences[0].LineNumber.Should().Be(1);
            doc.Sections[0].Paragraphs[0].Sentences[1].Content.Should().Be("sentence1");
            doc.Sections[0].Paragraphs[0].Sentences[1].IsFirstSentence.Should().BeFalse();
            doc.Sections[0].Paragraphs[0].Sentences[1].LineNumber.Should().Be(2);
            doc.Sections[0].ListBlocks.Count.Should().Be(1);
            doc.Sections[0].ListBlocks[0].ListElements.Count.Should().Be(3);
            doc.Sections[0].ListBlocks[0].ListElements[0].Level.Should().Be(0);
            doc.Sections[0].ListBlocks[0].ListElements[0].Sentences[0].Content.Should().Be("list0");
            doc.Sections[0].ListBlocks[0].ListElements[1].Level.Should().Be(0);
            doc.Sections[0].ListBlocks[0].ListElements[1].Sentences[0].Content.Should().Be("list1");
            doc.Sections[0].ListBlocks[0].ListElements[2].Level.Should().Be(1);
            doc.Sections[0].ListBlocks[0].ListElements[2].Sentences[0].Content.Should().Be("list2");
        }

        [Fact()]
        public void BuildMultiDocumentTest()
        {
            List<Document> docs = new List<Document>() {
                // 1つ目のDocument
                Document.Builder()
                    .SetFileName("doc1")
                    .AddSection(0)
                    .AddSectionHeader("sec1")
                    .AddParagraph()
                    .AddSentence(new Sentence("sentence00", 1))
                    .AddSentence(new Sentence("sentence01", 2))
                    .Build(),

                // 2つ目のDocument
                Document.Builder()
                    .SetFileName("doc2")
                    .AddSection(0)
                    .AddSectionHeader("sec2")
                    .AddParagraph()
                    .AddSentence(new Sentence("sentence10", 1))
                    .AddSentence(new Sentence("sentence11", 2))
                    .Build()
            };

            docs.Count.Should().Be(2);

            // 1つ目
            var first = docs[0];
            first.Sections.Count.Should().Be(1);
            first.FileName.Should().Be("doc1");
            first.Sections[0].HeaderSentences[0].Content.Should().Be("sec1");
            first.Sections[0].Level.Should().Be(0);
            first.Sections[0].Paragraphs.Count.Should().Be(1);
            first.Sections[0].Paragraphs[0].Sentences.Count.Should().Be(2);
            first.Sections[0].Paragraphs[0].Sentences[0].Content.Should().Be("sentence00");
            first.Sections[0].Paragraphs[0].Sentences[0].IsFirstSentence.Should().BeTrue();
            first.Sections[0].Paragraphs[0].Sentences[0].LineNumber.Should().Be(1);
            first.Sections[0].Paragraphs[0].Sentences[1].Content.Should().Be("sentence01");
            first.Sections[0].Paragraphs[0].Sentences[1].IsFirstSentence.Should().BeFalse();
            first.Sections[0].Paragraphs[0].Sentences[1].LineNumber.Should().Be(2);

            // 2つ目
            var second = docs[1];
            second.Sections.Count.Should().Be(1);
            second.FileName.Should().Be("doc2");
            second.Sections[0].HeaderSentences[0].Content.Should().Be("sec2");
            second.Sections[0].Level.Should().Be(0);
            second.Sections[0].Paragraphs.Count.Should().Be(1);
            second.Sections[0].Paragraphs[0].Sentences.Count.Should().Be(2);
            second.Sections[0].Paragraphs[0].Sentences[0].Content.Should().Be("sentence10");
            second.Sections[0].Paragraphs[0].Sentences[0].IsFirstSentence.Should().BeTrue();
            second.Sections[0].Paragraphs[0].Sentences[0].LineNumber.Should().Be(1);
            second.Sections[0].Paragraphs[0].Sentences[1].Content.Should().Be("sentence11");
            second.Sections[0].Paragraphs[0].Sentences[1].IsFirstSentence.Should().BeFalse();
            second.Sections[0].Paragraphs[0].Sentences[1].LineNumber.Should().Be(2);
        }

        [Fact]
        public void TokenizeTest()
        {
            Document doc = Document.Builder()
                .SetFileName("foobar")
                .AddSection(0)
                .AddSectionHeader("baz")
                .AddParagraph()
                .AddSentence(new Sentence("This is a foobar.", 1))
                .Build();

            doc.Sections.Count.Should().Be(1);
            doc.Sections[0].Paragraphs[0].Sentences[0].Tokens.Count.Should().Be(5);
            doc.Sections[0].Paragraphs[0].Sentences[0].Tokens[0].Surface.Should().Be("This");
            doc.Sections[0].Paragraphs[0].Sentences[0].Tokens[3].Surface.Should().Be("foobar");
            doc.Sections[0].Paragraphs[0].Sentences[0].Tokens[4].Surface.Should().Be(".");
        }

        [Fact]
        public void 日本語のトークナイザーテスト()
        {
            Document doc = Document.Builder(new KuromojiTokenizer())
                    .SetFileName("今日")
                    .AddSection(0)
                    .AddSectionHeader("天気")
                    .AddParagraph()
                    // LineNumberは実際のドキュメントのインデクスに合わせるためか1から始まる。
                    .AddSentence(new Sentence("今日は晴天だ。", 1))
                    .Build();

            doc.Sections.Count.Should().Be(1);
            doc.Sections[0].Paragraphs[0].Sentences[0].Tokens.Count.Should().Be(5);

            doc.Sections[0].Paragraphs[0].Sentences[0].Tokens[0].Surface.Should().Be("今日");
            doc.Sections[0].Paragraphs[0].Sentences[0].Tokens[1].Surface.Should().Be("は");
            doc.Sections[0].Paragraphs[0].Sentences[0].Tokens[2].Surface.Should().Be("晴天");
            doc.Sections[0].Paragraphs[0].Sentences[0].Tokens[3].Surface.Should().Be("だ");
            doc.Sections[0].Paragraphs[0].Sentences[0].Tokens[4].Surface.Should().Be("。");
        }

        [Fact]
        public void AddParagraphBeforeSectionTest()
        {
            Document document = Document.Builder()
                .SetFileName("Foobar")
                // AddParagraphは内部でSectionの有無を判定して、存在しない場合追加しているのでExceptionを吐かない。
                .AddParagraph()
                .AddSection(1)
                .Build();

            document.Sections.Count.Should().Be(2);
            document.Sections[0].Level.Should().Be(0);
            document.Sections[1].Level.Should().Be(1);
        }

        [Fact]
        public void AddListBlockBeforeSectionTest()
        {
            Action act = () => Document.Builder()
                .SetFileName("Foobar")
                .AddListBlock() // 必ず親要素を追加してからでないと子要素を追加できない。
                .AddSection(0)
                .Build();

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void AddListElementBeforeListBlockTest()
        {
            Action act = () => Document.Builder()
                .SetFileName("Foobar")
                .AddListElement(0, "foo") // 必ず親要素を追加してからでないと子要素を追加できない。
                .AddListBlock()
                .Build();

            act.Should().Throw<InvalidOperationException>();
        }
    }
}
