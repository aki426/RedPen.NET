using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Utility;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>KatakanaSpellCheckのConfiguration</summary>
    public record KatakanaSpellCheckConfiguration : ValidatorConfiguration, IMinRatioConfigParameter, IMinFreqConfigParameter, IWordSetConfigParameter
    {
        public double MinRatio { get; init; }
        public int MinFreq { get; init; }
        public HashSet<string> WordSet { get; init; }

        public KatakanaSpellCheckConfiguration(ValidationLevel level, double minRatio, int minFreq, HashSet<string> wordSet) : base(level)
        {
            this.MinRatio = minRatio;
            this.MinFreq = minFreq;
            this.WordSet = wordSet;
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
    public class KatakanaSpellCheckValidator : Validator, ISentenceValidatable, IDocumentValidatable
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

        private Dictionary<string, int> katakanaWordFrequencies = new Dictionary<string, int>();

        /// <summary>
        /// Katakana word dic with line number.
        /// </summary>
        private Dictionary<string, int> wordPositionMap = new Dictionary<string, int>();

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
            this.Config = config;

            // デフォルトリソースのカタカナ語はValidatorインスタンスが生成されるとき1回ロードするようにする。
            // これはロードコストをできるだけ減らすための措置。
            if (katakanaWordDict == null)
            {
                katakanaWordDict = ResourceFileLoader.LoadWordSet(DefaultResources.KatakanaSpellcheck);
            }
        }

        // TODO: Validatorの種類をSentenceからDocumentを取るValidatorへ変更する。

        /// <summary>
        /// Validate.
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <returns>A list of ValidationErrors.</returns>
        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            // validation
            // collect katakana words
            StringBuilder katakana = new StringBuilder();
            foreach (char c in sentence.Content)
            {
                if (UnicodeUtility.IsKatakana(c))
                {
                    katakana.Append(c);
                }
                else
                {
                    string katakanaWord = katakana.ToString();
                    addKatakana(katakanaWord);
                    katakana.Clear();
                }
            }
            if (katakana.Length > 0)
            {
                addKatakana(katakana.ToString());
            }

            // validation
            katakana = new StringBuilder();
            foreach (char c in sentence.Content)
            {
                if (UnicodeUtility.IsKatakana(c))
                {
                    katakana.Append(c);
                }
                else
                {
                    //this.checkKatakanaSpell(sentence, katakana.ToString());
                    katakana.Clear();
                }
            }
            //checkKatakanaSpell(sentence, katakana.ToString());

            // TODO: MessageKey引数はErrorMessageにバリエーションがある場合にValidator内で条件判定して引数として与える。
            //result.Add(new ValidationError(
            //    ValidationType.KatakanaSpellCheck,
            //    this.Level,
            //    sentence,
            //    MessageArgs: new object[] { argsForMessageArg }));

            return result;
        }

        private void addKatakana(string katakanaWord)
        {
            if (!katakanaWordFrequencies.ContainsKey(katakanaWord))
            {
                katakanaWordFrequencies[katakanaWord] = 0;
            }
            katakanaWordFrequencies[katakanaWord]++;
        }

        private void checkKatakanaSpell(Sentence sentence, string katakana)
        {
            if (katakana.Length <= maxIgnoreLength)
            {
                // 閾値以下だった場合はチェックをスキップ
                return;
            }

            // 以下の条件に合致した場合はスキップ。
            // デフォルトリソースのカタカナ語辞書に存在する場合
            // Configで指定されたカタカナ語セット内に存在する場合
            // 既存のカタカナ語出現辞書に存在する場合
            // 既存のカタカナ語出現頻度がしきい値以上の場合
            if (katakanaWordDict.Contains(katakana)
                || Config.WordSet.Contains(katakana)
                || wordPositionMap.ContainsKey(katakana)
                || (katakanaWordFrequencies.ContainsKey(katakana) && katakanaWordFrequencies[katakana] > Config.MinFreq))
            {
                return;
            }

            // Levenshtein距離を計算し、しきい値以下の場合はエラーとして出力（かけ離れ過ぎていないカタカナ語の表記ゆれを検出する工夫）
            int lsDistThreshold = (int)Math.Round(katakana.Length * Config.MinRatio);
            bool found = false;
            foreach (string key in wordPositionMap.Keys)
            {
                if (LevenshteinDistanceUtility.GetDistance(key, katakana) <= lsDistThreshold)
                {
                    found = true;
                    //addLocalizedError(sentence, katakana, key, dic[key].ToString());
                }
            }

            // すでに見つかった場合は既存辞書に追加する。
            if (!found)
            {
                wordPositionMap[katakana] = sentence.LineNumber;
            }
        }

        /// <summary>
        /// Validate Document
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>A list of ValidationErrors.</returns>
        public List<ValidationError> Validate(Document document)
        {
            List<ValidationError> errors = new List<ValidationError>();

            // すべてのTokenをさらって、カタカナ語を抽出しSurfaceでDictionary化する。
            Dictionary<string, List<(TokenElement token, Sentence sentence)>> katakanaLists =
                new Dictionary<string, List<(TokenElement token, Sentence sentence)>>();
            foreach (Sentence sentence in document.GetAllSentences())
            {
                foreach (TokenElement token in sentence.Tokens.Where(t => t.IsKatakanaWord()))
                {
                    if (!katakanaLists.ContainsKey(token.Surface))
                    {
                        katakanaLists[token.Surface] = new List<(TokenElement token, Sentence sentence)>();
                    }
                    katakanaLists[token.Surface].Add((token, sentence));
                }
            }

            // 出現したカタカナ語についてLevenshtein距離を計算し、類似するものをエラーとして出力する。
            foreach (string katakana in katakanaLists.Keys)
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
                if (katakanaWordDict.Contains(katakana)
                    || Config.WordSet.Contains(katakana)
                    || katakanaLists[katakana].Count > Config.MinFreq)
                {
                    continue;
                }

                // Levenshtein距離を計算し、しきい値以下の場合はエラーとして出力（かけ離れ過ぎていないカタカナ語の表記ゆれを検出する工夫）
                int lsDistThreshold = (int)Math.Round(katakana.Length * Config.MinRatio);
                bool found = false;
                // 現在対象としているカタカナ語同士でチェックしても意味が無いので抜く。
                foreach (string other in katakanaLists.Keys.Where(s => s != katakana))
                {
                    if (LevenshteinDistanceUtility.GetDistance(other, katakana) <= lsDistThreshold)
                    {
                        // otherの先頭文字位置を出現位置としてリストアップして文字列化しておく。
                        string positionsText = string.Join(", ", katakanaLists[other].Select(i => i.token.OffsetMap[0].ConvertToShortText()));

                        foreach ((TokenElement token, Sentence sentence) tokenAndSentence in katakanaLists[katakana])
                        {
                            // MessageArgsは、該当カタカナ語、表記ゆれ相手、表記ゆれ相手の出現位置、の3つ。
                            errors.Add(new ValidationError(
                                ValidationType.KatakanaSpellCheck,
                                this.Level,
                                tokenAndSentence.sentence,
                                tokenAndSentence.token.OffsetMap[0],
                                tokenAndSentence.token.OffsetMap[1],
                                MessageArgs: new object[] { katakana, other, positionsText }));
                        }
                    }
                }
            }

            return errors;
        }
    }
}
