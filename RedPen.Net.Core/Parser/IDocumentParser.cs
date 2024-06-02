//   Copyright (c) 2024 KANEDA Akihiro <taoist.aki@gmail.com>
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

// using System.Reflection.Metadata;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;
using System.IO;

namespace RedPen.Net.Core.Parser
{
    // TODO: C#では存在意義が無いので削除する。
    // そもそもすべてのParserはBaseDocumentParserを継承しており、BaseDocumentParserにParse関数があるため
    // IDocumentParserが無くても言語仕様上同じ機能を果たす。
    // また、IDocumentParserがあるためにVSでParse関数の定義／参照箇所を検索した際に実装のないIDocumentParserがヒットしてしまう。

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
