using System;
using System.Collections.Generic;
using System.Text;

namespace RedPen.Net.Core
{
    /// <summary>
    /// RedPenに存在する便利関数をテスタブルにするために括りだしたクラス。
    /// </summary>
    public static class RedPenUtility
    {
        /// <summary>
        /// Dictionary（Map）形式の文字列データをParseする関数。
        /// MEMO: 次のような形式のJAVAのMapの文字列表現に準拠している。
        /// 例：{smart,スマート},{distributed,ディストリビューテッド}
        /// </summary>
        /// <param name="mapStr">The map str.</param>
        /// <returns>A Dictionary.</returns>
        public static Dictionary<string, string> ParseMap(string mapStr)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            int start = 0, splitter = 0, end = 0;
            bool found = false;
            for (int i = 0; i < mapStr.Length; i++)
            {
                if (mapStr[i] == '{')
                {
                    start = i + 1;
                }
                else if (mapStr[i] == '}')
                {
                    end = i - 1;
                    found = true;
                }
                else if (mapStr[i] == ',')
                {
                    if (found == false)
                    {
                        splitter = i + 1; // e.g., SVM, SupportVector Machine
                        continue;
                    }
                    // extract key value pair
                    string key = mapStr.Substring(start, splitter - 1 - start);
                    while (mapStr[splitter + 1] == ' ') { ++splitter; } // skip white spaces
                    string value = mapStr.Substring(splitter, end + 1 - splitter);
                    map[key] = value;
                    // move pivots
                    start = i + 1;
                    end = i + 1;
                    splitter = i + 1;
                    found = false;
                }
            }

            // extract last key value pair
            if (splitter > 0 && end < mapStr.Length)
            { // for safe
                string key = mapStr.Substring(start, splitter - 1 - start);
                string value = mapStr.Substring(splitter, end + 1 - splitter);
                map[key] = value;
            }

            return map;
        }
    }
}
