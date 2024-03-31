// using System.Reflection.Metadata;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;
using System.IO;

namespace RedPen.Net.Core.Parser
{
    public interface IDocumentParser
    {
        Document Parse(Stream inputStream, SentenceExtractor sentenceExtractor, IRedPenTokenizer tokenizer);

        Document Parse(string content, SentenceExtractor sentenceExtractor, IRedPenTokenizer tokenizer);

        Document Parse(FileInfo file, SentenceExtractor sentenceExtractor, IRedPenTokenizer tokenizer);

        //IDocumentParser PLAIN = new PlainTextParser();
        //        DocumentParser WIKI = new WikiParser();
        //        DocumentParser MARKDOWN = new MarkdownParser();
        //        DocumentParser LATEX = new LaTeXParser();
        //        DocumentParser ASCIIDOC = new AsciiDocParser();
        //        DocumentParser PROPERTIES = new PropertiesParser();
        //        DocumentParser REVIEW = new ReVIEWParser();
        //        DocumentParser REST = new ReSTParser();

        //        Map<String, DocumentParser> PARSER_MAP = Collections.unmodifiableMap(
        //            new HashMap<String, DocumentParser>() {
        //            {
        //                put("PLAIN", PLAIN);
        //        put("WIKI", WIKI);
        //        put("MARKDOWN", MARKDOWN);
        //        put("LATEX", LATEX);
        //        put("ASCIIDOC", ASCIIDOC);
        //        put("PROPERTIES", PROPERTIES);
        //        put("REVIEW", REVIEW);
        //        put("REST", REST);
        //    }
        //});

        //static DocumentParser of(String parserType)
        //{
        //    DocumentParser parser = PARSER_MAP.get(parserType.toUpperCase());
        //    if (parser == null)
        //    {
        //        throw new IllegalArgumentException("no such parser for :" + parserType);
        //    }
        //    return parser;
        //}
    }
}
