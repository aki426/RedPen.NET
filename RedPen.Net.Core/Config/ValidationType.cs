using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using RedPen.Net.Core.Validators;

namespace RedPen.Net.Core.Config
{
    // NOTE: 新しくValidatorを追加する場合は、まず列挙型に追加してください。
    // 次に、Validatorと対になるValidatorConfigurationをそれぞれ実装します。
    // ValidationTypeを網羅的に列挙するタイプの判定は、Switch式を使い、抜け漏れの発生を予防してください。

    // 【用語の定義】
    // ValidationName : Validationの名前。ValidationTypeの列挙要素名そのもの。
    //      例）SentenceLength
    // ValidatorName : ValidationNameに対応するValidatorを継承する具象クラスのクラス名。
    //      ValidationNameに"Validator"を付加したもの。
    //      例）SentenceLengthValidator
    // ConfigurationName : ValidationNameに対応するValidatorConfigurationを継承する具象クラスのクラス名。
    //      ValidationNameに"Configuration"を付加したもの。
    //      例）SentenceLengthConfiguration

    /// <summary>Validationの種類の定義</summary>
    public enum ValidationType
    {
        CommaNumber,
        Contraction,
        DoubledConjunctiveParticleGa,
        DoubledJoshi,
        DoubledWord,
        DoubleNegative,
        DuplicatedSection,
        EmptySection,
        EndOfSentence,
        FrequentSentenceStart,
        GappedSection,
        HankakuKana,
        HeaderLength,
        Hyphenation,
        InvalidExpression,
        InvalidSymbol,
        InvalidWord,
        JapaneseAmbiguousNounConjunction,
        JapaneseAnchorExpression,
        JapaneseBrokenExpression,
        JapaneseExpressionVariation,
        JapaneseJoyoKanji,
        JapaneseNumberExpression,
        JapaneseStyle,
        KatakanaEndHyphen,
        KatakanaSpellCheck,
        ListLevel,
        LongKanjiChain,
        NumberFormat,
        Okurigana,
        ParagraphNumber,
        ParagraphStartWith,
        ParenthesizedSentence,
        Quotation,
        RedundantExpression,
        SectionLength,
        SectionLevel,
        SentenceLength,
        SpaceBeginningOfSentence,
        SpaceBetweenAlphabeticalWord,
        Spelling,
        StartWithCapitalLetter,
        SuccessiveSentence,
        SuccessiveWord,
        SuggestExpression,
        SymbolWithSpace,
        UnexpandedAcronym,
        VoidSection,
        WeakExpression,
        WordFrequency,
        WordNumber
    }

    /// <summary>
    /// ValidationTypeの拡張クラス。Validation、Validator、Configurationの名前解決に使えるメソッドを備える。
    /// </summary>
    public static class ValidationTypeExtend
    {
        /// <summary>
        /// ValidationName
        /// </summary>
        /// <param name="param">The param.</param>
        /// <returns>A string.</returns>
        public static string ValidationName(this ValidationType param) => param.ToString();

        /// <summary>
        /// ValidatorName
        /// </summary>
        /// <param name="param">The param.</param>
        /// <returns>A string.</returns>
        public static string ValidatorName(this ValidationType param) => $"{param.ToString()}Validator";

        /// <summary>
        /// ConfigurationName
        /// </summary>
        /// <param name="param">The param.</param>
        /// <returns>A string.</returns>
        public static string ConfigurationName(this ValidationType param) => $"{param.ToString()}Configuration";

        /// <summary>
        /// ValidationNameとして与えられた文字列からValidationTypeへ変換する関数。
        /// MEMO: 大文字小文字を区別しない。
        /// </summary>
        /// <param name="validationName">The validation name.</param>
        /// <returns>A ValidationType.</returns>
        public static ValidationType ConvertFrom(string validationName)
        {
            foreach (var value in Enum.GetValues(typeof(ValidationType)))
            {
                if (value.ToString().ToUpper() == validationName.ToUpper())
                {
                    return (ValidationType)value;
                }
            }

            throw new ArgumentException($"No such a validation name as {validationName}");
        }

        // NOTE: ValidationTypeから対応するValidatorクラスとValidatorConfigurationクラスのTypeを取得するメソッドを実装しています。
        // これらはアセンブリ内をスキャンしてValidatorクラスとValidatorConfigurationクラスを継承する具象クラスのみを取得するため、
        // 対応するクラスが存在しない場合はクラスが実装されていないことを意味します。
        // 逆に実装済みであることのチェッカーとして利用できます。

        /// <summary>Validatorクラスを実装した具象クラスのTypeリスト</summary>
        public static ImmutableList<Type> ValidatorTypes => Assembly.GetExecutingAssembly().GetTypes()
            .Where(i => typeof(Validator).IsAssignableFrom(i) && !i.IsAbstract).ToImmutableList();

        /// <summary>
        /// ValidationTypeに対応するValidatorクラスのTypeを取得する。
        /// </summary>
        /// <param name="param"></param>
        /// <returns>対応するクラスが存在しない場合はNull</returns>
        public static Type? TypeOfValidatorClass(this ValidationType param) =>
            ValidatorTypes.Where(i => i.Name == param.ValidatorName()).FirstOrDefault();

        /// <summary>ValidatorConfigurationクラスを実装した具象クラスのTypeリスト</summary>
        public static ImmutableList<Type> ConfigurationTypes =>
            Assembly.GetExecutingAssembly().GetTypes()
                .Where(i => typeof(ValidatorConfiguration).IsAssignableFrom(i) && !i.IsAbstract).ToImmutableList();

        /// <summary>
        /// ValidationTypeに対応するValidatorConfigurationクラスのTypeを取得する。
        /// </summary>
        /// <param name="param"></param>
        /// <returns>対応するクラスが存在しない場合はNull</returns>
        public static Type? TypeOfConfigurationClass(this ValidationType param) =>
            ConfigurationTypes.Where(i => i.Name == param.ConfigurationName()).FirstOrDefault();
    }
}
