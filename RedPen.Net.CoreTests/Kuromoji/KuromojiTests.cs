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
using System.Diagnostics;
using System.IO;
using System.Linq;
using FluentAssertions;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Ja;
using Lucene.Net.Analysis.Ja.Dict;
using Lucene.Net.Analysis.Ja.TokenAttributes;
using Lucene.Net.Analysis.TokenAttributes;
using RedPen.Net.Core.Model;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.CoreTests.Kuromoji
{
    /// <summary>
    /// KuromojiTokenizerの実装のための、Kuromojiの機能検証用テストケース。
    /// 以下のLucene.Netのコードを参照のこと。
    /// Lucene.Net.Analysis.Ja.JapaneseTokenizer
    /// Lucene.Net.Analysis.Tokenizer
    /// Lucene.Net.Util.AttributeSource
    /// </summary>
    public class KuromojiTests
    {
        private readonly ITestOutputHelper output;

        public KuromojiTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// KuromojiのJapaneseTokenizerを隠ぺいして効率良く使うためのWrapperクラス。
        /// </summary>
        public class KuromojiWrapper
        {
            /// <summary>JapaneseTokenizer</summary>
            public JapaneseTokenizer Tokenizer { get; init; }

            /// <summary>TokenのCharArray表現</summary>
            public ICharTermAttribute CharTerm { get; init; }

            /// <summary>開始と終了オフセット位置</summary>
            public IOffsetAttribute Offset { get; init; }

            /// <summary>Tokenの位置指定にかかわる項目。通常1。日本語には不要？</summary>
            public IPositionIncrementAttribute PositionIncrement { get; init; }

            /// <summary>TokenのPositionの長さに関する項目。グラフ構造の解析に使用するらしい。通常1。日本語には不要？</summary>
            public IPositionLengthAttribute PositionLength { get; init; }

            /// <summary>基本形。品詞によってはNULLになるとのこと。</summary>
            public IBaseFormAttribute BaseForm { get; init; }

            /// <summary>品詞に関する情報。</summary>
            public IPartOfSpeechAttribute PartOfSpeech { get; init; }

            /// <summary>読みと発音。読みと発音は異なる（は＝読み：ハと発音：ワ）。</summary>
            public IReadingAttribute Reading { get; init; }

            /// <summary>「語形変化」または「屈折」。日本語では活用形と考えられる。IPAdicにはデータがない？</summary>
            public IInflectionAttribute Inflection { get; init; }

            public KuromojiWrapper()
            {
                // kuromojiはデフォルトコンストラクタから呼び出せる。
                Tokenizer = new(
                    new StringReader(string.Empty), // 空のTextReaderとして暫定的に与える。本当に処理したいものはSetReaderで与える。
                    null, // UserDictionaryクラスインスタンスを別途ビルドして与えることでユーザ辞書を追加可能。
                    false, // 句読点は捨てない。
                    JapaneseTokenizerMode.NORMAL);

                CharTerm = Tokenizer.GetAttribute<ICharTermAttribute>();
                Offset = Tokenizer.GetAttribute<IOffsetAttribute>();
                PositionIncrement = Tokenizer.GetAttribute<IPositionIncrementAttribute>();
                PositionLength = Tokenizer.GetAttribute<IPositionLengthAttribute>();
                BaseForm = Tokenizer.GetAttribute<IBaseFormAttribute>();
                PartOfSpeech = Tokenizer.GetAttribute<IPartOfSpeechAttribute>();
                Reading = Tokenizer.GetAttribute<IReadingAttribute>();
                Inflection = Tokenizer.GetAttribute<IInflectionAttribute>();
            }

            /// <summary>
            /// 形態素解析関数。
            /// </summary>
            /// <param name="src"></param>
            /// <returns></returns>
            public List<TokenElement> Tokenize(Sentence src)
            {
                List<TokenElement> tokens = new List<TokenElement>();

                if (src.Content == null || src.Content == string.Empty)
                {
                    return tokens;
                }

                using (TextReader text = new StringReader(src.Content))
                {
                    Tokenizer.SetReader(text);
                    Tokenizer.Reset();

                    try
                    {
                        int currentOffset = 0;
                        while (Tokenizer.IncrementToken())
                        {
                            string surface = CharTerm.ToString();
                            string baseForm = BaseForm?.GetBaseForm()?.ToString() ?? "";
                            string reading = Reading.GetReading();
                            string pronounce = Reading.GetPronunciation();
                            string inflectionForm = Inflection.GetInflectionForm();
                            string inflectionType = Inflection.GetInflectionType();

                            tokens.Add(new TokenElement(
                                surface,
                                PartOfSpeech.GetPartOfSpeech().Split('-').ToImmutableList(),
                                reading,
                                Enumerable.Range(currentOffset, surface.Length).Select(i => src.ConvertToLineOffset(i)).ToImmutableList()
                            ));

                            // iteration.
                            currentOffset += surface.Length;
                        }

                        Tokenizer.End();
                    }
                    catch (IOException e)
                    {
                        Tokenizer.End();

                        // TODO: StackTraceを出力するように変更。
                        throw;
                    }
                    finally
                    {
                        Tokenizer.Dispose();
                    }
                }

                return tokens;
            }
        }

        public class TokenStreamWrapper
        {
            public static List<TokenElement> Tokenize(Sentence src)
            {
                List<TokenElement> tokens = new List<TokenElement>();

                if (src.Content == null || src.Content == string.Empty)
                {
                    return tokens;
                }

                using (var reader = new StringReader(src.Content))
                {
                    Tokenizer tokenizer = new JapaneseTokenizer(reader, null, false, JapaneseTokenizerMode.NORMAL);
                    var tokenStreamComponents = new TokenStreamComponents(tokenizer, tokenizer);
                    using (var tokenStream = tokenStreamComponents.TokenStream)
                    {
                        // NOTE: 処理の実行前にTextReader inputを反映させるためResetを実行する必要がある。
                        tokenStream.Reset();

                        int currentOffset = 0;
                        while (tokenStream.IncrementToken())
                        {
                            string surface = tokenStream.GetAttribute<ICharTermAttribute>().ToString();
                            //string baseForm = BaseForm?.GetBaseForm()?.ToString() ?? "";
                            string baseForm = tokenStream.GetAttribute<IBaseFormAttribute>()?.GetBaseForm()?.ToString() ?? "";
                            var read = tokenStream.GetAttribute<IReadingAttribute>();
                            string reading = read.GetReading();
                            string pronounce = read.GetPronunciation();
                            var inf = tokenStream.GetAttribute<IInflectionAttribute>();
                            string inflectionForm = inf.GetInflectionForm();
                            string inflectionType = inf.GetInflectionType();

                            tokens.Add(new TokenElement(
                                surface,
                                tokenStream.GetAttribute<IPartOfSpeechAttribute>().GetPartOfSpeech().Split('-').ToImmutableList(),
                                reading,
                                Enumerable.Range(currentOffset, surface.Length).Select(i => src.ConvertToLineOffset(i)).ToImmutableList()
                            ));

                            // iteration.
                            currentOffset += surface.Length;
                        }
                    }
                }

                return tokens;
            }
        }

        public static class KuromojiController
        {
            /// <summary>
            /// 形態素解析関数。
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
                        string reading = read.GetReading();

                        string pronunce = read.GetPronunciation();

                        var inf = tokenizer.GetAttribute<IInflectionAttribute>();
                        string inflectionForm = inf.GetInflectionForm();
                        string inflectionType = inf.GetInflectionType();

                        tokens.Add(new TokenElement(
                            surface,
                            tokenizer.GetAttribute<IPartOfSpeechAttribute>().GetPartOfSpeech().Split('-').ToImmutableList(),
                            reading,
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

        [Fact]
        public void Tokenize吾輩は猫であるTest2()
        {
            // Arrange
            string currentDirectory = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(currentDirectory, "Tokenizer", "DATA", "neko.txt");

            // ファイルの中身が空でないことも確認する場合
            FileInfo file = new FileInfo(filePath);
            file.Exists.Should().BeTrue("テストデータ「neko.txt」が存在しません。Download-WagahaiHaNekoDearu.ps1を実行してください。");
            file.Length.Should().NotBe(0, "ファイルが空です");

            // Act
            var fileContents = File.ReadAllText(filePath);

            var sw = new Stopwatch();
            sw.Start();

            var kuromojiWrapper = new KuromojiWrapper();
            List<TokenElement> tokens = kuromojiWrapper.Tokenize(new Sentence(fileContents, 1));

            sw.Stop();

            // Assert
            // 実行環境によるが30万字1500ms以下をスレショルドとした。
            sw.ElapsedMilliseconds.Should().BeLessThan(1500);

            // Token目視。
            output.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds}ms");

            // NOTE: 以下、全Tokenの表示。非常に長い時間がかかるので回帰テストには入れない方が良いがデバッグ出力としてコメントアウトして残す。。
            foreach (var token in tokens.Take(100))
            {
                output.WriteLine(token.ToString());
            }
        }

        // NOTE: 改善前は1SentenceごとにJapaneseTokenizer.Dispose()を呼んでいたが、Lucene.NETのテストコードを参照し、
        // JapaneseTokenizer.End()を呼ぶようにした。Disposeはエラー発生時のFinallyでコールする。

        // NOTE: 改善後は改善前より100ms程度若干速くなっている。
        // 3パターン一度に実行した場合、呼び出し順によってなぜか処理時間が変化する。
        //[RESULT 1(改善後)]Elapsed time: 613ms★
        //[RESULT 2(改善前)] Elapsed time: 847ms
        //[RESULT 3(TokenStream)] Elapsed time: 653ms

        //[RESULT 3(TokenStream)]Elapsed time: 776ms
        //[RESULT 1(改善後)] Elapsed time: 683ms
        //[RESULT 2(改善前)] Elapsed time: 640ms★

        //[RESULT 2(改善前)]Elapsed time: 763ms
        //[RESULT 3(TokenStream)] Elapsed time: 760ms
        //[RESULT 1(改善後)] Elapsed time: 577ms★

        // 単独実行では+100msくらい増える。
        //[RESULT 1(改善後)]Elapsed time: 792ms
        // 改善前、TokenStreamでも同じ。
        //[RESULT 2(改善前)]Elapsed time: 1053ms
        //[RESULT 3(TokenStream)]Elapsed time: 931ms

        // 呼び出し方によって差があるのでよくわからない部分はあるが、改善後が最も速い。

        // さらにJapaneseTokenizerをUsingで管理するコードを追加して比較。
        //[RESULT 1(改善後)]Elapsed time: 899ms
        //[RESULT 4(Using句を使った実装)] Elapsed time: 1014ms
        //[RESULT 5(Using句を使った実装2)] Elapsed time: 657ms
        // SentenceごとにJapaneseTokenizerをUsingで解放するためRESULT4は+100ms遅くなる。
        // 一方全SentenceでJapaneseTokenizerを1回しかNewしないResult5は-200msほど速くなる。
        // Tokenizeの処理負荷はほとんど問題にならないがUsing句を使った実装を採用する。

        [Fact]
        public void 既存実装との速度比較()
        {
            // Arrange
            string currentDirectory = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(currentDirectory, "Tokenizer", "DATA", "neko.txt");

            // ファイルの中身が空でないことも確認する場合
            FileInfo file = new FileInfo(filePath);
            file.Exists.Should().BeTrue("テストデータ「neko.txt」が存在しません。Download-WagahaiHaNekoDearu.ps1を実行してください。");
            file.Length.Should().NotBe(0, "ファイルが空です");

            // Act
            var fileContents = File.ReadAllText(filePath);
            //var sentences = fileContents.Split('\n').Select((item, index) => new Sentence(item, index)).Take(10000);
            var sentences = fileContents.Split('\n').Select((item, index) => new Sentence(item, index));

            var sw = new Stopwatch();

            // 挙動の安定のためダミーを用意。
            //List<TokenElement> dummy = new();
            //KuromojiTokenizer dummyTokenizer = new();
            //sw.Restart();
            //foreach (var item in sentences.Take(100))
            //{
            //    dummy.AddRange(dummyTokenizer.Tokenize(item));
            //}
            //sw.Stop();

            // 改善後計測
            List<TokenElement> result1 = new();
            KuromojiWrapper kuromojiWrapper = new();

            sw.Restart();

            foreach (var item in sentences)
            {
                result1.AddRange(kuromojiWrapper.Tokenize(item));
            }

            sw.Stop();
            kuromojiWrapper.Tokenizer.Dispose();
            output.WriteLine($"[RESULT 1(改善後)]Elapsed time: {sw.ElapsedMilliseconds}ms");

            // 改善前の既存実装計測
            //List<TokenElement> result2 = new();
            //KuromojiTokenizer tokenizer = new();

            //sw.Restart();

            //foreach (var item in sentences)
            //{
            //    result2.AddRange(tokenizer.Tokenize(item));
            //}

            //sw.Stop();
            //output.WriteLine($"[RESULT 2(改善前)]Elapsed time: {sw.ElapsedMilliseconds}ms");

            //// TokenStreamを使った実装
            //List<TokenElement> result3 = new();
            //KuromojiTokenizer tokenStream = new();

            //sw.Restart();

            //foreach (var item in sentences)
            //{
            //    result3.AddRange(tokenStream.Tokenize(item));
            //}

            //sw.Stop();
            //output.WriteLine($"[RESULT 3(TokenStream)]Elapsed time: {sw.ElapsedMilliseconds}ms");

            // Using句を使った実装
            List<TokenElement> result4 = new();

            sw.Restart();

            foreach (var item in sentences)
            {
                result4.AddRange(KuromojiController.Tokenize(item));
            }

            sw.Stop();
            output.WriteLine($"[RESULT 4(Using句を使った実装)]Elapsed time: {sw.ElapsedMilliseconds}ms");

            // Using句を使った実装2
            sw.Restart();

            List<List<TokenElement>> listlist = KuromojiController.Tokenize(sentences);

            sw.Stop();
            output.WriteLine($"[RESULT 5(Using句を使った実装2)]Elapsed time: {sw.ElapsedMilliseconds}ms");

            // Assert
            // nop

            // Token目視。

            // NOTE: 以下、全Tokenの表示。非常に長い時間がかかるので回帰テストには入れない方が良いがデバッグ出力としてコメントアウトして残す。。
            output.WriteLine("================ RESULT 1 ================");
            foreach (var token in result1.Take(20))
            {
                output.WriteLine(token.ToString());
            }

            //output.WriteLine("================ RESULT 2 ================");
            //foreach (var token in result2.Take(20))
            //{
            //    output.WriteLine(token.ToString());
            //}

            //output.WriteLine("================ RESULT 3 ================");
            //foreach (var token in result3.Take(20))
            //{
            //    output.WriteLine(token.ToString());
            //}

            output.WriteLine("================ RESULT 4 ================");
            foreach (var token in result4.Take(20))
            {
                output.WriteLine(token.ToString());
            }

            output.WriteLine("================ RESULT 5 ================");
            foreach (var token in listlist.SelectMany(x => x).Take(20))
            {
                output.WriteLine(token.ToString());
            }
        }

        [Theory]
        [InlineData("001", "これは山です。これは川だ。")]
        [InlineData("002", "これはテストですが、あれは本番です。")]
        [InlineData("003", "読まない。読もう。読む。読むとき。読めばいい。読め。")]
        [InlineData("004", "読みません。読みます。読みますとき。読みなさい。")]
        [InlineData("005", "広瀬すずは女優です")]
        [InlineData("006", @"
笑わない

笑いません

笑わなかった

笑いませんでした

笑おう

笑いましょう

笑う

笑います

笑った

笑いました

笑いだ

笑いです

笑いだった

笑いでした

笑いである

笑いです

笑いであった

笑いでした

笑うとき

笑ったとき

笑えば

笑え

笑いなさい

笑えよ

笑いなさいよ

笑われる

笑われます

笑われた

笑われました

笑える

笑えます

笑えた

笑えました

笑わせる

笑わせます

笑わせた

笑わせました

笑わさせる

笑わさせます

笑わさせた

笑わさせました

笑わぬ

笑いませぬ

笑わん

笑いません

笑おう

笑いましょう

笑うまい

笑いますまい

笑いたい

笑いたいです

笑いたかった

笑いたかったです

笑いたがる

笑いたがります

笑いたがった

笑いたがりました

笑いそうだ

笑いそうです

笑いそうだった

笑いそうでした

笑うようだ

笑うようです

笑うようだった

笑うようでした

笑うらしい

笑うらしいです

笑うらしかった

笑うらしかったです

笑っている

笑っています

笑っていた

笑っていました")]
        public void BasicUseTest(string nouse1, string text)
        {
            // kuromojiはデフォルトコンストラクタから呼び出せる。
            JapaneseTokenizer tokenizer = new(
                new StringReader(string.Empty), // 空のTextReaderとして暫定的に与える。本当に処理したいものはSetReaderで与える。
                null, // UserDictionaryクラスインスタンスを別途ビルドして与えることでユーザ辞書を追加可能。
                false, // 句読点は捨てない。
                JapaneseTokenizerMode.NORMAL);

            // Kuromojiは以下のAttributeを持ち、デフォルトコンストラクタ内ですべてセットされるので改めてセットしなくてよい。

            //this.termAtt = AddAttribute<ICharTermAttribute>();
            //this.offsetAtt = AddAttribute<IOffsetAttribute>();
            //this.posIncAtt = AddAttribute<IPositionIncrementAttribute>();
            //this.posLengthAtt = AddAttribute<IPositionLengthAttribute>();
            //this.basicFormAtt = AddAttribute<IBaseFormAttribute>();
            //this.posAtt = AddAttribute<IPartOfSpeechAttribute>();
            //this.readingAtt = AddAttribute<IReadingAttribute>();
            //this.inflectionAtt = AddAttribute<IInflectionAttribute>();

            // TokenのCharArray表現
            ICharTermAttribute charAttr = tokenizer.GetAttribute<ICharTermAttribute>();
            // 開始と終了オフセット位置
            IOffsetAttribute offsetAttr = tokenizer.GetAttribute<IOffsetAttribute>();
            // Tokenの位置指定にかかわる項目。通常1。日本語には不要？
            IPositionIncrementAttribute posIncrAttr = tokenizer.GetAttribute<IPositionIncrementAttribute>();
            // これもTikenのPositionの長さに関する項目でグラフ構造の解析に使用するらしい。通常1。日本語には不要？
            IPositionLengthAttribute posLenAttr = tokenizer.GetAttribute<IPositionLengthAttribute>();
            // 基本形を表すものと思われる。品詞によってはNULLになるとのこと。
            IBaseFormAttribute baseAttr = tokenizer.GetAttribute<IBaseFormAttribute>();
            // 品詞に関する情報。
            IPartOfSpeechAttribute POSAttr = tokenizer.GetAttribute<IPartOfSpeechAttribute>();
            // 読みと発音。読みと発音は異なる（は＝読み：ハと発音：ワ）。
            IReadingAttribute readAttr = tokenizer.GetAttribute<IReadingAttribute>();
            // 「語形変化」または「屈折」。日本語では活用形と考えられる。IPAdicにはデータがない？
            IInflectionAttribute inflectionAttr = tokenizer.GetAttribute<IInflectionAttribute>();

            // コンストラクタで与えなかった場合、もしくは繰り返し実行の場合はTextReaderを与える。
            tokenizer.SetReader(new StringReader(text));

            // ResetによってTokenize対象TextReaderのリフレッシュが行われる。
            tokenizer.Reset();

            // Tokenを1つずつ取得する。
            int currentOffset = 0;
            while (tokenizer.IncrementToken())
            {
                string surface = charAttr.ToString();

                int start = offsetAttr.StartOffset;
                int end = offsetAttr.EndOffset;

                //posIncrAttr.PositionIncrement;

                //posLenAttr.PositionLength;

                string baseForm = baseAttr?.GetBaseForm()?.ToString() ?? "";

                string pos = POSAttr.GetPartOfSpeech();

                string reading = readAttr.GetReading();
                string pronounce = readAttr.GetPronunciation();

                string inflectionForm = inflectionAttr.GetInflectionForm();
                string inflectionType = inflectionAttr.GetInflectionType();

                output.WriteLine($"{surface},\t{start}-{end},\t{baseForm},\t{pos},\t{reading}({pronounce}),\t{inflectionForm},\t{inflectionType}");

                // iteration.
                currentOffset += surface.Length;
            }
        }

        /// <summary>
        /// ユーザ辞書使用
        /// </summary>
        [Fact]
        public void UserDictTest()
        {
            const string s = "ここは関西国際空港です。";
            output.WriteLine($"対象の文字列:{s}");

            using (var reader = new StringReader(s))
            {
                Tokenizer tokenizer = new JapaneseTokenizer(reader, ReadDict(), false, JapaneseTokenizerMode.NORMAL);
                var tokenStreamComponents = new TokenStreamComponents(tokenizer, tokenizer);
                using (var tokenStream = tokenStreamComponents.TokenStream)
                {
                    // NOTE: 処理の実行前にTextReader inputを反映させるためResetを実行する必要がある。
                    tokenStream.Reset();

                    while (tokenStream.IncrementToken())
                    {
                        output.WriteLine("---");
                        output.WriteLine(
                            $"ICharTermAttribute=>{tokenStream.GetAttribute<ICharTermAttribute>().ToString()}");

                        output.WriteLine(
                            $"ITermToBytesRefAttribute#BytesRef=>{tokenStream.GetAttribute<ITermToBytesRefAttribute>().BytesRef}");

                        output.WriteLine(
                            $"IOffsetAttribute#StartOffset=>{tokenStream.GetAttribute<IOffsetAttribute>().StartOffset}");
                        output.WriteLine(
                            $"IOffsetAttribute#EndOffset=>{tokenStream.GetAttribute<IOffsetAttribute>().EndOffset}");

                        output.WriteLine(
                            $"IPositionIncrementAttribute=>{tokenStream.GetAttribute<IPositionIncrementAttribute>().PositionIncrement}");
                        output.WriteLine(
                            $"IPositionLengthAttribute=>{tokenStream.GetAttribute<IPositionLengthAttribute>().PositionLength}");

                        output.WriteLine(
                            $"IBaseFormAttribute#GetBaseForm=>{tokenStream.GetAttribute<IBaseFormAttribute>().GetBaseForm()}");

                        output.WriteLine(
                            $"IPartOfSpeechAttribute#GetPartOfSpeech=>{tokenStream.GetAttribute<IPartOfSpeechAttribute>().GetPartOfSpeech()}");

                        output.WriteLine(
                            $"IReadingAttribute#GetReading=>{tokenStream.GetAttribute<IReadingAttribute>().GetReading()}");
                        output.WriteLine(
                            $"IReadingAttribute#GetPronunciation=>{tokenStream.GetAttribute<IReadingAttribute>().GetPronunciation()}");

                        output.WriteLine(
                            $"IInflectionAttribute#GetInflectionForm=>{tokenStream.GetAttribute<IInflectionAttribute>().GetInflectionForm()}");
                        output.WriteLine(
                            $"IInflectionAttribute#GetInflectionType=>{tokenStream.GetAttribute<IInflectionAttribute>().GetInflectionType()}");

                        output.WriteLine("---");
                    }
                }
            }
        }

        public static UserDictionary ReadDict()
        {
            string dict_str = @"
# Custom segmentation for long entries
日本経済新聞,日本 経済 新聞,ニホン ケイザイ シンブン,カスタム名詞
関西国際空港,関西 国際 空港,カンサイ コクサイ クウコウ,テスト名詞

# Custom reading for sumo wrestler
朝青龍,朝青龍,アサショウリュウ,カスタム人名

# Silly entry:
abcd,a b cd,foo1 foo2 foo3,bar
abcdefg,ab cd efg,foo1 foo2 foo4,bar
";

            //TextReader reader = new StreamReader("userdict.txt", Encoding.UTF8);
            TextReader reader = new StringReader(dict_str);
            return new UserDictionary(reader);
        }
    }
}
