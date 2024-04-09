using System.Collections.Generic;
using System.Linq;
using RedPen.Net.Core.Utility;

namespace RedPen.Net.Core.Validators
{
    public abstract class DictionaryValidator : Validator
    {
        protected DictionaryLoader<HashSet<string>> Loader = WORD_LIST;
        private string _dictionaryPrefix;
        private HashSet<string> _dictionary = new HashSet<string>();

        public DictionaryValidator()
            : base(new object[] { "list", new HashSet<string>(), "dict", "" })
        {
        }

        public DictionaryValidator(params object[] keyValues)
            : this()
        {
            addDefaultProperties(keyValues);
        }

        public DictionaryValidator(string dictionaryPrefix)
            : this()
        {
            _dictionaryPrefix = dictionaryPrefix;
        }

        public DictionaryValidator(DictionaryLoader<HashSet<string>> loader, string dictionaryPrefix)
            : this(dictionaryPrefix)
        {
            Loader = loader;
        }

        protected override void Init()
        {
            // NOTE: 仮実装のためコメントアウト。
            // TODO: ResourceManagerを用いたコードへ変更する。

            if (_dictionaryPrefix != null)
            {
                //string defaultDictionaryFile = "default-resources/" + _dictionaryPrefix + "-" + this.SymbolTable.Lang + ".dat";
                //_dictionary = Loader.LoadCachedFromResource(defaultDictionaryFile, GetType().Name + " default dictionary");
            }

            string confFile = GetString("dict");
            if (!string.IsNullOrEmpty(confFile))
            {
                //GetHashSet("list").UnionWith(Loader.LoadCachedFromFile(FindFile(confFile), GetType().Name + " user dictionary"));
            }
        }

        protected bool InDictionary(string word)
        {
            HashSet<string> customDictionary = this.GetHashSet("list");
            return _dictionary.Contains(word) || (customDictionary != null && customDictionary.Contains(word));
        }

        protected bool DictionaryExists()
        {
            HashSet<string> customDictionary = GetHashSet("list");
            return _dictionary.Count > 0 || (customDictionary != null && customDictionary.Count > 0);
        }

        protected IEnumerable<string> StreamDictionary()
        {
            return _dictionary.Concat(GetHashSet("list"));
        }
    }
}
