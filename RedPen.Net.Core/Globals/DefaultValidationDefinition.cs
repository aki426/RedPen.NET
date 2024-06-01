using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Validators;

namespace RedPen.Net.Core.Globals
{
    /// <summary>RedPen.Net.Coreが提供する実装済みのValidation定義</summary>
    public class DefaultValidationDefinition
    {
        // NOTE: RedPen.Net.CoreはRedPen.NETのコアライブラリとして提供するものです。
        // RedPen.Net.Coreでは拡張性のために、本家のRedPenが持つJSのアドオンValidatorの代わりに、
        // コアライブラリに手を加えずに任意のValidatorを追加できるように設計しました。
        // 一方でRedPen.Net.Coreがデフォルトで提供するValidatorの定義はアプリケーション内で利用可能にする必要があります。
        // それはRedPen.Net.Core全体の不変でメタ的な定義であるため、ライブラリ全体のGlobalな定義としてこのクラスに集約することにしました。
        // （※デフォルトのValidatorの定義の第一目的は、JsonファイルからのConfiguration読み込み時に未定義のValidatorConfigurationを
        // ロードしないようにするためです）

        // TODO: 将来的にはPrismか.NET Community Toolkitを利用してもよいかもしれない。

        /// <summary>ValidatorConfigurationクラスを実装した具象クラスのTypeリスト</summary>
        public static ImmutableList<Type> ValidatorConfigurationTypes =>
            Assembly.GetExecutingAssembly().GetTypes()
                .Where(i => typeof(ValidatorConfiguration).IsAssignableFrom(i) && !i.IsAbstract).ToImmutableList();

        /// <summary>ValidationNameとValidatorConfigurationのTypeを紐づけた定義リスト</summary>
        public static ImmutableDictionary<string, Type> ValidatorConfTypeDefinitions =>
            ValidatorConfigurationTypes.Select(i => new KeyValuePair<string, Type>(i.Name.Replace("Configuration", ""), i))
            .ToImmutableDictionary();

        /// <summary>Validatorクラスを実装した具象クラスのTypeリスト</summary>
        public static ImmutableList<Type> ValidatorTypes =>
            Assembly.GetExecutingAssembly().GetTypes()
                .Where(i => typeof(Validator).IsAssignableFrom(i) && !i.IsAbstract).ToImmutableList();
    }
}
