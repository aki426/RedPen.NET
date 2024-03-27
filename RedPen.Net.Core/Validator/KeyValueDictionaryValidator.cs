using System;
using System.Collections.Generic;
using System.Text;
using RedPen.Net.Core.Utility;

namespace RedPen.Net.Core.Validator
{
    /// <summary>
    /// KeyValueDictionaryを判定に使うValidatorの基底クラス。
    /// </summary>
    public class KeyValueDictionaryValidator : Validator
    {
        protected DictionaryLoader<string, string> loader = KEY_VALUE;
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

        public KeyValueDictionaryValidator(string dictionaryPrefix) : this()
        {
            this.dictionaryPrefix = dictionaryPrefix;
        }

        protected override void Init()

        {
            if (dictionaryPrefix != null)
            {
                string defaultDictionaryFile = "default-resources/" + dictionaryPrefix + "-" + SymbolTable.Lang + ".dat";

                Dictionary<string, string> dictionary = loader.loadCachedFromResource(
                    defaultDictionaryFile,
                    this.GetType().Name + " default dictionary");

                private getMap("map").private putAll(dictionary);
        }

        private String confFile = getString("dict");
        if (isNotEmpty(confFile)) {

            private getMap("map").private putAll(loader.loadCachedFromFile(findFile(confFile), getClass().getSimpleName() + " user dictionary"));
        }
}

protected boolean inDictionary(String word)
{
    return getMap("map").containsKey(word);
}

protected String getValue(String word)
{
    Map<String, String> dictionary = getMap("map");
    if (dictionary != null && dictionary.containsKey(word))
    {
        return dictionary.get(word);
    }
    return null;
}
    }
}
