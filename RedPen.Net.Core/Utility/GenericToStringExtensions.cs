//   Copyright (c) 2024 KANEDA Akihiro <taoist.aki@gmail.com>
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System.Collections.Generic;
using System.Linq;

namespace RedPen.Net.Core.Utility
{
    /// <summary>
    /// The generic to string extensions.
    /// </summary>
    public static class GenericToStringExtensions
    {
        // Dictionary's KeyValuePair

        /// <summary>
        /// KeyValuePairのためのToString拡張。
        /// </summary>
        /// <param name="kvp">The kvp.</param>
        /// <returns>A string.</returns>
        public static string ToStringExt<K, V>(this KeyValuePair<K, V> kvp)
        {
            return string.Format("[{0}] => {1}", kvp.Key, kvp.Value);
        }

        // Dictionary

        /// <summary>
        /// DictionaryのためのToString拡張。
        /// </summary>
        /// <param name="dic">The dic.</param>
        /// <returns>A string.</returns>
        public static string ToStringExt<K, V>(this Dictionary<K, V> dic)
        {
            return "{" + string.Join(", ", dic.Select((kvp) => kvp.ToStringExt())) + "}";
        }

        // List

        /// <summary>
        /// ListのためのToString拡張。
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns>A string.</returns>
        public static string ToStringExt<T>(this List<T> list) => "[" + string.Join(", ", list) + "]";

        /// <summary>
        /// 二重リストのためのToString拡張。
        /// </summary>
        /// <param name="listOfLists">The list of lists.</param>
        /// <returns>A string.</returns>
        public static string ToStringExt<T>(this List<List<T>> listOfLists) =>
            "[" + string.Join(", ", listOfLists.Select(l => l.ToStringExt())) + "]";

        // HashSet

        /// <summary>
        /// HashSetのためのToString拡張。
        /// </summary>
        /// <param name="set">The set.</param>
        /// <returns>A string.</returns>
        public static string ToStringExt<T>(this HashSet<T> set) => "[" + string.Join(", ", set) + "]";
    }
}
