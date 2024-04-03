using System.Collections.Generic;
using RedPen.Net.Core.Utility;

namespace RedPen.Net.Core.Validators
{
    /// <summary>
    /// KeyValueDictionaryを判定に使うValidatorの基底クラス。
    /// </summary>
    public class KeyValueDictionaryValidator : Validator
    {
        protected DictionaryLoader<Dictionary<string, string>> loader = KEY_VALUE;
        private string dictionaryPrefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueDictionaryValidator"/> class.
        /// </summary>
        public KeyValueDictionaryValidator() : base(new object[] { "map", new Dictionary<string, object>(), "dict", "" })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueDictionaryValidator"/> class.
        /// </summary>
        /// <param name="keyValues">The key values.</param>
        public KeyValueDictionaryValidator(params object[] keyValues) : this()
        {
            addDefaultProperties(keyValues);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueDictionaryValidator"/> class.
        /// </summary>
        /// <param name="dictionaryPrefix">The dictionary prefix.</param>
        public KeyValueDictionaryValidator(string dictionaryPrefix) : this()
        {
            this.dictionaryPrefix = dictionaryPrefix;
        }

        /// <summary>
        /// Inits the.
        /// </summary>
        protected override void Init()
        {
            // MEMO: confファイルの定義では、mapはDictionary<string, string>型に対応する文字列データであり、
            // dictは>

            if (dictionaryPrefix != null)
            {
                // MEMO: C#ではResourceManagerにテキストファイルを登録している関係上、次のような命名規則となる。
                // ファイル名：<Prefix>.<Lang>.txt
                // プロパティ名：<Prefix>_<Lang>
                string propertyName = $"{dictionaryPrefix}_{SymbolTable.Lang}";

                Dictionary<string, string> dictionary = loader.LoadCachedFromResource(
                    propertyName,
                    this.GetType().Name + " default dictionary");

                // TODO: 次の行が何らかの副作用を期待したものだとすると、期待した挙動にならない可能性があるのでテストケースで検証する。こと。
                GetDictionary("map").Update(dictionary);
            }

            string confFile = GetString("dict");
            if (!string.IsNullOrEmpty(confFile))
            {
                // TODO: 次の行が何らかの副作用を期待したものだとすると、期待した挙動にならない可能性があるのでテストケースで検証する。こと。
                GetDictionary("map").Update(loader.LoadCachedFromFile(
                    FindFile(confFile),
                    this.GetType().Name + " user dictionary"));
            }
        }

        /// <summary>
        /// ins the dictionary.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns>A bool.</returns>
        protected bool inDictionary(string word)
        {
            return GetDictionary("map").ContainsKey(word);
        }

        /// <summary>
        /// gets the value.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns>A string? .</returns>
        protected string? getValue(string word)
        {
            Dictionary<string, string> dictionary = GetDictionary("map");
            if (dictionary != null && dictionary.ContainsKey(word))
            {
                return dictionary[word];
            }

            return null;
        }
    }
}
