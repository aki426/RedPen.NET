using System;
using System.Collections.Generic;
using System.IO;

namespace RedPen.Net.Core.Validator
{
    //使用例
    //var loader = new DictionaryLoader<string, string>(
    //    () => new Dictionary<string, string>(),
    //    (map, line) =>
    //    {
    //        var parts = line.Split('=');
    //        map.Add(parts[0], parts[1]);
    //    });
    //
    //var config = loader.Load("config.txt");

    /// <summary>
    /// JAVAのDictonaryLoaderの手前実装。
    /// MEMO: 記述を簡素化できるかもしれないが、JAVA版のコードを踏襲するためにこのような実装としている。
    /// </summary>
    public class DictionaryLoader<TKey, TValue>
    {
        private readonly Func<Dictionary<TKey, TValue>> _dictionaryFactory;
        private readonly Action<Dictionary<TKey, TValue>, string> _lineProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryLoader"/> class.
        /// </summary>
        /// <param name="dictionaryFactory">The dictionary factory.</param>
        /// <param name="lineProcessor">The line processor.</param>
        public DictionaryLoader(
            Func<Dictionary<TKey, TValue>> dictionaryFactory,
            Action<Dictionary<TKey, TValue>, string> lineProcessor)
        {
            _dictionaryFactory = dictionaryFactory;
            _lineProcessor = lineProcessor;
        }

        /// <summary>
        /// Loads the.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>A Dictionary.</returns>
        public Dictionary<TKey, TValue> Load(string filename)
        {
            var dictionary = _dictionaryFactory();
            foreach (var line in File.ReadAllLines(filename))
            {
                _lineProcessor(dictionary, line);
            }
            return dictionary;
        }
    }
}
