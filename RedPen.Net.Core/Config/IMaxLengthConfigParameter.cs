namespace RedPen.Net.Core.Config
{
    /// <summary>
    /// ValidatorConfigurationの継承クラスがどのようなパラメータを持つかを示すインターフェース。
    /// </summary>
    public interface IMaxLengthConfigParameter
    {
        /// <summary>
        /// 何らかの最長数を表すConfigurationパラメータ。
        /// </summary>
        public int MaxLength { get; init; }
    }
}
