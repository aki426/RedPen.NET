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

using System;
using System.Collections.Generic;
using System.Linq;

namespace RedPen.Net.Core.Utility
{
    /// <summary>DefaultResourceからの読み込みを行うクラス。</summary>
    public static class ResourceFileLoader
    {
        /// <summary>
        /// 複数行のテキスト表現から1行1ワードとしてHashSetに読み込む。
        /// MEMO: 前後の半角空白は除去され、半角英字の場合は小文字に変換される。
        /// また、行頭が#の場合はコメントとして無視される。
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>A HashSet.</returns>
        public static HashSet<string> LoadWordSet(string text)
        {
            return new HashSet<string>(text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim().ToLower())
                .Where(line => line != "")
                .Where(line => line[0] != '#'));
        }

        // TODO: LoadWordMapの実装。

        // TODO: 全てのDefaultResourceを読み込むValidationについて、EnableDefaultDictパラメータを反映し、
        // ValidatorConfigurationのコンストラクタでのデフォルトリソース読み込みを実装する。
    }
}
