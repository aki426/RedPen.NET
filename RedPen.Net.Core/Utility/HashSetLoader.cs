using System;
using System.Collections.Generic;
using System.IO;

namespace RedPen.Net.Core.Utility
{
    //使用例
    //var loader = new HashSetLoader<string>(
    //    () => new HashSet<string>(),
    //    (set, line) =>
    //    {
    //        var elements = line.Split(',');
    //        foreach (var ele in elements)
    //        {
    //            set.Add(ele);
    //        }
    //    });
    //
    //var config = loader.Load("config.txt");

    /// <summary>
    /// JAVAのDictonaryLoaderのHashSet版の手前実装。
    /// MEMO: 記述を簡素化できるかもしれないが、JAVA版のコードを踏襲するためにこのような実装としている。
    /// </summary>
    public class HashSetLoader<T>
    {
        private readonly Func<HashSet<T>> _hashSetFactory;
        private readonly Action<HashSet<T>, string> _lineProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="HashSetLoader"/> class.
        /// </summary>
        /// <param name="hashSetFactory">The hash set factory.</param>
        /// <param name="lineProcessor">The line processor.</param>
        public HashSetLoader(
            Func<HashSet<T>> hashSetFactory,
            Action<HashSet<T>, string> lineProcessor)
        {
            _hashSetFactory = hashSetFactory;
            _lineProcessor = lineProcessor;
        }

        /// <summary>
        /// Loads the.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>A HashSet.</returns>
        public HashSet<T> Load(string filename)
        {
            var hashSet = _hashSetFactory();
            foreach (var line in File.ReadAllLines(filename))
            {
                _lineProcessor(hashSet, line);
            }
            return hashSet;
        }
    }
}
