using System.Collections.Generic;
using System.Linq;
using RedPen.Net.Core.Parser;
using RedPen.Net.Core.Tokenizer;

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
        public static List<Sentence> GetAllSentencesRecursive(List<Section> sections)
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

        /// <summary>
        /// センテンスのリストを、ドキュメント構造上のまとまりごとに抽出する。
        /// MEMO: HeaderSentence、Paragraph、ListElement内のSentenceリストを出現順に取得することを意図している。
        /// </summary>
        /// <returns>A list of List.</returns>
        public List<List<Sentence>> GetAllSentencesByDocumentStructure()
        {
            return GetSentencesByStructureRecursive(this.Sections);
        }

        /// <summary>
        /// センテンスのリストを、ドキュメント構造上のまとまりごとに再帰で抽出する。
        /// MEMO: HeaderSentence、Paragraph、ListElement内のSentenceリストを出現順に取得することを意図している。
        /// </summary>
        /// <param name="sections"></param>
        /// <returns></returns>
        public static List<List<Sentence>> GetSentencesByStructureRecursive(List<Section> sections)
        {
            List<List<Sentence>> sentences = new List<List<Sentence>>();
            foreach (Section section in sections)
            {
                // ヘッダーは複数センテンスを持つ。
                sentences.Add(section.HeaderSentences);
                // パラグラフ1つにつき複数センテンスを持つ。
                sentences.AddRange(section.Paragraphs.Select(i => i.Sentences).ToList());
                // リストブロック1:リストエレメント多:センテンス多、という構造。
                sentences.AddRange(section.ListBlocks.SelectMany(block => block.ListElements.Select(ele => ele.Sentences)));

                // サブセクションに再帰。
                sentences.AddRange(GetSentencesByStructureRecursive(section.Subsections));
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
