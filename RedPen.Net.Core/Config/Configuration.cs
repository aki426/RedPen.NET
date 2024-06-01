using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

namespace RedPen.Net.Core.Config
{
    public record Configuration
    {
        /// <summary>RedPenが処理する対象ドキュメントの言語設定。
        /// NOTE: デフォルトコンストラクタで生成された後、オブジェクト初期化子で初期化されなかった場合のデフォルト値は"ja-JP"。
        /// Jsonデシリアライズ時のデフォルト値とは別設定である点に注意。</summary>
        private readonly string _documentLang = "ja-JP";

        /// <summary>RedPenが処理する対象ドキュメントの言語設定。</summary>
        public string DocumentLang
        {
            get { return _documentLang; }
            init
            {
                // 値のチェック。
                // MEMO: CultureInfo.GetCultureInfo()はNullの場合ArgumentNullException、
                // サポートされていないカルチャの場合CultureNotFoundExceptionをスローするため、nullチェックの代わりになる。
                _ = CultureInfo.GetCultureInfo(value);
                _documentLang = value;
            }
        }

        // NOTE: JAVA版の言語＆バリアント設定は、"en", "ja", "ja.hankaku", "ja.zenkaku2", "ru", "ko"
        // だったが、C#版では言語設定がCultureInfoに対応した表現（ja-JPなど）である一方、
        // Variantがなんの制約もないStringで定義されていることには疑問が残る。
        // しかし、言語間でVariantに共通性が無さそうなこと、Enumで定義すると将来的に数が多くなりすぎる可能性があることなどから、
        // Variantは一旦String型定義のままにしておく。

        /// <summary>RedPen実行時のバリアント設定。言語設定にヴァリエーションを持たせるために設定する。</summary>
        public string Variant { get; init; } = "";

        /// <summary>RedPenが出力するメッセージの言語設定。
        /// NOTE: デフォルトコンストラクタで生成された後、オブジェクト初期化子で初期化されなかった場合のデフォルト値は""。
        /// Jsonデシリアライズ時のデフォルト値とは別設定である点に注意。</summary>
        private readonly string _messageLang = "ja-JP";

        /// <summary>RedPenが出力するメッセージの言語設定。</summary>
        public string MessageLang
        {
            get { return _messageLang; }
            init
            {
                // 値のチェック。
                // MEMO: CultureInfo.GetCultureInfo()はNullの場合ArgumentNullException、
                // サポートされていないカルチャの場合CultureNotFoundExceptionをスローするため、nullチェックの代わりになる。
                _ = CultureInfo.GetCultureInfo(value);
                _messageLang = value;
            }
        }

        /// <summary>対象ドキュメントの言語設定のCultureInfo表現。</summary>
        [JsonIgnore]
        public CultureInfo DocumentCultureInfo => CultureInfo.GetCultureInfo(DocumentLang);

        /// <summary>出力メッセージの言語設定のCultureInfo表現。</summary>
        [JsonIgnore]
        public CultureInfo MessageCultureInfo => CultureInfo.GetCultureInfo(MessageLang);

        /// <summary>RedPen実行時のバリデータ設定。文書構造のチェックを行うための設定。</summary>
        public List<ValidatorConfiguration> ValidatorConfigurations { get; init; } = new();
        /// <summary>RedPen実行時のシンボル設定。文書構造のParsing/Tokenizeを行うための設定。</summary>
        public List<Symbol> Symbols { get; init; } = new();

        /// <summary>TODO: SymbolTableの定義やConfigurationとの関係については見直す必要があるが、暫定的にはこのようにしておく。</summary>
        [JsonIgnore]
        public SymbolTable SymbolTable => new(DocumentCultureInfo.Name, Variant, Symbols);

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class.
        /// </summary>
        /// <param name="documentLang">The document lang.</param>
        /// <param name="variant">The variant.</param>
        /// <param name="messageLang">The message lang.</param>
        /// <param name="validatorConfigurations">The validator configurations.</param>
        /// <param name="symbols">The symbols.</param>
        [JsonConstructor]
        public Configuration(
            string? documentLang,
            string? variant,
            string? messageLang,
            List<ValidatorConfiguration>? validatorConfigurations,
            List<Symbol>? symbols)
        {
            // Jsonデシリアライズ時のデフォルト値はJsonファイルで省略つまりnullで入力された場合のデフォルト値として定義。
            DocumentLang = documentLang ?? "ja-JP";
            Variant = variant ?? "";
            MessageLang = messageLang ?? DocumentLang;
            ValidatorConfigurations = validatorConfigurations ?? new List<ValidatorConfiguration>();
            Symbols = symbols ?? new List<Symbol>();
        }

        /// <summary>
        /// テストケースまたは非Jsonデシリアライズ用途のConfiguration生成用コンストラクタ。
        /// Initializes a new instance of the <see cref="Configuration"/> class.
        /// </summary>
        public Configuration() { }
    }
}
