using System;
using System.Collections.Generic;
using System.Linq;

namespace RedPen.Net.Core.Utility
{
    /// <summary>
    /// The collection extensions.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Gets the value or default.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns>A TValue? .</returns>
        public static TValue? GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key) =>
            dictionary.GetValueOrDefault(key, default!);

        /// <summary>
        /// Gets the value or default.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>A TValue.</returns>
        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (dictionary is null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            return dictionary.TryGetValue(key, out TValue? value) ? value : defaultValue;
        }

        // ComputeIfPresentは次のようなnullを返すデリゲートがintがNull非許容型の値型である旨のエラーが出て実装できない。
        // dict.ComputeIfPresent("blueberry", (k, v) => null).Should().Be((true, 4));
        // Claude 3などにも提案してもらって何パターンか試したが、
        // 「Keyが存在しかつmappingFunctionの結果がnullの場合DictionaryからKeyを削除する」という
        // JAVA版の挙動が再現できないため不使用とする。
        // TODO: 型指定周りの解決策が見つかったら再検討する。

        ///// <summary>
        ///// JAVAのMap.computeIfPresentメソッドのC#移植版。
        ///// 指定したキーが辞書に存在する場合、与えられた関数を実行し、その結果で辞書の値を更新します。
        ///// 関数には、キーと現在の値が渡されます。キーが存在しない場合は、デフォルト値が返されます。
        ///// </summary>
        ///// <param name="dictionary">The dictionary.</param>
        ///// <param name="key">The key.</param>
        ///// <param name="mappingFunction">The mapping function.</param>
        ///// <returns>A TValue.</returns>
        //public static (bool containsKey, TValue? value) ComputeIfPresent<TKey, TValue>(
        //    this IDictionary<TKey, TValue> dictionary,
        //    TKey key,
        //    Func<TKey, TValue, TValue> mappingFunction)
        //{
        //    if (dictionary.TryGetValue(key, out TValue value))
        //    {
        //        TValue newValue = mappingFunction(key, value);

        //        if (newValue == null)
        //        {
        //            dictionary.Remove(key);
        //            return (false, default(TValue));
        //        }
        //        else
        //        {
        //            dictionary[key] = newValue;
        //            return (true, newValue);
        //        }
        //    }
        //    else
        //    {
        //        return (false, default(TValue));
        //    }
        //}

        /// <summary>
        /// JAVAのMap.computeIfAbsentメソッドのC#移植版。
        /// 指定したキーが辞書に存在しない場合、与えられた関数を実行し、その結果を辞書に追加します。
        /// 関数にはキーのみが渡されます。キーが存在する場合は、現在の値が返されます。
        ///
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="mappingFunction">The mapping function.</param>
        /// <returns>A TValue.</returns>
        public static TValue ComputeIfAbsent<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TKey, TValue> mappingFunction)
        {
            if (dictionary.TryGetValue(key, out TValue value))
            {
                return value;
            }

            TValue newValue = mappingFunction(key);
            dictionary[key] = newValue;
            return newValue;
        }

        /// <summary>
        /// 2つのDictionaryを結合する関数。firstの内容が優先される。
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>An IDictionary.</returns>
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(
            this IDictionary<TKey, TValue> first,
            IDictionary<TKey, TValue> second
            ) => first.Concat(second)
                .GroupBy(pair => pair.Key, (_, pairs) => pairs.First())
                .ToDictionary(pair => pair.Key, pair => pair.Value);

        /// <summary>
        /// 2つのDictionaryを結合する関数。secondの内容が優先される。
        /// JAVAのMap.putAllメソッドに相当する。
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>An IDictionary.</returns>
        public static IDictionary<TKey, TValue> Update<TKey, TValue>(
            this IDictionary<TKey, TValue> first,
            IDictionary<TKey, TValue> second
            ) => first.Concat(second)
                .GroupBy(pair => pair.Key, (_, pairs) => pairs.Last())
                .ToDictionary(pair => pair.Key, pair => pair.Value);
    }
}
