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

        //private static Dictionary<ValidationLevel, string> mapping = new Dictionary<ValidationLevel, string>()
        //{
        //    { ValidationLevel.OFF, "Off" },
        //    { ValidationLevel.INFO, "Info" },
        //    { ValidationLevel.WARN, "Warn" },
        //    { ValidationLevel.ERROR, "Error" }
        //};

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

            //switch (level.ToUpper())
            //{
            //    case "OFF":
            //        return ValidationLevel.OFF;

            //    case "INFO":
            //        return ValidationLevel.INFO;

            //    case "WARN":
            //        return ValidationLevel.WARN;

            //    case "ERROR":
            //        return ValidationLevel.ERROR;

            //    default:
            //        throw new ArgumentException("No such a error level as " + level);
            //}
        }
    }
}
