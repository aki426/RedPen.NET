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
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using NLog;
using RedPen.Net.Core.Config;

namespace RedPen.Net.Core.Validators
{
    /// <summary>
    /// Validatorを生成するクラス。
    /// 厳密なFactoryパターンではないので、生成したらしっぱなしであることに注意。
    /// </summary>
    public class ValidatorFactory
    {
        private static Logger LOG = LogManager.GetCurrentClassLogger();

        /// <summary>外部から追加されたValidationNameとValidatorのクラス定義</summary>
        public ImmutableDictionary<string, Type> NameToValidatorMap { get; init; }

        /// <summary>
        /// RedPen.Net.CoreのデフォルトValidatiorしか使用しない場合のコンストラクタ。
        /// Prevents a default instance of the <see cref="ValidatorFactory"/> class from being created.
        /// </summary>
        public ValidatorFactory()
        {
            // 追加Validatorは無し。
            NameToValidatorMap = ImmutableDictionary<string, Type>.Empty;
        }

        /// <summary>
        /// ライブラリ外でValidatorを追加する場合のコンストラクタ。
        /// Initializes a new instance of the <see cref="ValidatorFactory"/> class.
        /// </summary>
        /// <param name="nameToValidatorMap">The name to validator map.</param>
        public ValidatorFactory(ImmutableDictionary<string, Type> nameToValidatorMap)
        {
            // DI
            NameToValidatorMap = nameToValidatorMap;
        }

        /// <summary>
        /// Activatorを使ってValidatorConfigurationを継承した具体的なValidatorConfigurationに対応する
        /// Validatorを継承した具体的なValidatorインスタンスを生成する関数。
        /// MEMO: ValidationName、ValidatorConfiguration、Validatorの対応関係は、ValidationType.csに定義されている。
        /// </summary>
        /// <param name="cultureInfo">ドキュメントの言語設定。Validatorが対応するかどうかを判定するために用いる。</param>
        /// <param name="symbolTable">Validator動作時のSymbolTable設定。</param>
        /// <param name="validatorConfiguration">具体的なValidatorConfigurationを与える。</param>
        /// <returns>引数に対応したValidatorインスタンス。</returns>
        public Validator? GetValidator(
            CultureInfo cultureInfo,
            SymbolTable symbolTable,
            ValidatorConfiguration validatorConfiguration)
        {
            // NOTE: Validatorの引数付きコンストラクタの引数定義は実装時の要注意事項として全Validatorで共通とする。
            var args = new object[] { cultureInfo, symbolTable, validatorConfiguration };

            // 外部プロジェクトから与えられたValidationNameに対応するValidatorが存在する場合は、そのValidatorを生成する。
            object v;
            if (NameToValidatorMap.ContainsKey(validatorConfiguration.ValidationName))
            {
                v = Activator.CreateInstance(
                    NameToValidatorMap[validatorConfiguration.ValidationName],
                    BindingFlags.CreateInstance,
                    null,
                    args,
                    null);
            }
            else
            {
                // NOTE: ValidatorConfigurationのValidationNameから対応するValidatorの名前を引いてTypeへ変換し、
                // Validator生成する。
                v = Activator.CreateInstance(
                    Type.GetType(GetValidatorFullName(validatorConfiguration)),
                    BindingFlags.CreateInstance,
                    null,
                    args,
                    null);
            }

            // 正しくValidatorを生成出来ていなかった場合はExceptionを投げる。
            var validator = v as Validator;
            if (validator != null)
            {
                if (validatorConfiguration.ValidationName == validator.ValidationName)
                {
                    return validator;
                }
            }

            LOG.Warn($"Failed to create Validator instance for {validatorConfiguration.ValidationName}");
            return null;
        }

        /// <summary>
        /// ValidatorConfigurationから、対応するValidatorを取得する。
        /// NOTE: ValidatorとValidationConfigurationは同じ名前空間に属していることが前提となる。
        /// </summary>
        /// <param name="validatorConfiguration">The validator configuration.</param>
        /// <returns>Validatorのアセンブリ修飾名</returns>
        public static string GetValidatorFullName(ValidatorConfiguration validatorConfiguration)
        {
            var confFullName = validatorConfiguration.GetType().FullName;
            return $"{confFullName.Substring(0, confFullName.Length - "Configuration".Length)}Validator";
        }
    }
}
