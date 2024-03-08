using redpen_core.parser;
using redpen_core.tokenizer;

namespace redpen_core.model
{
    // MEMO: DocumentBuilderという名前なのでBuilderパターンを実装しているように見えるが、
    // DocumentBuilderがDirectorの役割をしていて、TokenizerがBuilderの役割をしているように見える。
    // 実際問題としていろいろなフォーマットに対応するためにIRedPenTokenizerを実装したTokenizerを差し替えて
    // DocumentBuilder内で使用しているので、BuilderパターンというよりはStrategyパターンに近いように見える。

    public class DocumentBuilder
    {
        public IRedPenTokenizer Tokenizer { get; set; }
        public bool Built { get; set; } = false;
        public List<Section> Sections { get; init; }

        public string? FileName { get; set; }
        public HashSet<PreprocessorRule> PreprocessorRules { get; set; }

        public DocumentBuilder() : this(new WhiteSpaceTokenizer())
        {
        }

        public DocumentBuilder(IRedPenTokenizer tokenizer)
        {
            Sections = new List<Section>();
            FileName = null;
            Tokenizer = tokenizer;
        }

        private void EnsureNotBuilt()
        {
            if (this.Built)
            {
                throw new InvalidOperationException("already built");
            }
        }

        public Document Build()
        {
            this.Built = true;
            return new Document(Sections, FileName, PreprocessorRules);
        }

        //    /**
        //     * Add a section.
        //     *
        //     * @param section a section in file content
        //     * @return DocumentBuilder itself
        //     */
        public DocumentBuilder AppendSection(Section section)
        {
            // Buildフラグの検証。不正の場合は例外をスローする。
            EnsureNotBuilt();

            foreach (Sentence sentence in section.HeaderSentences)
            {
                sentence.Tokens = this.Tokenizer.Tokenize(sentence.Content);
            }

            Sections.Add(section);

            return this;
        }

        public DocumentBuilder AddParagraph()
        {
            EnsureNotBuilt();
            if (Sections.Count == 0)
            {
                List<Sentence> headers = new List<Sentence>();
                headers.Add(new Sentence("", 0));
                this.AppendSection(new Section(0, headers));
            }

            Section lastSection = this.Sections.Last();
            lastSection.Paragraphs.Add(new Paragraph());

            return this;
        }

        //    /**
        //     * Add a sentence.
        //     *
        //     * @param sentence sentence
        //     * @return DocumentBuilder itself
        //     */
        public DocumentBuilder AddSentence(Sentence sentence)
        {
            EnsureNotBuilt();

            if (Sections.Count == 0)
            {
                List<Sentence> headers = new List<Sentence>();
                headers.Add(new Sentence("", 0));
                this.AppendSection(new Section(0, headers));
            }

            Section lastSection = this.Sections.Last();
            if (lastSection.Paragraphs.Count == 0)
            {
                AddParagraph(); // Note: add paragraph automatically
            }

            Paragraph lastParagraph = lastSection.Paragraphs.Last();

            lastParagraph.AppendSentence(sentence);
            if (lastParagraph.Sentences.Count == 1)
            {
                sentence.IsFirstSentence = true;
            }

            sentence.Tokens = Tokenizer.Tokenize(sentence.Content);
            return this;
        }

        //    /**
        //     * Add a new list block.
        //     *
        //     * @return Builder itself
        //     */
        public DocumentBuilder AddListBlock()
        {
            EnsureNotBuilt();

            if (Sections.Count == 0)
            {
                throw new InvalidOperationException("No section to add a sentence");
            }

            Section lastSection = Sections.Last();

            lastSection.AppendListBlock();

            return this;
        }

        //    /**
        //     * Add list element to the last list block.
        //     *
        //     * @param level    indentation level
        //     * @param contents content of list element
        //     * @return Builder
        //     */
        public DocumentBuilder AddListElement(int level, List<Sentence> contents)
        {
            EnsureNotBuilt();
            if (Sections.Count == 0)
            {
                throw new InvalidOperationException("No section to add a sentence");
            }
            Section lastSection = Sections.Last();

            foreach (Sentence sentence in contents)
            {
                sentence.Tokens = Tokenizer.Tokenize(sentence.Content);
            }
            lastSection.AppendListElement(contents, level);
            return this;
        }

        //    /**
        //     * Add list element to the last list block.
        //     *
        //     * @param level indentation level
        //     * @param str   content of list element
        //     * @return Builder
        //     * NOTE: parameter str is not split into more than one Sentence object.
        //     */
        public DocumentBuilder AddListElement(int level, string str)
        {
            EnsureNotBuilt();

            List<Sentence> elementSentence = new List<Sentence>();
            elementSentence.Add(new Sentence(str, 0));
            this.AddListElement(level, elementSentence);
            return this;
        }

        //    /**
        //     * Add a section to the document.
        //     *
        //     * @param level  section level
        //     * @param header header contents
        //     * @return Builder
        //     */
        public DocumentBuilder AddSection(int level, List<Sentence> header)
        {
            EnsureNotBuilt();
            AppendSection(new Section(level, header));
            return this;
        }

        //    /**
        //     * Add section header content to the last section.
        //     *
        //     * @param header header content
        //     * @return Builder
        //     * NOTE: parameter header is not split into more than one Sentence object.
        //     */
        public DocumentBuilder AddSectionHeader(string header)
        {
            EnsureNotBuilt();
            Section lastSection = Sections.Last();
            if (null == lastSection)
            {
                throw new InvalidOperationException("Document does not have any section");
            }
            List<Sentence> headers = lastSection.HeaderSentences;
            Sentence sentence = new Sentence(header, headers.Count);
            sentence.Tokens = Tokenizer.Tokenize(sentence.Content);
            headers.Add(sentence);
            return this;
        }

        //    /**
        //     * Add a section without header content.
        //     *
        //     * @param level section level
        //     * @return Builder
        //     */
        public DocumentBuilder AddSection(int level)
        {
            EnsureNotBuilt();
            AddSection(level, new List<Sentence>());
            return this;
        }

        //    /**
        //     * Add sentence to document.
        //     *
        //     * @param content    sentence content
        //     * @param lineNumber line number
        //     * @return Builder
        //     */
        public DocumentBuilder AddSentence(string content, int lineNumber)
        {
            EnsureNotBuilt();
            AddSentence(new Sentence(content, lineNumber));
            return this;
        }

        //    /**
        //     * Set file name.
        //     *
        //     * @param name file name
        //     * @return Builder
        //     */
        public DocumentBuilder SetFileName(string? name)
        {
            EnsureNotBuilt();
            this.FileName = name ?? string.Empty;
            return this;
        }

        //    /**
        //     * Set preprocessor rules
        //     *
        //     * @param rules preprocessor rules
        //     * @return Builder
        //     */
        public DocumentBuilder SetPreprocessorRules(HashSet<PreprocessorRule> rules)
        {
            this.PreprocessorRules = rules;
            return this;
        }
    }
}
