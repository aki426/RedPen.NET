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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Lucene.Net.Analysis.Ja;
using Lucene.Net.Analysis.Ja.TokenAttributes;
using Lucene.Net.Analysis.TokenAttributes;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Tokenizer
{
    /// <summary>
    /// Lucene.Net.Analysis.Ja.JapaneseTokenizerを効率良く利用するためのクラス。
    /// NOTE: リソース管理したくなかったのでStatic関数にラップした。
    /// </summary>
    public static class KuromojiController
    {
        /// <summary>
        /// Sentence単発用の形態素解析関数。
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static List<TokenElement> Tokenize(Sentence src)
        {
            if (string.IsNullOrEmpty(src.Content))
            {
                return new List<TokenElement>();
            }

            using var text = new StringReader(src.Content);
            using var tokenizer = new JapaneseTokenizer(
                text,
                null,  // UserDictionaryクラスインスタンスを別途ビルドして与えることでユーザ辞書を追加可能
                false, // 句読点は捨てない
                JapaneseTokenizerMode.NORMAL);

            List<TokenElement> tokens = new List<TokenElement>();

            try
            {
                tokenizer.Reset();
                int currentOffset = 0;
                while (tokenizer.IncrementToken())
                {
                    string surface = tokenizer.GetAttribute<ICharTermAttribute>().ToString();

                    string baseForm = tokenizer.GetAttribute<IBaseFormAttribute>()?.GetBaseForm()?.ToString() ?? "";

                    var read = tokenizer.GetAttribute<IReadingAttribute>();
                    string reading = read?.GetReading() ?? ""; // 英語表現はReadingがNullになる
                    string pronunce = read?.GetPronunciation() ?? ""; // 英語表現はPronunciationがNullになる

                    var inf = tokenizer.GetAttribute<IInflectionAttribute>();
                    string inflectionForm = inf.GetInflectionForm();
                    string inflectionType = inf.GetInflectionType();

                    //tokens.Add(new TokenElement(
                    //    surface,
                    //    tokenizer.GetAttribute<IPartOfSpeechAttribute>().GetPartOfSpeech().Split('-').ToImmutableList(),
                    //    reading,
                    //    Enumerable.Range(currentOffset, surface.Length).Select(i => src.ConvertToLineOffset(i)).ToImmutableList()
                    //));

                    tokens.Add(new TokenElement(
                        surface,
                        reading,
                        pronunce,
                        tokenizer.GetAttribute<IPartOfSpeechAttribute>().GetPartOfSpeech().Split('-').ToImmutableList(),
                        baseForm,
                        inflectionType,
                        inflectionForm,
                        Enumerable.Range(currentOffset, surface.Length).Select(i => src.ConvertToLineOffset(i)).ToImmutableList()
                    ));

                    // iteration.
                    currentOffset += surface.Length;
                }

                tokenizer.End();
            }
            catch (IOException e)
            {
                tokenizer.End();

                // TODO: StackTraceを出力するように変更。
                throw;
            }

            return tokens;
        }

        /// <summary>
        /// Sentenceのリストを一括処理するための形態素解析関数。
        /// </summary>
        /// <param name="srcs"></param>
        /// <returns></returns>
        public static List<List<TokenElement>> Tokenize(IEnumerable<Sentence> srcs)
        {
            List<List<TokenElement>> tokens = new List<List<TokenElement>>();

            // Tokenizerの生成とDisposeを一番外側のusingで管理
            using var tokenizer = new JapaneseTokenizer(
                new StringReader(string.Empty),
                null,
                false,
                JapaneseTokenizerMode.NORMAL);

            foreach (var src in srcs)
            {
                if (string.IsNullOrEmpty(src.Content))
                {
                    tokens.Add(new List<TokenElement>());
                    continue;
                }

                // TextReaderのライフサイクルを明確に管理
                using var textReader = new StringReader(src.Content);
                var subList = new List<TokenElement>();

                try
                {
                    tokenizer.SetReader(textReader);
                    tokenizer.Reset();

                    int currentOffset = 0;
                    while (tokenizer.IncrementToken())
                    {
                        string surface = tokenizer.GetAttribute<ICharTermAttribute>().ToString();

                        string baseForm = tokenizer.GetAttribute<IBaseFormAttribute>()?.GetBaseForm()?.ToString() ?? "";

                        var read = tokenizer.GetAttribute<IReadingAttribute>();
                        string reading = read.GetReading();

                        string pronounce = read.GetPronunciation();

                        var inf = tokenizer.GetAttribute<IInflectionAttribute>();
                        string inflectionForm = inf.GetInflectionForm();
                        string inflectionType = inf.GetInflectionType();

                        subList.Add(new TokenElement(
                            surface,
                            tokenizer.GetAttribute<IPartOfSpeechAttribute>().GetPartOfSpeech().Split('-').ToImmutableList(),
                            reading,
                            Enumerable.Range(currentOffset, surface.Length).Select(i => src.ConvertToLineOffset(i)).ToImmutableList()
                        ));

                        currentOffset += surface.Length;
                    }

                    tokenizer.End();  // Endの呼び出しは維持

                    tokenizer.Dispose(); // Using句があるにもかかわらずDisposeが必要。
                                         // 無いと次のIterationのSetReaderにて例外スローが起きる。

                    tokens.Add(subList);
                }
                catch (IOException e)
                {
                    tokenizer.End();
                    // TODO: エラーログ記録を追加することを推奨
                    //logger?.LogError(e, "Tokenization failed");
                    throw;
                }
            }

            return tokens;
        }
    }
}
