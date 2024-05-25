using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Utility;

namespace RedPen.Net.Core.Validators.DocumentValidator
{
    /// <summary>KatakanaSpellCheckのConfiguration</summary>
    public record KatakanaSpellCheckConfiguration
        : ValidatorConfiguration,
        IMaxRatioConfigParameter, IMinFreqConfigParameter, IExpressionSetConfigParameter,
        IEnableDefaultDictConfigParameter
    {
        public double MaxRatio { get; init; }
        public int MinFreq { get; init; }
        public HashSet<string> ExpressionSet { get; init; }
        public bool EnableDefaultDict { get; init; }

        public KatakanaSpellCheckConfiguration(ValidationLevel level, double maxRatio, int minFreq, HashSet<string> expressionSet, bool enableDefaultDict) : base(level)
        {
            this.MaxRatio = maxRatio;
            this.MinFreq = minFreq;
            this.ExpressionSet = expressionSet;
            this.EnableDefaultDict = enableDefaultDict;
        }
    }

    // カタカナ語のスペルチェッカです。
    // 日本語のカタカナ語には複数の表現形式があります。
    // 例えば、"index"はカタカナ語では"インデクス"または"インデックス"と表現されます。
    // このような表記ゆれとも取れる表記の違いは、厳密なルールにより正誤を判定することが難しいです。
    // そこで、このValidatorは、カタカナ語が他の文で使われているカタカナ語との類似性を検証します。
    // 類似性はLevenshtein距離で定義されます。
    // ある2つのカタカナ語について、30%未満の距離のペアは類似していると判定します。
    // なお、カタカナ語の長さが小さい場合、ほとんどのペアが似ていると判定されてしまうため、
    // カタカナ語の長さのしきい値を設定しています。
    // カタカナ語の長さがしきい値より小さい場合、類似性を検出しません。

    /// <summary>KatakanaSpellCheckのValidator</summary>
    public class KatakanaSpellCheckValidator : Validator, IDocumentValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public KatakanaSpellCheckConfiguration Config { get; init; }

        /// <summary></summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

        private static int maxIgnoreLength = 3;

        /// <summary>
        /// Exception word list.
        /// </summary>
        private static HashSet<string> katakanaWordDict = null; // new HashSet<string>();

        /// <summary>LevenshteinDistanceUtility</summary>
        private LevenshteinDistanceUtility levenshteinDistance;

        // TODO: コンストラクタの引数定義は共通にすること。
        /// <summary>
        /// Initializes a new instance of the <see cref="KatakanaSpellCheckValidator"/> class.
        /// </summary>
        /// <param name="documentLangForTest">The document lang for test.</param>
        /// <param name="symbolTable">The symbol table.</param>
        /// <param name="config">The config.</param>
        public KatakanaSpellCheckValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            KatakanaSpellCheckConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            Config = config;

            // デフォルトリソースのカタカナ語はValidatorインスタンスが生成されるとき1回ロードするようにする。
            // これはロードコストをできるだけ減らすための措置。
            if (katakanaWordDict == null)
            {
                katakanaWordDict = ResourceFileLoader.LoadWordSet(DefaultResources.KatakanaSpellcheck);
            }

            // LevenshteinDistanceUtilityとメモ化キャッシュの初期化
            // MEMO: LevenStein距離の計算は、挿入、削除、置換のコストが全て1とする。
            // これによって計算結果は2つの文字列を入れ替えても同じになる。
            levenshteinDistance = new LevenshteinDistanceUtility();
        }

        /// <summary>
        /// Validate Document
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>A list of ValidationErrors.</returns>
        public List<ValidationError> Validate(Document document)
        {
            var errors = new List<ValidationError>();

            // すべてのTokenをさらって、カタカナ語を抽出しSurfaceでDictionary化する。
            var katakanaLists =
                new Dictionary<string, List<(TokenElement token, Sentence sentence)>>();
            foreach (var sentence in document.GetAllSentences())
            {
                // 固有名詞としてカタナナ交じりのSurfaceでToken分割されることがあるので、さらにカタカナ語部分を分割する。
                // MEMO: 現状の仕様では分割したTokenからはReading情報が欠落するので注意。
                var parsedTokens = TokenElement.ParseKatakanaToken(sentence.Tokens);

                // カタカナ語は途中で分割されていることがあるので、隣接したものは連結して1つのカタカナ語のTokenとして扱う。
                List<TokenElement> katakanaTokens = TokenElement.GetConcatedKatakanaWords(parsedTokens);

                // 元のTokensからカタカナ語のみ抽出して加える。
                // これにより、カタカナ語のTokenと、隣接する連続したカタカナ語を連結したTokenが両方リストに含まれる。
                katakanaTokens.AddRange(parsedTokens.Where(t => t.IsKatakanaWord()));

                foreach (var token in katakanaTokens)
                {
                    if (!katakanaLists.ContainsKey(token.Surface))
                    {
                        katakanaLists[token.Surface] = new List<(TokenElement token, Sentence sentence)>();
                    }
                    katakanaLists[token.Surface].Add((token, sentence));
                }
            }

            // 出現したカタカナ語についてLevenshtein距離を計算し、類似するものをエラーとして出力する。
            foreach (var katakana in katakanaLists.Keys)
            {
                if (katakana.Length <= maxIgnoreLength)
                {
                    // 閾値以下だった場合はチェックをスキップ
                    continue;
                }

                // 以下の条件に合致した場合はスキップ。
                // デフォルトリソースのカタカナ語辞書に存在する場合
                // Configで指定されたカタカナ語セット内に存在する場合
                // 既存のカタカナ語出現頻度がしきい値以上の場合（＝たくさん出現しているものはエラーではないと考える）
                if ((Config.EnableDefaultDict && katakanaWordDict.Contains(katakana))
                    || Config.ExpressionSet.Contains(katakana)
                    || Config.MinFreq <= katakanaLists[katakana].Count)
                {
                    continue;
                }

                // Levenshtein距離を計算し、しきい値以下の場合はエラーとして出力（かけ離れ過ぎていないカタカナ語の表記ゆれを検出する工夫）
                var lsDistThreshold = (int)Math.Round(katakana.Length * Config.MaxRatio);
                var found = false;
                // 現在対象としているカタカナ語同士でチェックしても意味が無いので抜く。
                foreach (var other in katakanaLists.Keys.Where(s => s != katakana))
                {
                    // MEMO: Levenshtein距離計算コストをスキップするためメモ化関数を用いる。
                    if (levenshteinDistance.GetDistanceMemoize(other, katakana) <= lsDistThreshold)
                    {
                        // otherの先頭文字位置を出現位置としてリストアップして文字列化しておく。
                        var positionsText = string.Join(", ", katakanaLists[other].Select(i => i.token.OffsetMap[0].ConvertToShortText()));

                        foreach (var tokenAndSentence in katakanaLists[katakana])
                        {
                            // 今エラーだと考えているカタカナ語について、相手の出現位置が1つでも一致した場合は、
                            // 部分文字列同士で一致したと判定してしまっていることになるので、その場合はエラーにしない。
                            if (katakanaLists[other].Any(i => i.token.Overlap(tokenAndSentence.token)))
                            {
                                // nothing
                            }
                            else
                            {
                                // MessageArgsは、該当カタカナ語、表記ゆれ相手、表記ゆれ相手の出現位置、の3つ。
                                errors.Add(new ValidationError(
                                    ValidationType.KatakanaSpellCheck,
                                    Level,
                                    tokenAndSentence.sentence,
                                    tokenAndSentence.token.OffsetMap[0],
                                    tokenAndSentence.token.OffsetMap[1],
                                    MessageArgs: new object[] { katakana, other, positionsText }));
                            }
                        }
                    }
                }
            }

            return errors;
        }
    }
}
