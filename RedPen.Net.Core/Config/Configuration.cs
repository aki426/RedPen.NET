using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

namespace RedPen.Net.Core.Config
{
    public record Configuration
    {
        /// <summary>RedPen実行時の言語設定。入出力、エラーメッセージの言語設定に使用される。</summary>
        private readonly string _lang;
        /// <summary>RedPen実行時の言語設定。入出力、エラーメッセージの言語設定に使用される。</summary>
        public string Lang
        {
            get { return _lang; }
            init
            {
                // 値のチェック。
                // MEMO: CultureInfo.GetCultureInfo()はNullの場合ArgumentNullException、
                // サポートされていないカルチャの場合CultureNotFoundExceptionをスローするため、
                _ = CultureInfo.GetCultureInfo(value);
                _lang = value;
            }
        }
        /// <summary>言語設定のCultureInfo表現。</summary>
        [JsonIgnore]
        public CultureInfo CultureInfo => CultureInfo.GetCultureInfo(Lang);

        // NOTE: JAVA版の言語＆バリアント設定は、"en", "ja", "ja.hankaku", "ja.zenkaku2", "ru", "ko"
        // だったが、C#版ではCultureInfoに対応した言語表現（ja-JPなど）である一方、なんの制約もないStringで定義されたVariantでは疑問が残る。
        // しかし、言語間でVariantに共通性がなさそうなこと、Enumで実装すると将来的に数が多くなりすぎる可能性があることなどから、
        // Variantは一旦String型定義のままにしておく。

        /// <summary>RedPen実行時のバリアント設定。言語設定にヴァリエーションを持たせるために設定する。</summary>
        public string Variant { get; init; }
        /// <summary>RedPen実行時のバリデータ設定。文書構造のチェックを行うための設定。</summary>
        public List<ValidatorConfiguration> ValidatorConfigurations { get; init; }
        /// <summary>RedPen実行時のシンボル設定。文書構造のParsing/Tokenizeを行うための設定。</summary>
        public List<Symbol> Symbols { get; init; }

        /// <summary>TODO: SymbolTableの定義やConfigurationとの関係については見直す必要があるが、暫定的にはこのようにしておく。</summary>
        [JsonIgnore]
        public SymbolTable SymbolTable => new(CultureInfo.Name, Variant, Symbols);
    }
}
