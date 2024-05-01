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
    // KatakanaEndHyphenはカタカナ語の末尾のハイフン（長音記号、KATAKANA-HIRAGANA PROLONGED SOUND MARK）
    // に関するValidationです。
    // カタカナ語は末尾ハイフンに複数のバリエーションを持つことがあります。
    // 例えば"Computer"に対応するカタカナ語には「コンピュータ」と「コンピューター」がありどちらも一般的に使用されています。
    // KatakanaEndHyphenは日本産業規格JIS Z8301:2008 G.6.2.2 b) G.3.のルールを用いて末尾ハイフンを検証します。

    // JIS Z8301:2008のルールは以下のとおりです。
    // a) その言葉が3音以上の場合には、語尾に長音符号を付けない。例：エレベータ（elevator）
    // b) その言葉が2音以下の場合には、語尾に長音符号を付ける。 例：カー（car)、カバー（cover）
    // c) 複合の語は、それぞれの成分語について 上記a)又はb) を適用する。例：モーターカー（motor car）
    // d) 上記a)～c)による場合で、長音符号を書き表す音(例1)、はねる音(例2)、及びつまる音(例3)は、それぞれ1音と認め、
    // よう(拗)音は1音(例4)としない。例：テーパ（taper）、ダンパ（damper）、ニッパ（nipper）、シャワー（shower）
    // (JIS Z8301:2008 G.6.2.2 b 表G.3より引用)

    // 一方、JIS Z8301:2019では、Z8301:2008のG.6.2.2は削除されており、代わりに
    // 『H.6 外来語の表記 外来語の表記は，主として“外来語の表記（平成3.6.28 内閣告示第2号）”による。 』
    // となっています。

    // 『外来語の表記（平成3.6.28 内閣告示第2号）』は次にURLにて公開されています。
    // https://www.mhlw.go.jp/web/t_doc?dataId=00ta0026&dataType=1&pageNo=1
    // 長音の運用に関しては平成三年六月二十八日付け内閣官房長官通知(別添)の別紙「外来語の表記」の
    // 留意事項その2(細則的な事項)Ⅲ項「撥音、促音、長音その他に関するもの」の３に記載されています。

    // 3　長音は、原則として長音符合「ー」を用いて書く。
    // 〔例〕　エネルギー オーバーコート グループ ゲーム ショーテーブル パーティー ウェールズ(地)
    // ポーランド(地) ローマ(地) ゲーテ(人) ニュートン(人)
    // 注1 長音符号の代わりに母音字を添えて書く慣用もある。
    // 〔例〕　バレエ(舞踊) ミイラ
    // 注2 「エー」「オー」と書かず、「エイ」「オウ」と書くような慣用のある場合は、それによる。
    // 〔例〕　エイト ペイント レイアウト スペイン(地) ケインズ(人) サラダボウル ボウリング(球技)
    // 注3 英語の語末の‐er、‐or、‐arなどに当たるものは、原則としてア列の長音とし長音符号「ー」を用いて書き表す。
    // ただし、慣用に応じて「ー」を省くことができる。
    // 〔例〕　エレベーター ギター コンピューター マフラー エレベータ コンピュータ スリッパ

    // つまり2024年現在、JIS規格から末尾ハイフンを除去するルールは無く、
    // 公文書含む世間一般のルールでも末尾ハイフンを除去するルールは無く、
    // 慣用によって末尾ハイフンを除去しても除去しなくてもよい、ということになっています。

    // このValidatorの存在意義はかつてに比べて減っているという現在の状況を踏まえて利用してください。

    /// <summary>KatakanaEndHyphenのConfiguration</summary>
    public record KatakanaEndHyphenConfiguration : ValidatorConfiguration, IWordSetConfigParameter
    {
        public HashSet<string> WordSet { get; init; }

        public KatakanaEndHyphenConfiguration(ValidationLevel level, HashSet<string> wordSet) : base(level)
        {
            this.WordSet = wordSet;
        }
    }

    /// <summary>KatakanaEndHyphenのValidator</summary>
    public class KatakanaEndHyphenValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        // TODO: 専用のValidatorConfigurationを別途定義する。
        /// <summary>ValidatorConfiguration</summary>
        public KatakanaEndHyphenConfiguration Config { get; init; }

        // TODO: サポート対象言語がANYではない場合overrideで再定義する。
        /// <summary></summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

        // TODO: コンストラクタの引数定義は共通にすること。
        public KatakanaEndHyphenValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            KatakanaEndHyphenConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;
        }

        // JIS Z8301:2008のルール
        // a) その言葉が3音以上の場合には、語尾に長音符号を付けない。例：エレベータ（elevator）
        // b) その言葉が2音以下の場合には、語尾に長音符号を付ける。 例：カー（car)、カバー（cover）
        // について、長音符号を付けない語の長さのスレショルドとして2を設定する。

        /// <summary>デフォルトのハイフンの有無ルールを適用するカタカナ語の音の長さ（＝長音符号を除いた文字数）基準値</summary>
        private static readonly int DEFAULT_END_HYPHEN_THRESHOLD_WORD_LENGTH = 3;

        /// <summary>カタカナ誤の長音記号</summary>
        private static readonly char KATAKANA_HYPHEN_CHAR = 'ー';

        /// <summary>カタカナ語の中黒記号</summary>
        private static readonly char KATAKANA_MIDDLE_DOT_CHAR = '・';

        /// <summary>
        /// Validation.
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <returns>A list of ValidationErrors.</returns>
        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            // validation
            StringBuilder katakana = new StringBuilder();
            List<LineOffset> positionBuffer = new List<LineOffset>();

            for (int i = 0; i < sentence.Content.Length; i++)
            {
                char c = sentence.Content[i];

                // カタカナ文字を収集
                if (StringUtils.IsKatakana(c) && c != KATAKANA_MIDDLE_DOT_CHAR)
                {
                    katakana.Append(c);
                    positionBuffer.Add(sentence.ConvertToLineOffset(i));
                }
                else
                {
                    if (this.Config.WordSet.Count == 0 || !this.Config.WordSet.Contains(katakana.ToString()))
                    {
                        if (HasInvalidEndHyphen(katakana.ToString()))
                        {
                            result.Add(new ValidationError(
                                ValidationType.KatakanaEndHyphen,
                                this.Level,
                                sentence,
                                positionBuffer.First(),
                                positionBuffer.Last(),
                                MessageArgs: new object[] { katakana.ToString() }));
                        }
                    }

                    katakana.Clear();
                    positionBuffer.Clear();
                }
            }

            // カタカナ語で終わっているセンテンスの救済措置。
            if (katakana.Length > 0)
            {
                if (this.Config.WordSet.Count == 0 || !this.Config.WordSet.Contains(katakana.ToString()))
                {
                    if (HasInvalidEndHyphen(katakana.ToString()))
                    {
                        result.Add(new ValidationError(
                            ValidationType.KatakanaEndHyphen,
                            this.Level,
                            sentence,
                            positionBuffer.First(),
                            positionBuffer.Last(),
                            MessageArgs: new object[] { katakana.ToString() }));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 入力されたカタカナ語に対して、閾値より文字数が長く、末尾がハイフンで、
        /// かつ末尾から2文字目がハイフンを許容する文字ではないかを検証する。
        /// </summary>
        /// <param name="katakana">The katakana.</param>
        /// <returns>A bool.</returns>
        public static bool HasInvalidEndHyphen(string katakana)
        {
            return (DEFAULT_END_HYPHEN_THRESHOLD_WORD_LENGTH < katakana.Length &&
                katakana[katakana.Length - 1] == KATAKANA_HYPHEN_CHAR &&
                IsCharacterBeforeHyphen(katakana[katakana.Length - 2]));
        }

        /// <summary>
        /// 末尾から2文字目がハイフンを許容する文字である場合Trueを返す関数。
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns>A bool.</returns>
        private static bool IsCharacterBeforeHyphen(char c)
        {
            return c != 'ャ' && c != 'ュ' && c != 'ョ';
        }
    }
}
