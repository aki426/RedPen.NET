using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedPen.Net.Core.Utility
{
    /// <summary>DefaultResourceからの読み込みを行うクラス。</summary>
    public static class ResourceFileLoader
    {
        /// <summary>
        /// 複数行のテキスト表現から1行1ワードとしてHashSetに読み込む。
        /// MEMO: 前後の半角空白は除去され、半角英字の場合は小文字に変換される。
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>A HashSet.</returns>
        public static HashSet<string> LoadWordSet(string text)
        {
            return new HashSet<string>(text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim().ToLower()));
        }
    }
}
