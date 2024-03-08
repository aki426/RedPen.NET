namespace redpen_core.model
{
    public record class ListElement
    {
        public List<Sentence> Sentences { get; init; }
        public int Level { get; init; }

        public ListElement(List<Sentence> sentences, int level)
        {
            Sentences = sentences;
            Level = level;
        }

        public Sentence GetSentence(int index)
        {
            return this.Sentences[index];
        }

        public int GetNumberOfSentences()
        {
            return this.Sentences.Count;
        }
    }
}
