namespace redpen_core.model
{
    // MEMO: JAVA版にあった、プロパティおよびListのメソッドで済む関数は削除した。

    public record class Section
    {
        /// <summary>Subsections</summary>
        public List<Section> Subsections { get; init; }

        public List<Paragraph> Paragraphs { get; init; }
        public List<ListBlock> ListBlocks { get; init; }
        public List<Sentence> HeaderSentences { get; init; }
        public int Level { get; set; }
        public Section? ParentSection { get; set; }

        public Section(int level, List<Sentence> headerSentences)
        {
            this.Level = level;
            this.ParentSection = null;

            this.Subsections = new List<Section>();
            this.Paragraphs = new List<Paragraph>();
            this.ListBlocks = new List<ListBlock>();
            this.HeaderSentences = headerSentences;
        }

        public Section(int level) : this(level, new List<Sentence>())
        {
        }

        public Section(int level, string headerString) :
            this(level, new List<Sentence> { new Sentence(headerString, 0) })
        { }

        public void AppendSubSection(Section section)
        {
            section.ParentSection = this;
            this.Subsections.Add(section);
        }

        public Sentence? GetHeaderSentence(int index)
        {
            if (this.HeaderSentences.Count > index)
            {
                return this.HeaderSentences[index];
            }

            return null;
        }

        /// <summary>
        /// Get iterator of header sentences.
        /// When there is not specified header in the section, return null otherwise return specified id.
        /// </summary>
        /// <returns>sentence containing all header contents in the section</returns>
        public Sentence GetJoinedHeaderContents()
        {
            int lineNum = HeaderSentences.Count > 0 ? HeaderSentences[0].LineNumber : 0;

            return new Sentence(string.Join(" ", HeaderSentences.Select(s => s.Content)), lineNum);
        }

        public void AppendListElement(List<Sentence> contents, int level)
        {
            if (this.ListBlocks.Count == 0)
            {
                this.ListBlocks.Add(new ListBlock());
            }
            this.ListBlocks.Last().AppendListElement(contents, level);
        }

        public void AppendListBlock()
        {
            this.ListBlocks.Add(new ListBlock());
        }
    }
}
