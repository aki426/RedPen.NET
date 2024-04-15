using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;

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

        /// <summary>このValidatorが出力するErrorMessageの言語設定。</summary>
        public CultureInfo Lang { get; init; }

        /// <summary>多言語対応のためのエラーメッセージリソースマネージャ</summary>
        private ResourceManager errorMessages { get; init; }

        // MEMO: ValidatorConfigurationはValidatorクラスそのものには実装せず、
        // 継承先の具象クラスとしての個別のValidatorクラスでプロパティとして実装しコンストラクタからDIする。

        // MEMO: Configurationの中でValidation時に必要な情報は個別のValidatorConfigurationとSymbolTableのみである。
        /// <summary>Validation中に使用するSymbolTable</summary>
        public SymbolTable SymbolTable { get; init; }

        /// <summary>
        /// このValidatorにサポートされている言語のリストを返す。空のリストだった場合、サポート対象言語に制限は無いとみなせる。
        /// TODO: サポート対象言語が限られる場合は、このメソッドを実装クラスでオーバーライドする。
        /// </summary>
        public virtual List<string> SupportedLanguages => new List<string>();

        protected Validator(ValidationLevel level, CultureInfo lang, ResourceManager errorMessages, SymbolTable symbolTable)
        {
            Level = level;
            Lang = lang;
            this.errorMessages = errorMessages;
            SymbolTable = symbolTable;
        }

        /// <summary>
        /// create a ValidationError for the specified position with localized message with specified message key
        /// </summary>
        /// <param name="MessageKey"></param>
        /// <param name="sentenceWithError"></param>
        /// <param name="args"></param>
        protected internal ValidationError GetLocalizedError(
            Sentence sentenceWithError,
            object[] args,
            string? MessageKey = null)
        {
            return new ValidationError(
                this.ValidationName,
                GetLocalizedErrorMessage(args, MessageKey),
                sentenceWithError,
                Level);
        }

        /// <summary>
        /// create a ValidationError for the specified position with specified message key
        /// </summary>
        /// <param name="messageKey"></param>
        /// <param name="sentenceWithError"></param>
        /// <param name="start">start position in parsed sentence</param>
        /// <param name="end">end position in parsed sentence</param>
        /// <param name="args"></param>
        protected internal ValidationError GetLocalizedErrorWithPosition(
            Sentence sentenceWithError,
            object[] args,
            int start,
            int end,
            string? MessageKey = null)
        {
            return new ValidationError(
                this.ValidationName,
                GetLocalizedErrorMessage(args, MessageKey), // メッセージ生成。
                sentenceWithError,
                start,
                end,
                Level);
        }

        /// <summary>
        /// create a ValidationError using the details within the given token &amp; localized message
        /// </summary>
        /// <param name="sentenceWithError"></param>
        /// <param name="token">the TokenElement that has the error</param>
        /// <param name="args"></param>
        protected internal ValidationError GetLocalizedErrorFromToken(Sentence sentenceWithError, TokenElement token, object[] args)
        {
            // Surface, ゆらぎ表現, ゆらぎ出現位置、の順で登録。
            List<object> argList = new List<object>() { token.Surface };
            foreach (object arg in args)
            {
                argList.Add(arg);
            }

            return GetLocalizedErrorWithPosition(
                sentenceWithError,
                argList.ToArray(),
                token.Offset, // start
                token.Offset + token.Surface.Length // end
            );
        }

        /// <summary>
        /// returns localized error message for the given key formatted with argument
        /// </summary>
        /// <param name="MessageKey">ErrorMessageにキーが設定されている場合はここで設定。通常無いのでデフォルト値はnull。</param>
        /// <param name="args">objects to format</param>
        /// <returns>localized error message</returns>
        /// <exception cref="InvalidOperationException"></exception>
        protected internal string GetLocalizedErrorMessage(object[] args, string? MessageKey = null)
        {
            if (errorMessages == null)
            {
                throw new InvalidOperationException("message resource not found.");
            }
            else
            {
                // ErrorMessageにもValidatorNameだけでなく、エラーの種類によってはキー名を指定することがある。
                string suffix = MessageKey == null ? "" : "." + MessageKey;

                // ValidationMessage.resxではErrorMessageは"XXXValidator"という形式の識別子で登録されている。
                // 「Validatorのクラス名 + "." + キー名」という形式の識別子で検索した現在のロケール用のメッセージ。
                string pattern = errorMessages.GetString(this.GetType().Name + suffix, Lang);

                // MessageFormatの代わりにstring.Formatを使用
                return string.Format(Lang, pattern, args);
            }
        }
    }

    public interface IDocumentValidatable
    {
        public void PreValidate(Document document);

        public List<ValidationError> Validate(Document document);
    }

    public interface ISectionValidatable
    {
        public void PreValidate(Section section);

        public List<ValidationError> Validate(Section section);
    }

    public interface ISentenceValidatable
    {
        public void PreValidate(Sentence sentence);

        public List<ValidationError> Validate(Sentence sentence);
    }
}
