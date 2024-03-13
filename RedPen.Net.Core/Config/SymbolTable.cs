using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RedPen.Net.Core.Config
{
    public partial class SymbolTable : ICloneable, IEquatable<SymbolTable>
    {
        private static readonly long serialVersionUID = 1612920745151501631L;
        private static Logger LOG = LogManager.GetCurrentClassLogger();

        // MEMO: symbolDictionaryとvalueDictionaryはJAVA版ではLinkedHashMapを使用しているため、
        // 登録順序を維持したい意図があったのかもしれないが、実際の利用側のコードでは順序が必要とされていなかった。
        // C#版ではLinkedHashMapの代わりにOrderedDictionaryを使う方法もあるが、ジェネリックに対応しておらず
        // 使い勝手が悪そうなため、素直にDictionaryで実装することにした。
        private Dictionary<SymbolType, Symbol> symbolDictionary = new Dictionary<SymbolType, Symbol>();

        private Dictionary<char, Symbol> valueDictionary = new Dictionary<char, Symbol>();

        public string Variant { get; private set; }
        public string Lang { get; private set; }

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

        //    /**
        //     * Override a symbol with a new definition
        //     *
        //     * @param symbol symbol to define
        //     */
        public void UpdateSymbol(Symbol symbol)
        {
            symbolDictionary[symbol.Type] = symbol;
            valueDictionary[symbol.Value] = symbol;
        }

        // TODO: 命名がおかしいので、getterのみのTypesプロパティなどに修正する。
        public HashSet<SymbolType> GetNames()
        {
            return new HashSet<SymbolType>(symbolDictionary.Keys);
        }

        //    /**
        //     * Get the character specified with the symbolType.
        //     *
        //     * @param symbolType character symbolType
        //     * @return character containing the settings
        //     */
        public Symbol GetSymbol(SymbolType symbolType)
        {
            return this.symbolDictionary[symbolType];
        }

        //    /**
        //     * Get the character specified with the value.
        //     *
        //     * @param value character symbolType
        //     * @return character containing the settings
        //     */
        public Symbol GetSymbol(char value)
        {
            return this.valueDictionary[value];
        }

        //    /**
        //     * Detect the specified character is exit in the dictionary.
        //     *
        //     * @param type character symbolType
        //     * @return character when exist, null when the specified
        //     *         character does not exist
        //     */
        /// <summary>
        /// Detect the specified character is exit in the dictionary.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>現在のテーブ
        /// ル内のシンボルに対応するchar型の値か、
        /// テーブルに登録が無い場合はデフォルトシンボルテーブルの対応するchar型の値</returns>
        public char GetValueOrFallbackToDefault(SymbolType type)
        {
            return this.symbolDictionary.ContainsKey(type)
                ? this.symbolDictionary[type].Value : DEFAULT_SYMBOLS[type].Value;
        }

        //    /**
        //     * Detect the specified character is exit in the dictionary.
        //     *
        //     * @param value character value
        //     * @return character when exist, null when the specified
        //     *         character does not exist
        //     */
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

        //    @Override
        public override string ToString()
        {
            return $"SymbolTable{{symbolDictionary={symbolDictionary}, Lang='{Lang}', Variant='{Variant}'}}";
        }

        public object Clone()
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