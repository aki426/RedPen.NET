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

namespace RedPen.Net.Core.Utility
{
    public class StringUtility
    {
    }

    public static class StringExtensions
    {
        public static string RemoveEnd(this string source, string toRemove)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(toRemove))
            {
                return source;
            }

            return source.EndsWith(toRemove, StringComparison.Ordinal)
                ? source[..^toRemove.Length]
                : source;
        }
    }
}
