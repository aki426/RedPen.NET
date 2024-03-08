using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace redpen_core.model
{
    public record class Paragraph
    {
        public List<Sentence> Sentences { get; init; }

        public Paragraph(List<Sentence> sentences)
        {
            this.Sentences = sentences;
        }

        public Paragraph() : this(new List<Sentence>())
        {
        }

        public Sentence GetSentence(int index)
        {
            return this.Sentences[index];
        }

        public Paragraph AppendSentence(string content, int lineNum)
        {
            // MEMO: センテンスは1行に1つというモデルなのか？
            // TODO: Mutableな実装なのでImmutableに書き換えられればそうする。
            this.Sentences.Add(new Sentence(content, lineNum));
            return this;
        }

        public Paragraph AppendSentence(Sentence sentence)
        {
            // MEMO: 追加されたSentenceと既存のSentenceの整合性は？
            this.Sentences.Add(sentence);
            return this;
        }

        public int GetNumberOfSentences()
        {
            return this.Sentences.Count;
        }
    }
}
