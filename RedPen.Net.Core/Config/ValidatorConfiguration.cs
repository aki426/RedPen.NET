using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RedPen.Net.Core.Config
{
    // MEMO: 個別のValidatorConfigurationクラスの実装方法はValidator.csまたは既存のクラスを参照してください。

    /// <summary>
    /// RedPenConfigファイル内のValidatorの設定1つ分に相当するConfigurationを表現するための基底クラス。
    /// </summary>
    /// <param name="Level"></param>
    public abstract record ValidatorConfiguration(ValidationLevel Level = ValidationLevel.OFF)
    {
        [JsonIgnore]
        /// <summary>クラス名からValidationNameを取得する（末尾の"Configuration"を除去したものがValidationの識別子）。</summary>
        public string ValidationName => this.GetType().Name.Substring(0, this.GetType().Name.Length - "Configuration".Length);

        /// <summary>JsonのNameプロパティに出力するためのプロパティ。</summary>
        public string Name => ValidationName;

        [JsonIgnore]
        public ValidationType Type => ValidationTypeExtend.ConvertFrom(ValidationName);
    }

    // MEMO: 以下、個別のValidatorConfigurationクラスのプロパティ定義のためのInterface

    /// <summary>ValidatorConfigurationの継承クラスがMaxLengthプロパティを持つことを示すインターフェース。</summary>
    public interface IMaxLengthConfigParameter
    {
        /// <summary>何らかの最長数を表すConfigurationパラメータ。</summary>
        public int MaxLength { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがMinLengthプロパティを持つことを示すインターフェース。</summary>
    public interface IMinLengthConfigParameter
    {
        /// <summary>何らかの最小数を表すConfigurationパラメータ。</summary>
        public int MinLength { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがMinCountプロパティを持つことを示すインターフェース。</summary>
    public interface IMinCountConfigParameter
    {
        public int MinCount { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがMaxLevelプロパティを持つことを示すインターフェース。</summary>
    public interface IMaxLevelConfigParameter
    {
        public int MaxLevel { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがMinLevelプロパティを持つことを示すインターフェース。</summary>
    public interface IMinLevelConfigParameter
    {
        public int MinLevel { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがWordMapプロパティを持つことを示すインターフェース。</summary>
    public interface IWordMapConfigParameter
    {
        /// <summary>何らかの単語のマッピング（辞書）を表すConfigurationパラメータ。</summary>
        public Dictionary<string, string> WordMap { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがWordListプロパティを持つことを示すインターフェース。</summary>
    public interface IWordListConfigParameter
    {
        /// <summary>何らかの単語リストを表すConfigurationパラメータ。</summary>
        public List<string> WordList { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがWordSetプロパティを持つことを示すインターフェース。</summary>
    public interface IWordSetConfigParameter
    {
        /// <summary>何らかの単語セットを表すConfigurationパラメータ。
        /// MEMO: 単語セットに単語が含まれるかどうか、を判定するValidationの場合に用いる。</summary>
        public HashSet<string> WordSet { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがExpressionSetプロパティを持つことを示すインターフェース。</summary>
    public interface IExpressionSetConfigParameter
    {
        /// <summary>何らかの単語セットを表すConfigurationパラメータ。
        /// MEMO: 単語セットに単語が含まれるかどうか、を判定するValidationの場合に用いる。</summary>
        public HashSet<string> ExpressionSet { get; init; }
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
        public string SkipAfter { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがSkipBeforeプロパティを持つことを示すインターフェース。</summary>
    public interface ISkipBeforeConfigParameter
    {
        public string SkipBefore { get; init; }
    }

    public interface IMinIntervalConfigParameter
    {
        public int MinInterval { get; init; }
    }

    public interface IMinRatioConfigParameter
    {
        public double MinRatio { get; init; }
    }

    public interface IMinFreqConfigParameter
    {
        public int MinFreq { get; init; }
    }

    public interface IEnableDefaultDictConfigParameter
    {
        public bool EnableDefaultDict { get; init; }
    }

    public interface IMaxDistanceConfigParameter
    {
        public int MaxDistance { get; init; }
    }
}
