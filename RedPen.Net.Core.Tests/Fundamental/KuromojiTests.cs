using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Ja;
using Lucene.Net.Analysis.Ja.Dict;
using Lucene.Net.Analysis.Ja.TokenAttributes;
using Lucene.Net.Analysis.TokenAttributes;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Fundamental
{
    public class KuromojiTests
    {
        private readonly ITestOutputHelper output;

        public KuromojiTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact(DisplayName = "Kuromojiの基本動作確認", Skip = "Kuromoji動作確認のためのTempTest")]
        public void MyTestMethod()
        {
            // 解析対象文
            const string text = ".NETの機械学習もかなり実用的になってきた。";

            using (var reader = new StringReader(text))
            using (var tokenizer = new JapaneseTokenizer(reader, GetUserDictionary(), false, JapaneseTokenizerMode.NORMAL))
            {
                var tsc = new TokenStreamComponents(tokenizer);

                var ts = tsc.TokenStream;

                ts.Reset();

                while (ts.IncrementToken())
                {
                    // 用語
                    var term = ts.GetAttribute<ICharTermAttribute>();
                    // 品詞
                    var partOfSpeech = ts.AddAttribute<IPartOfSpeechAttribute>();
                    // 読み
                    var reading = ts.AddAttribute<IReadingAttribute>();

                    //Console.WriteLine($"{term.ToString()} {reading.GetReading()} {partOfSpeech.GetPartOfSpeech()}");
                    output.WriteLine($"{term.ToString()} {reading.GetReading()} {partOfSpeech.GetPartOfSpeech()}");
                }
            }
        }

        // ユーザー辞書作成
        private static UserDictionary GetUserDictionary()
        {
            const string dic =
        @"
.NET,.NET,ドットネット,カスタム名詞
機械学習,機械学習,キカイガクシュウ,カスタム名詞
";

            using (var reader = new StringReader(dic))
            {
                return new UserDictionary(reader);
            }
        }
    }
}