using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RedPen.Net.Core.Config
{
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorConfiguration"/> class.
        /// </summary>
        /// <param name="level">ValidationLevelのテキスト表現</param>
        public ValidatorConfiguration(string level) : this(ValidationLevelExtend.ConvertFrom(level)) { }
    }

    /// <summary>ValidatorConfigurationの継承クラスがMaxLengthプロパティを持つことを示すインターフェース。</summary>
    public interface IMaxLengthConfigParameter
    {
        /// <summary>何らかの最長数を表すConfigurationパラメータ。</summary>
        public int MaxLength { get; init; }
    }

    /// <summary>ValidatorConfigurationの継承クラスがWordMapプロパティを持つことを示すインターフェース。</summary>
    public interface IWordMapConfigParameter
    {
        /// <summary>何らかの単語のマッピング（辞書）を表すConfigurationパラメータ。</summary>
        public Dictionary<string, string> WordMap { get; init; }
    }

    public interface IWordListConfigParameter
    {
        public List<string> WordList { get; init; }
    }

    public interface IMaxNumberConfigParameter
    {
        public int MaxNumber { get; init; }
    }
}
