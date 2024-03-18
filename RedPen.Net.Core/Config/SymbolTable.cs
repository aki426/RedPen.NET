using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RedPen.Net.Core.Config
{
    /// <summary>
    /// The symbol table.
    /// </summary>
    public partial class SymbolTable : ICloneable, IEquatable<SymbolTable>
    {
        private static readonly long serialVersionUID = 1612920745151501631L;
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        // MEMO: symbolDictionaryとvalueDictionaryはJAVA版ではLinkedHashMapを使用しているため、
        // 登録順序を維持したい意図があったのかもしれないが、実際の利用側のコードでは順序が必要とされていなかった。
        // C#版ではLinkedHashMapの代わりにOrderedDictionaryを使う方法もあるが、ジェネリックに対応しておらず
        // 使い勝手が悪そうなため、素直にDictionaryで実装することにした。
        private Dictionary<SymbolType, Symbol> symbolDictionary = new Dictionary<SymbolType, Symbol>();

        private Dictionary<char, Symbol> valueDictionary = new Dictionary<char, Symbol>();

        /// <summary>
        /// Gets the variant.
        /// </summary>
        public string Variant { get; private set; }

        /// <summary>
        /// Gets the lang.
        /// </summary>
        public string Lang { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolTable"/> class.
        /// </summary>
        /// <param name="lang">The lang.</param>
        /// <param name="variant">The variant.</param>
        /// <param name="customSymbols">The custom symbols.</param>
        public SymbolTable(string lang, string? variant, List<Symbol> customSymbols)
        {
            this.Lang = lang;
            this.Variant = variant ?? string.Empty;
            // いちいちデフォルトDictionaryからコピーするのは効率が悪い気がするが、
            // 何回も実行するわけではないのでコスト面では問題ないはず。
            // 一方、デフォルトシンボルテーブルを変換して直接symbolDictionaryとvalueDictionaryへ代入する方法もあるはず。
            // TODO: パフォーマンスが問題になったら要検討。
            DefaultSymbols.Values.ToList().ForEach(i => this.UpdateSymbol(i));
            customSymbols.ForEach(i => this.UpdateSymbol(i));
        }

        // TODO: GetDefaultSymbolsの反す型をIDictionaryかReadOnlyDictionaryか使用法によって検討する。

        /// <summary>langとvariantの設定に応じてデフォルトシンボルのDictionaryを取得する。</summary>
        public IDictionary<SymbolType, Symbol> DefaultSymbols
        {
            get
            {
                switch (Lang)
                {
                    case "ja":
                        LOG.Info("\"ja\" is specified.");
                        switch (this.Variant)
                        {
                            case "hankaku":
                                LOG.Info("\"hankaku\" variant is specified");
                                return JAPANESE_HANKAKU_SYMBOLS;

                            case "zenkaku2":
                                LOG.Info("\"zenkaku2\" variant is specified");
                                return JAPANESE_ZENKAKU2_SYMBOLS;

                            default:
                                this.Variant = "zenkaku";
                                LOG.Info("\"zenkaku\" variant is specified");
                                return JAPANESE_SYMBOLS;
                        }
                    //case "ru":
                    //    LOG.Info("\"ru\" is specified");
                    //    return RUSSIAN_SYMBOLS;

                    default:
                        LOG.Info("Default symbol settings are loaded");
                        return DEFAULT_SYMBOLS;
                }
            }
        }

        /// <summary>
        /// a symbol with a new definition
        /// </summary>
        /// <param name="symbol">symbol to define</param>
        public void UpdateSymbol(Symbol symbol)
        {
            symbolDictionary[symbol.Type] = symbol;
            valueDictionary[symbol.Value] = symbol;
        }

        // TODO: 命名がおかしいので、getterのみのTypesプロパティなどに修正する。
        /// <summary>
        /// Gets the names.
        /// </summary>
        /// <returns>A HashSet.</returns>
        public HashSet<SymbolType> GetNames()
        {
            return new HashSet<SymbolType>(symbolDictionary.Keys);
        }

        /// <summary>
        /// Get the character specified with the symbolType.
        /// </summary>
        /// <param name="symbolType">character symbolType</param>
        /// <returns>character containing the settings</returns>
        public Symbol GetSymbol(SymbolType symbolType)
        {
            return this.symbolDictionary[symbolType];
        }

        /// <summary>
        /// Get the character specified with the value.
        /// </summary>
        /// <param name="value">character symbolType</param>
        /// <returns>character containing the settings</returns>
        public Symbol GetSymbol(char value)
        {
            return this.valueDictionary[value];
        }

        /// <summary>
        /// Detect the specified character is exit in the dictionary.
        /// </summary>
        /// <param name="type">character symbolType</param>
        /// <returns>character when exist, null when the specified character does not exist
        /// 現在のテーブル内のシンボルに対応するchar型の値か、
        /// テーブルに登録が無い場合はデフォルトシンボルテーブルの対応するchar型の値</returns>
        public char GetValueOrFallbackToDefault(SymbolType type)
        {
            return this.symbolDictionary.ContainsKey(type)
                ? this.symbolDictionary[type].Value : DEFAULT_SYMBOLS[type].Value;
        }

        /// <summary>
        /// Detect the specified character is exit in the dictionary.
        /// </summary>
        /// <param name="value">character value</param>
        /// <returns>character when exist, null when the specified character does not exist</returns>
        public bool ContainsValue(char value)
        {
            return this.valueDictionary.ContainsKey(value);
        }

        //    @Override
        //public boolean equals(Object o)
        //    {
        //        if (this == o) return true;
        //        if (o == null || getClass() != o.getClass()) return false;

        //        SymbolTable that = (SymbolTable)o;
        //        return Objects.equals(Lang, that.Lang) &&
        //               Objects.equals(Variant, that.Variant) &&
        //               Objects.equals(symbolDictionary, that.symbolDictionary);
        //    }

        //    @Override
        //public int hashCode()
        //    {
        //        return Objects.hash(Lang, Variant, symbolDictionary);
        //    }

        //    @Override
        //public SymbolTable clone()
        //    {
        //        try
        //        {
        //            SymbolTable clone = (SymbolTable)super.clone();
        //            clone.symbolDictionary = new LinkedHashMap<>(symbolDictionary);
        //            clone.valueDictionary = new LinkedHashMap<>(valueDictionary);
        //            return clone;
        //        }
        //        catch (CloneNotSupportedException e)
        //        {
        //            throw new RuntimeException(e);
        //        }
        //    }

        /// <summary>
        /// Tos the string.
        /// </summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            return $"SymbolTable{{symbolDictionary={symbolDictionary}, Lang='{Lang}', Variant='{Variant}'}}";
        }

        /// <summary>
        /// Clones the.
        /// </summary>
        /// <returns>An object.</returns>
        public object Clone()
        {
            return this.DeepCopy();
        }

        /// <summary>
        /// Deeps the copy.
        /// </summary>
        /// <returns>A SymbolTable.</returns>
        public SymbolTable DeepCopy()
        {
            SymbolTable clone = (SymbolTable)this.MemberwiseClone();
            // MEMO: string型は値型のため、MemberwiseCloneでDeepCopyされるはず。
            // TODO: MemberwiseCloneと値型、参照型の挙動について調査する。
            //clone.Lang = string.Copy(Lang);
            //clone.Variant = string.Copy(Variant);
            clone.symbolDictionary = new Dictionary<SymbolType, Symbol>(symbolDictionary);
            clone.valueDictionary = new Dictionary<char, Symbol>(valueDictionary);

            return clone;
        }

        /// <summary>
        /// Equals the.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>A bool.</returns>
        public bool Equals(SymbolTable? other)
        {
            if (this == other) return true;
            if (other == null) return false;
            //if (other is not SymbolTable) return false;

            return Lang == other.Lang &&
                   Variant == other.Variant &&
                   symbolDictionary.SequenceEqual(other.symbolDictionary);
        }
    }
}
