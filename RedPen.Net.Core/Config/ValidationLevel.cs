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

namespace RedPen.Net.Core.Config
{
    /// <summary>Validatorのレベル設定。そのままValidationError結果のレベルとして用いられる。</summary>
    public enum ValidationLevel
    {
        OFF, // 該当のValidatorを無効化する
        INFO,
        WARN,
        ERROR
    }

    /// <summary>Level Enumの拡張クラス。Enumに重要度とテキスト表現を付加する。</summary>
    public static class ValidationLevelExtend
    {
        private static Dictionary<ValidationLevel, int> severity = new Dictionary<ValidationLevel, int>()
        {
            { ValidationLevel.OFF, 0 },
            { ValidationLevel.INFO, 1 },
            { ValidationLevel.WARN, 2 },
            { ValidationLevel.ERROR, 3 }
        };

        /// <summary>
        /// Are the worse than.
        /// </summary>
        /// <param name="param">The param.</param>
        /// <param name="other">The other.</param>
        /// <returns>A bool.</returns>
        public static bool IsWorseThan(this ValidationLevel param, ValidationLevel other)
        {
            return severity[param] >= severity[other];
        }

        /// <summary>
        /// Converts the from.
        /// </summary>
        /// <param name="severity">The severity.</param>
        /// <returns>A Level.</returns>
        public static ValidationLevel ConvertFrom(int severity)
        {
            // 0~3の範囲外の値が来た場合はOFFまたはERRORにする。
            if (severity < 0)
            {
                return ValidationLevel.OFF;
            }
            else if (severity > 3)
            {
                return ValidationLevel.ERROR;
            }
            else
            {
                switch (severity)
                {
                    case 0:
                        return ValidationLevel.OFF;

                    case 1:
                        return ValidationLevel.INFO;

                    case 2:
                        return ValidationLevel.WARN;

                    case 3:
                        return ValidationLevel.ERROR;

                    default:
                        throw new ArgumentException("No such a error level as " + severity);
                }
            }
        }

        /// <summary>
        /// 名前からValidationLevelを取得する
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static ValidationLevel ConvertFrom(string level)
        {
            foreach (var value in Enum.GetValues(typeof(ValidationLevel)))
            {
                if (value.ToString() == level.ToUpper())
                {
                    return (ValidationLevel)value;
                }
            }

            throw new ArgumentException("No such a error level as " + level);
        }
    }
}
