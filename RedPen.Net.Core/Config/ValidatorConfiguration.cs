using System.Collections.Generic;
using System.Text.Json.Serialization;
using RedPen.Net.Core.Utility;

namespace RedPen.Net.Core.Config
{
    // MEMO: 個別のValidatorConfigurationクラスの実装方法はValidator.csまたは既存のクラスを参照してください。

    /// <summary>
    /// RedPenConfigファイル内のValidatorの設定1つ分に相当するConfigurationを表現するための基底クラス。
    /// </summary>
    /// <param name="Level"></param>
    public abstract record ValidatorConfiguration(ValidationLevel Level = ValidationLevel.OFF)
    {
        /// <summary>クラス名からValidationNameを取得する（末尾の"Configuration"を除去したものがValidationの識別子）。</summary>
        [JsonIgnore]
        public string ValidationName => this.GetType().Name.Substring(0, this.GetType().Name.Length - "Configuration".Length);

        /// <summary>JsonのNameプロパティに出力するためのプロパティ。</summary>
        [JsonIgnore]
        public string Name => ValidationName;

        ///// <summary>This type.</summary>
        //[JsonIgnore]
        //public ValidationType Type => ValidationTypeExtend.ConvertFrom(ValidationName);

        /// <summary>デバッグ用のコレクション内確認用文字列出力</summary>
        [JsonIgnore]
        public string StringCollection => GetStringCollectionDebugText();

        /// <summary>SetまたはMapパラメータに対して読み込むべき外部ファイル</summary>
        public string DictFile { get; init; }

        /// <summary>
        /// Gets the string collection debug text.
        /// </summary>
        /// <returns>A string.</returns>
        private string GetStringCollectionDebugText()
        {
            if (this is ICharSetConfigParameter charSetConfig)
            {
                return $"CharSet: {charSetConfig.CharSet.ToStringExt()}";
            }
            else if (this is IWordSetConfigParameter wordSetConfig)
            {
                return $"WordSet: {wordSetConfig.WordSet.ToStringExt()}";
            }
            else if (this is IExpressionSetConfigParameter expressionSetConfig)
            {
                return $"ExpressionSet: {expressionSetConfig.ExpressionSet.ToStringExt()}";
            }
            else if (this is IGrammarRuleSetConfigParameter grammarRuleSetConfig)
            {
                return $"GrammarRuleSet: {grammarRuleSetConfig.GrammarRuleSet.ToStringExt()}";
            }
            else if (this is ICharMapConfigParameter charMapConfig)
            {
                return $"CharMap: {charMapConfig.CharMap.ToStringExt()}";
            }
            else if (this is IWordMapConfigParameter wordMapConfig)
            {
                return $"WordMap: {wordMapConfig.WordMap.ToStringExt()}";
            }
            else if (this is IExpressionMapConfigParameter expressionMapConfig)
            {
                return $"ExpressionMap: {expressionMapConfig.ExpressionMap.ToStringExt()}";
            }
            else if (this is IGrammarRuleMapConfigParameter grammarRuleMapConfig)
            {
                return $"GrammarRuleMap: {grammarRuleMapConfig.GrammarRuleMap.ToStringExt()}";
            }
            else
            {
                return "Nothing.";
            }
        }
    }

    #region Max/Min系のプロパティ定義のためのInterface

    /// <summary>ValidatorConfigurationの継承クラスがMaxLengthプロパティを持つことを示すインターフェース。</summary>
    public interface IMaxLengthConfigParameter
    {
        /// <summary>エラーとする文字数の最大値を表すConfigurationパラメータ。</summary>
        public int MaxLength { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがMinLengthプロパティを持つことを示すインターフェース。</summary>
    public interface IMinLengthConfigParameter
    {
        /// <summary>エラーとする文字数の最小値を表すConfigurationパラメータ。</summary>
        public int MinLength { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがMaxCountプロパティを持つことを示すインターフェース。</summary>
    public interface IMaxCountConfigParameter
    {
        /// <summary>エラーとする対象物の個数の最大値を表すConfigurationパラメータ。</summary>
        public int MaxCount { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがMinCountプロパティを持つことを示すインターフェース。</summary>
    public interface IMinCountConfigParameter
    {
        /// <summary>エラーとする対象物の個数の最小値を表すConfigurationパラメータ。</summary>
        public int MinCount { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがMaxLevelプロパティを持つことを示すインターフェース。</summary>
    public interface IMaxLevelConfigParameter
    {
        /// <summary>エラーとするレベルの最大値を表すConfigurationパラメータ。</summary>
        public int MaxLevel { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがMinLevelプロパティを持つことを示すインターフェース。</summary>
    public interface IMinLevelConfigParameter
    {
        /// <summary>エラーとするレベルの最小値を表すConfigurationパラメータ。</summary>
        public int MinLevel { get; init; }
    }

    /// <summary>MaxInterval</summary>
    public interface IMaxIntervalConfigParameter
    {
        /// <summary>エラーとする間隔の最大値を表すConfigurationパラメータ。</summary>
        public int MaxInterval { get; init; }
    }

    /// <summary>MinInterval</summary>
    public interface IMinIntervalConfigParameter
    {
        /// <summary>エラーとする間隔の最小値を表すConfigurationパラメータ。</summary>
        public int MinInterval { get; init; }
    }

    /// <summary>MaxRatio</summary>
    public interface IMaxRatioConfigParameter
    {
        /// <summary>エラーとする割合の最大値を表すConfigurationパラメータ。</summary>
        public double MaxRatio { get; init; }
    }

    /// <summary>MinRatio</summary>
    public interface IMinRatioConfigParameter
    {
        /// <summary>エラーとする割合の最小値を表すConfigurationパラメータ。</summary>
        public double MinRatio { get; init; }
    }

    /// <summary>MaxFreq</summary>
    public interface IMaxFreqConfigParameter
    {
        /// <summary>エラーとする発生回数の最大値を表すConfigurationパラメータ。</summary>
        public int MaxFreq { get; init; }
    }

    /// <summary>MinFreq</summary>
    public interface IMinFreqConfigParameter
    {
        /// <summary>エラーとする発生回数の最小値を表すConfigurationパラメータ。</summary>
        public int MinFreq { get; init; }
    }

    /// <summary>MaxDistance</summary>
    public interface IMaxDistanceConfigParameter
    {
        /// <summary>エラーとする対象物の間隔の最大値を表すConfigurationパラメータ。</summary>
        public int MaxDistance { get; init; }
    }

    /// <summary>MinDistance</summary>
    public interface IMinDistanceConfigParameter
    {
        /// <summary>エラーとする対象物の間隔の最小値を表すConfigurationパラメータ。</summary>
        public int MinDistance { get; init; }
    }

    #endregion Max/Min系のプロパティ定義のためのInterface

    #region Set/Map系のプロパティ定義のためのInterface

    /// <summary>ValidatorConfigurationの継承クラスがCharSetプロパティを持つことを示すインターフェース。</summary>
    public interface ICharSetConfigParameter
    {
        /// <summary>文字セットを表すConfigurationパラメータ。
        /// NOTE: 文字セットに含まれる文字がドキュメント中に出現するか、の判定等に用いる。</summary>
        public HashSet<string> CharSet { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがWordSetプロパティを持つことを示すインターフェース。</summary>
    public interface IWordSetConfigParameter
    {
        /// <summary>何らかの単語（＝Token）セットを表すConfigurationパラメータ。
        /// NOTE: 単語セットに含まれる単語がドキュメント中に出現するか、の判定等に用いる。</summary>
        public HashSet<string> WordSet { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがExpressionSetプロパティを持つことを示すインターフェース。</summary>
    public interface IExpressionSetConfigParameter
    {
        /// <summary>何らかの表現（単語の連続）セットを表すConfigurationパラメータ。
        /// NOTE: 表現セットに含まれる表現がドキュメント中に出現するか、の判定等に用いる。</summary>
        public HashSet<string> ExpressionSet { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがGrammarRuleSetプロパティを持つことを示すインターフェース。</summary>
    public interface IGrammarRuleSetConfigParameter
    {
        /// <summary>何らかの文法ルールセットを表すConfigurationパラメータ。
        /// NOTE: 文法ルールセットに含まれる文法パターンがドキュメント中に出現するか、の判定等に用いる。</summary>
        public HashSet<string> GrammarRuleSet { get; init; }
    }

    // TODO: Dictionary<string, string>の代わりにDictionary<char, char>に変更することも検討する。
    // ただしJsonフォーマットに文字を指定する方法が無いので、stringで十分かもしれない。

    /// <summary>ValidatorConfigurationの継承クラスがCharMapプロパティを持つことを示すインターフェース。</summary>
    public interface ICharMapConfigParameter
    {
        /// <summary>何らかの文字のマッピング（辞書）を表すConfigurationパラメータ。</summary>
        public Dictionary<string, string> CharMap { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがWordMapプロパティを持つことを示すインターフェース。</summary>
    public interface IWordMapConfigParameter
    {
        /// <summary>何らかの単語のマッピング（辞書）を表すConfigurationパラメータ。</summary>
        public Dictionary<string, string> WordMap { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがExpressionMapプロパティを持つことを示すインターフェース。</summary>
    public interface IExpressionMapConfigParameter
    {
        /// <summary>何らかの表現のマッピング（辞書）を表すConfigurationパラメータ。</summary>
        public Dictionary<string, string> ExpressionMap { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがGrammarRuleMapプロパティを持つことを示すインターフェース。</summary>
    public interface IGrammarRuleMapConfigParameter
    {
        /// <summary>GrammarRuleと対応する文字列のマッピングをあらわすConfigurationパラメータ。</summary>
        public Dictionary<string, string> GrammarRuleMap { get; init; }
    }

    #endregion Set/Map系のプロパティ定義のためのInterface

    #region その他のプロパティ定義のためのInterface

    /// <summary>EnableDefaultDict</summary>
    public interface IEnableDefaultDictConfigParameter
    {
        /// <summary>デフォルト辞書を使用するかどうかを表す。デフォルトではTrue。</summary>
        public bool EnableDefaultDict { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがJodoshiStyleプロパティを持つことを示すインターフェース。</summary>
    public interface IJodoshiStyleConfigParameter
    {
        /// <summary>日本語の助動詞のスタイルをだ・である調で統一するか、です・ます調で統一するかのフラグ</summary>
        public JodoshiStyle JodoshiStyle { get; init; }
    }

    /// <summary>日本語の助動詞のスタイルをだ・である調で統一するか、です・ます調で統一するかのフラグ</summary>
    public enum JodoshiStyle
    {
        DaDearu, // だ・である調へ統一
        DesuMasu // です・ます調へ統一
    }

    /// <summary>ValidatorConfigurationの継承クラスがNumberStyleプロパティを持つことを示すインターフェース。</summary>
    public interface INumberStyleConfigParameter
    {
        /// <summary>日本語の計数表現のスタイルを表すConfigurationパラメータ。</summary>
        public NumberStyle NumberStyle { get; init; }
    }

    /// <summary>計数表現のスタイルを表す列挙型</summary>
    public enum NumberStyle
    {
        HankakuOnly, // 半角数字の数字表現「1つ」のみを許容するモード
        ZenkakuOnly, // 全角数字の数字表現「１つ」のみを許容するモード
        KansujiOnly, // 漢数字の数字表現「一つ」のみを許容するモード
        HiraganaOnly // ひらがなの数字表現「ひとつ」のみを許容するモード
    }

    /// <summary>ValidatorConfigurationの継承クラスがForbiddenプロパティを持つことを示すインターフェース。</summary>
    public interface IForbiddenConfigParameter
    {
        /// <summary>スペースを許容しないかどうかを表すConfigurationパラメータ。Trueのときスペースを許容しない。</summary>
        public bool Forbidden { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがSkipAfterプロパティを持つことを示すインターフェース。</summary>
    public interface ISkipAfterConfigParameter
    {
        /// <summary>指定された文字列に含まれる文字が、その後ろにスペースを取らなくても良い＝Skipできる文字であることを表すプロパティ</summary>
        public string SkipAfter { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがSkipBeforeプロパティを持つことを示すインターフェース。</summary>
    public interface ISkipBeforeConfigParameter
    {
        /// <summary>
        /// 指定された文字列に含まれる文字が、その前にスペースを取らなくても良い＝Skipできる文字であることを表すプロパティ
        /// </summary>
        public string SkipBefore { get; init; }
    }

    #endregion その他のプロパティ定義のためのInterface
}
