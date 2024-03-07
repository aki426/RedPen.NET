﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using redpen_core.parser;
using redpen_core.tokenizer;

namespace redpen_core.parser
{
    public interface IDocumentParser
    {
        //Document Parse(string content, SentenceExtractor sentenceExtractor, RedPenTokenizer tokenizer);

        //        /**
        //    * Given input stream, return Document instance from a stream.
        //    *
        //    * @param is                input stream containing input content
        //    * @param sentenceExtractor SentenceExtractor object
        //    * @param tokenizer         tokenizer
        //    * @return a generated file content
        //    * @throws cc.redpen.RedPenException if Parser failed to parse input.
        //    */
        //        Document parse(InputStream is, SentenceExtractor sentenceExtractor, RedPenTokenizer tokenizer)
        //        throws RedPenException;

        //        /**
        //         * Given content, return Document instance for the specified file.
        //         *
        //         * @param content           input content
        //         * @param sentenceExtractor SentenceExtractor object
        //         * @param tokenizer         tokenizer
        //         * @return a generated file content
        //         * @throws cc.redpen.RedPenException if Parser failed to parse input.
        //         */
        //        Document parse(String content, SentenceExtractor sentenceExtractor, RedPenTokenizer tokenizer)
        //        throws RedPenException;

        //        /**
        //         * Given input file name, return Document instance for the specified file.
        //         *
        //         * @param file              input file
        //         * @param sentenceExtractor SentenceExtractor object
        //         * @param tokenizer         tokenizer
        //         * @return a generated file content
        //         * @throws cc.redpen.RedPenException if Parser failed to parse input.
        //         */
        //        Document parse(File file, SentenceExtractor sentenceExtractor, RedPenTokenizer tokenizer)
        //        throws RedPenException;

        //        DocumentParser PLAIN = new PlainTextParser();
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
