using RedPen.Net.Core.Parser;
using RedPen.Net.Core.Tokenizer;
using System.Collections.Generic;
using System.Linq;

namespace RedPen.Net.Core.Model
{
    public record Document
    {
        // MEMO: 何のため？
        //private static readonly long serialVersionUID = 1628589004095293831L;

        public List<Section> Sections { get; init; }

        public string? FileName { get; init; } // TODO: このプロパティは必要？　Documentモデル自身が元ファイル名を知っている必要ある？

        public HashSet<PreprocessorRule> PreprocessorRules { get; init; }

        public Document(
            List<Section> sections,
            string? fileName,
            HashSet<PreprocessorRule> preprocessorRules)
        {
            Sections = sections;
            FileName = fileName;
            PreprocessorRules = preprocessorRules;
        }

        public Section? GetLastSection()
        {
            if (Sections.Count <= 0)
            {
                return null;
            }

            return Sections.Last();
        }

        /// <summary>
        /// Document内の全Sentenceを取得する関数。
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public List<Sentence> GetAllSentences()
        {
            // 全SectionからSentenceを抽出する。
            List<Sentence> sentences = new List<Sentence>();
            foreach (Section section in this.Sections)
            {
                // ParagraphsからSentenceを抽出する。
                sentences.AddRange(section.Paragraphs.SelectMany(i => i.Sentences));
                // TODO: 先にParagraphからSentenceを抽出しているのはなぜ？　順番は関係ない？
                sentences.AddRange(section.HeaderSentences);
                // ListBlocksからSentenceを抽出する。
                sentences.AddRange(section.ListBlocks.SelectMany(i => i.ListElements.SelectMany(j => j.Sentences)));
            }

            return sentences;
        }

        public static DocumentBuilder Builder()
        {
            return new DocumentBuilder();
        }

        public static DocumentBuilder Builder(IRedPenTokenizer tokenizer)
        {
            return new DocumentBuilder(tokenizer);
        }
    }
}
