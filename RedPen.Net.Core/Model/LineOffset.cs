﻿//   Copyright (c) 2024 KANEDA Akihiro <taoist.aki@gmail.com>
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

namespace RedPen.Net.Core.Model
{
    /// <summary>原文テキストにおける行番号とオフセット位置を表す。
    /// 原文テキストにおけるある文字の位置を、1始まりの行Indexと0始まりの列Indexで表現したもの、という解釈も成り立つ。</summary>
    public record LineOffset : IComparable<LineOffset>
    {
        /// <summary>行番号。1始まり。</summary>
        public int LineNum { get; init; }
        /// <summary>オフセット位置。0始まり。ある1文字の列番号と解釈してもよい。</summary>
        public int Offset { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LineOffset"/> class.
        /// </summary>
        /// <param name="lineNum">The line num.</param>
        /// <param name="offset">The offset.</param>
        public LineOffset(int lineNum, int offset)
        {
            LineNum = lineNum;
            Offset = offset;
        }

        /// <summary>
        /// Compares the to.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>An int.</returns>
        public int CompareTo(LineOffset? other)
        {
            if (other == null)
            {
                return 1;
            }

            if (LineNum != other.LineNum)
            {
                return LineNum - other.LineNum;
            }

            return Offset - other.Offset;
        }

        /// <summary>
        /// (L1,2)のような行＋オフセットの表示形式に変換する。
        /// </summary>
        /// <returns>A string.</returns>
        public string ConvertToShortText()
        {
            return $"(L{LineNum},{Offset})";
        }

        /// <summary>
        /// ある行に存在する文字列strに対して、開始位置を指定して、その文字列の各文字に対するLineOffsetリストを生成する。
        /// NOTE: 改行コードはLFのみを想定。LFが現れた場合にのみ行番号を増加する。
        /// </summary>
        /// <param name="lineNum"></param>
        /// <param name="startOffset"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static List<LineOffset> MakeOffsetList(int lineNum, int startOffset, string str)
        {
            List<LineOffset> result = new List<LineOffset>();

            foreach (char ch in str)
            {
                if (ch == '\n')
                {
                    result.Add(new LineOffset(lineNum, startOffset));
                    lineNum++;
                    startOffset = 0;
                }
                else
                {
                    result.Add(new LineOffset(lineNum, startOffset));
                    startOffset++;
                }
            }

            return result;
        }
    }
}
