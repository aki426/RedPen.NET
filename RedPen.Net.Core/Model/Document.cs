using Lucene.Net.Util;
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
                // TODO: 暫定的にHeaderSentence -> Paragraph -> ListBlockの順でSentenceを取得するが、
                // PlainTextParser以外のParserを使用した場合にもこの順番で良いか検証する必要がある。。
                sentences.AddRange(section.HeaderSentences);
                sentences.AddRange(section.Paragraphs.SelectMany(i => i.Sentences));
                sentences.AddRange(section.ListBlocks.SelectMany(i => i.ListElements.SelectMany(j => j.Sentences)));
            }

            return sentences;
        }

        // TODO: Sectionに対してはSubsectionsがあるので再帰的にSentenceを取得する必要がある。

        /// <summary>
        /// Sectionのリストから、Sectionの内容とさらにそのSubsectionsの内容を再帰的に取得する関数。
        /// </summary>
        /// <param name="sections">The sections.</param>
        /// <returns>A list of Sentences.</returns>
        public List<Sentence> GetAllSentencesRecursive(List<Section> sections)
        {
            // 全SectionからSentenceを抽出する。
            List<Sentence> sentences = new List<Sentence>();
            foreach (Section section in sections)
            {
                // TODO: 暫定的にHeaderSentence -> Paragraph -> ListBlockの順でSentenceを取得するが、
                // PlainTextParser以外のParserを使用した場合にもこの順番で良いか検証する必要がある。。
                sentences.AddRange(section.HeaderSentences);
                sentences.AddRange(section.Paragraphs.SelectMany(i => i.Sentences));
                sentences.AddRange(section.ListBlocks.SelectMany(i => i.ListElements.SelectMany(j => j.Sentences)));

                // 再帰的にSubsectionからもSentenceを取得する。
                sentences.AddRange(GetAllSentencesRecursive(section.Subsections));
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
