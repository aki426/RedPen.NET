using RedPen.Net.Core.Parser;
using RedPen.Net.Core.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RedPen.Net.Core.Model
{
    // MEMO: DocumentBuilderという名前なのでBuilderパターンを実装しているように見えるが、
    // DocumentBuilderがDirectorの役割をしていて、TokenizerがBuilderの役割をしているように見える。
    // 実際問題としていろいろなフォーマットに対応するためにIRedPenTokenizerを実装したTokenizerを差し替えて
    // DocumentBuilder内で使用しているので、BuilderパターンというよりはStrategyパターンに近いように見える。

    /// <summary>
    /// The document builder.
    /// </summary>
    public class DocumentBuilder
    {
        /// <summary>IRedPenTokenizer</summary>
        public IRedPenTokenizer Tokenizer { get; set; }

        /// <summary>The flag of built</summary>
        public bool Built { get; set; } = false;

        /// <summary>Section list</summary>
        public List<Section> Sections { get; init; }

        /// <summary>File name of this document</summary>
        public string? FileName { get; set; }

        /// <summary>RedPen preprocessor rules</summary>
        public HashSet<PreprocessorRule> PreprocessorRules { get; set; }

        /// <summary>Initializes a new instance of the <see cref="DocumentBuilder"/> class.</summary>
        public DocumentBuilder() : this(new WhiteSpaceTokenizer())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentBuilder"/> class.
        /// </summary>
        /// <param name="tokenizer">The tokenizer.</param>
        public DocumentBuilder(IRedPenTokenizer tokenizer)
        {
            Sections = new List<Section>();
            FileName = null;
            Tokenizer = tokenizer;
        }

        /// <summary>
        /// Ensures the not built.
        /// </summary>
        private void EnsureNotBuilt()
        {
            if (this.Built)
            {
                throw new InvalidOperationException("already built");
            }
        }

        /// <summary>
        /// Builds the.
        /// </summary>
        /// <returns>A Document.</returns>
        public Document Build()
        {
            this.Built = true;
            return new Document(Sections, FileName, PreprocessorRules);
        }

        /// <summary>
        /// Add a section.
        /// </summary>
        /// <param name="section">a section in file content</param>
        /// <returns>A DocumentBuilder.</returns>
        public DocumentBuilder AppendSection(Section section)
        {
            // Buildフラグの検証。不正の場合は例外をスローする。
            EnsureNotBuilt();

            // すべてのHeaderSentencesに対してTokenizeを実行する。
            section = section with
            {
                HeaderSentences = section.HeaderSentences.Select(sentence => sentence with { Tokens = Tokenizer.Tokenize(sentence) }).ToList()
            };

            //foreach (Sentence sentence in section.HeaderSentences)
            //{
            //    sentence.Tokens = this.Tokenizer.Tokenize(sentence);
            //}

            Sections.Add(section);

            return this;
        }

        /// <summary>
        /// Adds the paragraph.
        /// </summary>
        /// <returns>A DocumentBuilder.</returns>
        public DocumentBuilder AddParagraph()
        {
            EnsureNotBuilt();
            if (Sections.Count == 0)
            {
                // TODO: 一連の処理をCreateHeaderとかNewSectionなどといった関数にまとめる。
                List<Sentence> headers = new List<Sentence>();
                headers.Add(new Sentence("", 0));
                this.AppendSection(new Section(0, headers));
            }

            Section lastSection = this.Sections.Last();
            lastSection.Paragraphs.Add(new Paragraph());

            return this;
        }

        /// <summary>
        /// Sentenceを追加する。入れるべきSectionやParagraphが存在しない場合は自動的に追加される。
        /// 追加されるSentenceのTokenizeも同時に行われる。
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <returns>A DocumentBuilder.</returns>
        public DocumentBuilder AddSentence(Sentence sentence)
        {
            EnsureNotBuilt();

            // Sectionが存在しない場合は、Sectionを作成する。
            if (Sections.Count == 0)
            {
                List<Sentence> headers = new List<Sentence>();
                headers.Add(new Sentence("", 0));
                this.AppendSection(new Section(0, headers));
            }
            Section lastSection = this.Sections.Last();

            // Paragraphが存在しない場合は、Paragraphを作成する。
            if (lastSection.Paragraphs.Count == 0)
            {
                AddParagraph();
            }
            Paragraph lastParagraph = lastSection.Paragraphs.Last();

            // Paragraphで最初のSentenceだった場合、フラグをTrueに設定する。
            if (lastParagraph.Sentences.Count == 0)
            {
                sentence = sentence with { IsFirstSentence = true };
            }
            // Tokenize
            sentence = sentence with { Tokens = Tokenizer.Tokenize(sentence) };

            lastParagraph.AppendSentence(sentence);

            return this;
        }

        /// <summary>
        /// Adds the list block.
        /// </summary>
        /// <returns>A DocumentBuilder.</returns>
        public DocumentBuilder AddListBlock()
        {
            EnsureNotBuilt();

            // TODO: 登録すべき先のオブジェクトが存在しない場合に自動的に追加する／Exceptionを投げる差はどこにあるのか要検討。
            if (Sections.Count == 0)
            {
                throw new InvalidOperationException("No section to add a sentence");
            }

            Section lastSection = Sections.Last();

            lastSection.AppendListBlock();

            return this;
        }

        /// <summary>
        /// Adds the list element.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="contents">The contents.</param>
        /// <returns>A DocumentBuilder.</returns>
        public DocumentBuilder AddListElement(int level, List<Sentence> contents)
        {
            EnsureNotBuilt();

            // TODO: 登録すべき先のオブジェクトが存在しない場合に自動的に追加する／Exceptionを投げる差はどこにあるのか要検討。
            if (Sections.Count == 0)
            {
                throw new InvalidOperationException("No section to add a sentence");
            }
            Section lastSection = Sections.Last();

            //foreach (Sentence sentence in contents)
            //{
            //    sentence.Tokens = Tokenizer.Tokenize(sentence);
            //}

            // Tokenize済みのSentenceリストを追加する。
            lastSection.AppendListElement(
                contents.Select(sentence => sentence with { Tokens = Tokenizer.Tokenize(sentence) }).ToList(),
                level);

            return this;
        }

        /// <summary>
        /// Adds the list element.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="str">The str.</param>
        /// <returns>A DocumentBuilder.</returns>
        public DocumentBuilder AddListElement(int level, string str)
        {
            EnsureNotBuilt();

            List<Sentence> elementSentence = new List<Sentence>();
            elementSentence.Add(new Sentence(str, 0));
            this.AddListElement(level, elementSentence);
            return this;
        }

        /// <summary>
        /// Adds the section.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="header">The header.</param>
        /// <returns>A DocumentBuilder.</returns>
        public DocumentBuilder AddSection(int level, List<Sentence> header)
        {
            EnsureNotBuilt();
            AppendSection(new Section(level, header));
            return this;
        }

        /// <summary>
        /// SectionHeaderとして文字列を追加する。
        /// 文字列は現在の最後のSectionのHeaderSentencesの末尾に1行追加される。
        /// Tokenizeも同時に行われる。
        /// </summary>
        /// <param name="header">The header string.</param>
        /// <returns>A DocumentBuilder.</returns>
        public DocumentBuilder AddSectionHeader(string header)
        {
            EnsureNotBuilt();

            // TODO: 登録すべき先のオブジェクトが存在しない場合に自動的に追加する／Exceptionを投げる差はどこにあるのか要検討。
            Section lastSection = Sections.Last();
            if (null == lastSection)
            {
                throw new InvalidOperationException("Document does not have any section");
            }

            // 登録先のSectionのHeaderSentences。
            List<Sentence> headers = lastSection.HeaderSentences;

            // TODO: 新しいSentenceのLineNumはheaders.Count + 1になるべきでは？
            Sentence sentence = new Sentence(header, headers.Count);
            sentence = sentence with { Tokens = Tokenizer.Tokenize(sentence) };

            headers.Add(sentence);

            return this;
        }

        /// <summary>
        /// Adds the section.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>A DocumentBuilder.</returns>
        public DocumentBuilder AddSection(int level)
        {
            EnsureNotBuilt();
            AddSection(level, new List<Sentence>());
            return this;
        }

        /// <summary>
        /// Adds the sentence.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <returns>A DocumentBuilder.</returns>
        public DocumentBuilder AddSentence(string content, int lineNumber)
        {
            EnsureNotBuilt();
            AddSentence(new Sentence(content, lineNumber));
            return this;
        }

        /// <summary>
        /// Sets the file name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A DocumentBuilder.</returns>
        public DocumentBuilder SetFileName(string? name)
        {
            EnsureNotBuilt();
            this.FileName = name ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the preprocessor rules.
        /// </summary>
        /// <param name="rules">The rules.</param>
        /// <returns>A DocumentBuilder.</returns>
        public DocumentBuilder SetPreprocessorRules(HashSet<PreprocessorRule> rules)
        {
            this.PreprocessorRules = rules;
            return this;
        }
    }
}
