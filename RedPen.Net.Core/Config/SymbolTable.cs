using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using NLog;

namespace RedPen.Net.Core.Config
{
    /// <summary>
    /// The symbol table.
    /// </summary>
    public record SymbolTable
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        /// <summary>Lang</summary>
        public string Lang { get; init; }

        /// <summary>Variant</summary>
        public string Variant { get; init; }

        /// <summary>langとvariantの設定に応じたデフォルトのSymbolのDictionaryを取得する。</summary>
        public ImmutableDictionary<SymbolType, Symbol> DefaultSymbols { get; init; }

        // MEMO: symbolDictionaryとvalueDictionaryはJAVA版ではLinkedHashMapを使用しているため、
        // 登録順序を維持したい意図があったのかもしれないが、実際の利用側のコードでは順序が必要とされていなかった。
        // C#版ではLinkedHashMapの代わりにOrderedDictionaryを使う方法もあるが、ジェネリックに対応しておらず
        // 使い勝手が悪そうなため、素直にDictionaryで実装することにした。

        /// <summary>SymbolTypeからSymbolを引くDictionary。</summary>
        public ImmutableDictionary<SymbolType, Symbol> SymbolTypeDictionary { get; init; }

        // TODO: 本当にchar型の値をキーにしたDictionaryが必要なのか検討する。
        /// <summary>charからSymbolを引くDictionary。</summary>
        public ImmutableDictionary<char, Symbol> CharValueDictionary { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolTable"/> class.
        /// </summary>
        /// <param name="cultureInfo">The culture info.</param>
        /// <param name="variant">The variant.</param>
        /// <param name="customSymbols">デフォルトシンボルに対して追加設定したいSymbol</param>
        public SymbolTable(CultureInfo cultureInfo, string variant, List<Symbol> customSymbols) : this(cultureInfo.Name, variant, customSymbols) { }

        // TODO: DefaultSymbolLoaderに依存した実装ではなく、DefaultSymbolをコンストラクタ引数として渡す実装を検討する。
        // その場合SymbolTableがLangとVariantを持つ意味が無くなるため削除してもよい。
        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolTable"/> class.
        /// </summary>
        /// <param name="lang">The lang.</param>
        /// <param name="variant">The variant.</param>
        /// <param name="customSymbols">デフォルトシンボルに対して追加設定したいSymbol</param>
        public SymbolTable(string lang, string variant, List<Symbol> customSymbols)
        {
            this.Lang = lang;
            this.Variant = variant;
            DefaultSymbols = DefaultSymbolLoader.GetInstance().GetSymbolDictionary(lang, variant);

            // デフォルトシンボルとカスタムシンボルを結合してシンボル検索用Dictionaryを作成する。
            ImmutableDictionary<SymbolType, Symbol>.Builder typeDictBuilder = ImmutableDictionary.CreateBuilder<SymbolType, Symbol>();
            ImmutableDictionary<char, Symbol>.Builder charDictBuilder = ImmutableDictionary.CreateBuilder<char, Symbol>();
            foreach (Symbol symbol in DefaultSymbols.Values)
            {
                typeDictBuilder.Add(symbol.Type, symbol);
                // よく考えたらSymbolTypeは一意性があるが、char型の文字は複数のSymbolで同じものが使われているのでDictionaryで表現できない。
                // TODO: ImmutableDictionary<char, List<Symbol>>への変更を検討する。
                charDictBuilder[symbol.Value] = symbol;
            }
            foreach (Symbol symbol in customSymbols)
            {
                // MEMO: カスタムシンボルはデフォルトシンボルテーブルに対してUpdateするため上書きする。
                typeDictBuilder[symbol.Type] = symbol;
                charDictBuilder[symbol.Value] = symbol;
            }

            SymbolTypeDictionary = typeDictBuilder.ToImmutable();
            CharValueDictionary = charDictBuilder.ToImmutable();
        }

        // TODO: そもそもインスタンス生成時にデフォルトSymbolのDictionaryに対して
        // カスタムシンボルを追加する形で初期化しているので、DefaultSymbolsにFallBackすることはないはず。
        // よってこの関数は不要で直接symbolTypeDictionaryから取得するように変更する。

        /// <summary>
        /// Detect the specified character is exit in the dictionary.
        /// </summary>
        /// <param name="type">character symbolType</param>
        /// <returns>character when exist, null when the specified character does not exist
        /// 現在のテーブル内のシンボルに対応するchar型の値か、
        /// テーブルに登録が無い場合はデフォルトシンボルテーブルの対応するchar型の値</returns>
        public char GetValueOrFallbackToDefault(SymbolType type)
        {
            return this.SymbolTypeDictionary.ContainsKey(type)
                ? this.SymbolTypeDictionary[type].Value : DefaultSymbols[type].Value;
        }
    }
}
