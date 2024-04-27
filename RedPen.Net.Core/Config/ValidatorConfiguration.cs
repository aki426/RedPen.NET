using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RedPen.Net.Core.Config
{
    // MEMO: 個別のValidatorConfigurationクラスの実装方法は既存のクラスを参照してください。

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

    /// <summary>ValidatorConfigurationの継承クラスがMaxLengthプロパティを持つことを示すインターフェース。</summary>
    public interface IMaxLengthConfigParameter
    {
        /// <summary>何らかの最長数を表すConfigurationパラメータ。</summary>
        public int MaxLength { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがMaxNumberプロパティを持つことを示すインターフェース。</summary>
    public interface IMaxNumberConfigParameter
    {
        public int MaxNumber { get; init; }
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

    /// <summary>ValidatorConfigurationの継承クラスがJodoshiStyleプロパティを持つことを示すインターフェース。</summary>
    public interface IJodoshiStyleConfigParameter
    {
        /// <summary>日本語の助動詞のスタイルをだ・である調で統一するか、です・ます調で統一するかのフラグ</summary>
        public JodoshiStyle JodoshiStyle { get; init; }
    }

    /// <summary>日本語の助動詞のスタイルをだ・である調で統一するか、です・ます調で統一するかのフラグ</summary>
    public enum JodoshiStyle
    {
        DaDearu,
        DesuMasu
    }
}
