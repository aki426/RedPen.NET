using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.DocumentValidator
{
    // MEMO: Configurationの定義は短いのでValidatorファイル内に併記する。

    /// <summary>JapaneseExpressionVariationのConfiguration</summary>
    public record JapaneseWordVariationConfiguration : ValidatorConfiguration, IWordMapConfigParameter
    {
        public Dictionary<string, string> WordMap { get; init; }

        public JapaneseWordVariationConfiguration(ValidationLevel level, Dictionary<string, string> wordMap) : base(level)
        {
            WordMap = wordMap;
        }
    }

    /// <summary>日本語の表記ゆれを検出するValidator。</summary>
    public class JapaneseWordVariationValidator : Validator, IDocumentValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidationConfiguration</summary>
        public JapaneseWordVariationConfiguration Config { get; init; }

        // MEMO: 一旦、複数Documentをまたいだゆらぎ表現検出は対応しないものとする。
        // 1Documentにつき1Validationを原則とする。

        /// <summary>ドキュメント内のすべてのTokenの正規化されたReadingをキーとしたマップ</summary>
        public Dictionary<string, List<TokenElement>> readingMap;

        /// <summary>あらかじめ定義されたゆらぎ検出のためのReadingのMap。JapaneseExpressionVariationConfiguration.WordMapを用いる。</summary>
        private Dictionary<string, string> spellingVariationMap => Config.WordMap;

        /// <summary>サポート対象言語はja-JPのみ。</summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

        // MEMO: デフォルトリソースからの辞書データ読み込みはJapaneseExpressionVariationConfiguration生成時に行う。
        // ValidatorはJapaneseExpressionVariationConfigurationの設定情報のみを用いて処理を行う。

        /// <summary>
        /// Initializes a new instance of the <see cref="JapaneseWordVariationValidator"/> class.
        /// </summary>
        public JapaneseWordVariationValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            JapaneseWordVariationConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;
            this.readingMap = new Dictionary<string, List<TokenElement>>();
        }

        /// <summary>現在Validatorが持つReadingMapを初期化する。</summary>
        private void Init()
        {
            if (this.readingMap == null || this.readingMap.Any())
            {
                this.readingMap = new Dictionary<string, List<TokenElement>>();
            }
        }

        /// <summary>
        /// すべてのTokenを抽出する。
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        private void UpdateReadingMap(Sentence sentence)
        {
            // 名詞のみ収集するためのリスト。
            List<TokenElement> nouns = new List<TokenElement>();

            foreach (TokenElement token in sentence.Tokens)
            {
                // 半角スペーススキップ。
                if (token.Surface.Equals(" "))
                {
                    continue;
                }

                // tokenの正規化された読みであるreadingを取得し、readingMapに登録する。
                string reading = GetNormalizedReading(token);
                if (!this.readingMap.ContainsKey(reading))
                {
                    this.readingMap[reading] = new List<TokenElement>();
                }
                // reading->tokenの関係を登録。
                this.readingMap[reading].Add(token);

                // 名詞を収集。
                if (token.Tags[0].Equals("名詞"))
                {
                    nouns.Add(token);
                }
                else
                {
                    // 名詞ではない場合で、nounsが2以上の場合、それは複合名詞がnouns内に格納されておりかつ複合名詞の区切りということ。
                    if (nouns.Count > 1)
                    {
                        // 複合名詞として追加登録する。これはNeologdのTokenizeルールによって分割された単語を結合する働きをする。
                        // TODO: Sentenceが体言止めで終わっておりかつFullStopが無い場合、Sentenceの終わりでこのケースに落ちてこないので
                        // 名詞登録処理がされないケースが想定される。要チェック。

                        var compoundNounToken = new TokenElement(
                            string.Join("", nouns.Select(i => i.Surface)),
                            nouns[0].Tags.ToImmutableList(),
                            string.Join("", nouns.Select(i => GetNormalizedReading(i))),
                            nouns.SelectMany(n => n.OffsetMap).ToImmutableList()
                        );

                        // 読みを正規化してからreadingMapに登録する。
                        string compoundReading = GetNormalizedReading(compoundNounToken);
                        if (!this.readingMap.ContainsKey(compoundReading))
                        {
                            this.readingMap[compoundReading] = new List<TokenElement>();
                        }
                        this.readingMap[compoundReading].Add(compoundNounToken);
                    }

                    // 名詞意外の品詞が来たら、そこで名詞の連続は切れているのでクリアする。
                    // つまり複合名詞は最長のもの1つが登録されるし、たかだか1個の名詞のあと他品詞が来た場合は複合名詞化はされない。
                    if (nouns.Any()) { nouns.Clear(); }
                }
            }
        }

        /// <summary>
        /// 辞書から引かれたゆらぎの表現を含む、正規化されたReadingを取得する。
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private string GetNormalizedReading(TokenElement token) =>
            Normalize(GetPlainReading(token));

        /// <summary>
        /// 英語表現は小文字に、WordMapから取得できるReadingがあればそれを、なければTokenElementからReadingまたはSurfaceを取得する。
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>小文字化またはWordMapから取得されたReadingか元々のTokenのReadingまたはSurface</returns>
        private string GetPlainReading(TokenElement token)
        {
            // MEMO: 英語表現はToLower()で大文字小文字を無視する。
            string surface = token.Surface.ToLower();

            // MEMO: むしろゆらぎ表現マップは読みを登録した辞書として機能する。
            // PlainReadingの代わりに登録された読みがあればそれを返す。
            return spellingVariationMap.ContainsKey(surface) ?
                spellingVariationMap[surface] :
                // surfaceが辞書に存在しない場合は、TokenElementからreadingまたはsurfaceを返す。
                (token.Reading == null || token.Reading == "" ? surface : token.Reading);
        }

        /// <summary>
        /// normalizes the.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>A string.</returns>
        private string Normalize(string input) =>
            // MEMO: 長音、促音、ヴ行、を正規化＝使用しない表現に置換。
            input.Replace("ー", "").Replace("ッ", "").Replace("ヴァ", "バ").Replace("ヴィ", "ビ")
                .Replace("ヴェ", "ベ").Replace("ヴォ", "ボ").Replace("ヴ", "ブ");

        /// <summary>
        /// Validate document.
        /// </summary>
        /// <param name="document">※不要。</param>
        /// <returns>A list of ValidationErrors.</returns>
        public List<ValidationError> Validate(Document document)
        {
            // readingMap初期化。
            Init();

            // すべてのSentenceを抽出し、ReadingMapを作成する。
            foreach (Sentence sentence in document.GetAllSentences())
            {
                // Token抽出。
                UpdateReadingMap(sentence);
            }

            List<ValidationError> errors = new List<ValidationError>();

            foreach (Sentence sentence in document.GetAllSentences())
            {
                foreach (TokenElement token in sentence.Tokens)
                {
                    // readingを取得。ゆらぎ表現が含まれる。
                    string reading = GetNormalizedReading(token);
                    if (!this.readingMap.ContainsKey(reading))
                    {
                        continue;
                    }

                    // tokenに対して取得したreadingでエラーを生成する。
                    errors.AddRange(GenerateErrors(sentence, token, reading));
                }
            }

            return errors;
        }

        /// <summary>
        /// TargetTokenに対してReadingが同じだがSurfaceが異なる複数のTokenを取得し、その出現位置を示すエラーメッセージを生成する。
        /// </summary>
        /// <param name="document"></param>
        /// <param name="sentence"></param>
        /// <param name="targetToken">評価対象のToken</param>
        /// <param name="reading">正規化されたtargetTokenのReading</param>
        private List<ValidationError> GenerateErrors(
            Sentence sentence,
            TokenElement targetToken,
            string reading)
        {
            List<ValidationError> errors = new List<ValidationError>();

            // ReadingマップからtargetTokenの出現回数を取得。
            var targetTokenCount = this.readingMap[reading].Where(i => i.Surface == targetToken.Surface).Count();

            // MEMO: 確認中のtargetTokenとSurfaceが異なるものを集める。品詞は考慮していない。
            // TODO: 複数の品詞のTokenが一緒くたにListに入ってしまうのでそのようなケースを考慮すべきか検討する。つまり品詞ごとに分けるか。
            IEnumerable<List<TokenElement>> enumerable = this.readingMap[reading].GroupBy(t => t.Surface)
                // targetTokenとSurfaceが異なるものを集める。
                .Where(g => g.Key != targetToken.Surface)
                // targetTokenと同じかより多い数のTokenがある場合は、targetTokenがゆらぎで、相手が正しい表現の可能性がある。
                .Where(g => targetTokenCount <= g.Count())
                .Select(g => g.ToList());

            foreach (List<TokenElement> tokenList in enumerable)
            {
                if (tokenList.Any())
                {
                    string positionsText = string.Join(", ", tokenList.Select(t => t.OffsetMap[0].ConvertToShortText()));

                    if (targetTokenCount == tokenList.Count())
                    {
                        // 出現数が同数の場合、どちらかがゆらぎでどちらかが正の可能性。=> key: SameCount
                        errors.Add(new ValidationError(
                            ValidationType.JapaneseWordVariation,
                            this.Level,
                            sentence,
                            targetToken.OffsetMap[0],
                            targetToken.OffsetMap[^1],
                            // エラー箇所のToken表現, ゆらぎ表現, ゆらぎ出現位置、の順で登録。
                            MessageArgs: new object[]
                            {
                                targetToken.GetSurfaceAndTagString(),
                                tokenList[0].GetSurfaceAndTagString(),
                                positionsText
                            },
                            MessageKey: "SameCount"
                        ));
                    }
                    else
                    {
                        // 相手の方が多い場合、targetTokenがゆらぎで相手が正と推測。=> key: Normal
                        errors.Add(new ValidationError(
                            ValidationType.JapaneseWordVariation,
                            this.Level,
                            sentence,
                            targetToken.OffsetMap[0],
                            targetToken.OffsetMap[^1],
                            // エラー箇所のToken表現, ゆらぎ表現, ゆらぎ出現位置、の順で登録。
                            MessageArgs: new object[]
                            {
                                targetToken.GetSurfaceAndTagString(),
                                tokenList[0].GetSurfaceAndTagString(),
                                positionsText
                            },
                            MessageKey: "Normal"
                        ));
                    }
                }
            }

            return errors;
        }
    }
}
