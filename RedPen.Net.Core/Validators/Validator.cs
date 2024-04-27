using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators
{
    // MEMO: 【抽象クラスの関数について】
    // JAVAの場合、仮想関数の扱いは、Interfaceおよび抽象クラス内では次のとおり。
    // final：仮想関数でなくなりオーバーライド不可、abstract：オーバーライド必須の抽象メソッド、それ以外：オーバーライド可能（@Overrideが必要）の仮想メソッド
    // C#の場合、仮想関数の扱いは、Interfaceおよび抽象クラス内では次のとおり。
    // キーワード無し：オーバーライド不可、abstract：オーバーライド必須の抽象メソッド、virtual：オーバーライド可能の仮想メソッド
    // つまりJAVAでfinalがついている関数はC#ではキーワード無し、キーワード無しのものはvirtualをつける。

    // MEMO: 【抽象クラスのメンバー変数について】
    // JAVAの場合、メンバー変数の扱いは、Interfaceおよび抽象クラス内では次のとおり。
    // final：コンストラクタまたはstatic initializerで初期化必須、継承先の具象クラスからはアクセスできますが、オーバーライド(上書き)することはできません。
    // finalキーワードは、その変数が最終的な値を持つことを意味しています。finalな変数は一度初期化されると、その値を変更することはできません。
    // private：オーバーライド不可、abstract：オーバーライド必須、それ以外：オーバーライド可能
    // キーワード無し：これらの変数は、抽象クラスのインスタンスに所属します。抽象クラスそのものはインスタンス化できませんが、具象クラスのインスタンスを介してアクセスできます。
    // static：具象クラスにおけるStaticメンバー変数と同じ。
    // final：finalメンバー変数は、コンストラクタまたはスタティックイニシャライザで初期化する必要があります。
    // private：抽象クラス内でのみアクセス可能です。具象クラスからはアクセスできません。
    // protected：同一パッケージ内の他のクラスや、抽象クラスを継承した具象クラスからアクセス可能です。
    // public：どこからでもアクセス可能です。

    // MEMO: JAVAのInterfaceはC#と異なり、具象メソッドとメンバー変数を持つことができる。

    /// <summary>
    /// The validator.
    /// </summary>
    public abstract class Validator
    {
        /// <summary>Nlog</summary>
        private static Logger LOG = LogManager.GetCurrentClassLogger();

        /// <summary>クラス名からValidationNameを取得する（末尾の"Validatro"を除去したものがValidationの識別子）。</summary>
        public string ValidationName => this.GetType().Name.Substring(0, this.GetType().Name.Length - "Validator".Length);

        /// <summary>ValidationLevel</summary>
        public ValidationLevel Level { get; set; }

        // MEMO: Configurationの中でValidation時に必要な情報は個別のValidatorConfigurationとSymbolTableのみである。
        // SymbolTableは入力文書に応じた言語設定で決定されるものなので、Validatorは外部からDIされValidate時に利用する。

        /// <summary>Validation中に使用するSymbolTable</summary>
        public SymbolTable SymbolTable { get; init; }

        /// <summary>
        /// このValidatorにサポートされている言語のリストを返す。空のリストだった場合、サポート対象言語に制限は無いとみなせる。
        /// TODO: サポート対象言語が限られる場合は、このメソッドを実装クラスでオーバーライドする。
        /// </summary>
        public virtual List<string> SupportedLanguages => new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Validator"/> class.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="documentLang">The document lang.</param>
        /// <param name="symbolTable">The symbol table.</param>
        protected Validator(
            ValidationLevel level,
            CultureInfo documentLang,
            SymbolTable symbolTable)
        {
            Level = level;
            SymbolTable = symbolTable;

            // サポート対象言語チェック。
            if (SupportedLanguages.Any())
            {
                if (!SupportedLanguages.Contains(documentLang.Name))
                {
                    throw new InvalidOperationException($"{this.ValidationName} don't support the language: {documentLang.Name}");
                }
            }
        }
    }

    /// <summary>ValidatorがDocumentを対象にValidateする場合に実装するインターフェース。</summary>
    public interface IDocumentValidatable
    {
        /// <summary>Validation処理本体</summary>
        public List<ValidationError> Validate(Document document);
    }

    /// <summary>ValidatorがSectionを対象にValidateする場合に実装するインターフェース。</summary>
    public interface ISectionValidatable
    {
        /// <summary>Validation処理本体</summary>
        public List<ValidationError> Validate(Section section);
    }

    /// <summary>ValidatorがSentenceを対象にValidateする場合に実装するインターフェース。</summary>
    public interface ISentenceValidatable
    {
        /// <summary>Validation処理本体</summary>
        public List<ValidationError> Validate(Sentence sentence);
    }

    // MEMO: 以下はValidator実装のボイラープレート。

    // TODO: Validator初期実装時は以下を適宜コピペして使ってもよい。

    //// TODO: Configurationにはパラメータに応じたInterfaceがあるので、必要なパラメータはInterfaceを実装することで定義する。

    ///// <summary>XXXのConfiguration</summary>
    //public record XXXConfiguration : ValidatorConfiguration
    //{
    //    public XXXConfiguration(ValidationLevel level) : base(level)
    //    {
    //    }
    //}

    //// TODO: Validation対象に応じて、IDocumentValidatable, ISectionValidatable, ISentenceValidatableを実装する。
    //public class XXXValidator : Validator, ISentenceValidatable
    //{
    //    /// <summary>Nlog</summary>
    //    private static Logger log = LogManager.GetCurrentClassLogger();

    //    // TODO: 専用のValidatorConfigurationを別途定義する。

    //    public XXXConfiguration Config { get; init; }

    //    // TODO: サポート対象言語がANYではない場合overrideで再定義する。
    //    /// <summary></summary>
    //    public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

    //    // TODO: コンストラクタの引数定義は共通にすること。
    //    public XXXValidator(
    //        CultureInfo documentLangForTest,
    //        SymbolTable symbolTable,
    //        XXXConfiguration config) :
    //        base(
    //            config.Level,
    //            documentLangForTest,
    //            symbolTable)
    //    {
    //        this.Config = config;
    //    }

    //    public void PreValidate(Sentence sentence)
    //    {
    //        // nothing.
    //    }

    //    public List<ValidationError> Validate(Sentence sentence)
    //    {
    //        List<ValidationError> result = new List<ValidationError>();

    //        // validation

    //        // TODO: MessageKey引数はErrorMessageにバリエーションがある場合にValidator内で条件判定して引数として与える。
    //        result.Add(new ValidationError(
    //            ValidationType.XXX,
    //            this.Level,
    //            sentence,
    //            MessageArgs: new object[] { argsForMessageArg }));

    //        return result;
    //    }
    //}
}
