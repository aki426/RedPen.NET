using System;
using System.Globalization;
using System.Reflection;
using NLog;
using RedPen.Net.Core.Config;

namespace RedPen.Net.Core.Validators
{
    /// <summary>
    /// Validatorを生成するクラス。厳密なFactoryパターンではないので、生成したらしっぱなしであることに注意。
    /// Validator生成のための定義は、ValidationType.csに記述されている。
    /// </summary>
    public class ValidatorFactory
    {
        private static Logger LOG = LogManager.GetCurrentClassLogger();

        private static ValidatorFactory instance;

        /// <summary>
        /// Prevents a default instance of the <see cref="ValidatorFactory"/> class from being created.
        /// </summary>
        private ValidatorFactory()
        {
        }

        /// <summary>
        /// Singletonインスタンスを取得する。
        /// </summary>
        /// <returns>A ValidatorFactory.</returns>
        public static ValidatorFactory GetInstance()
        {
            if (instance == null)
            {
                instance = new ValidatorFactory();
            }

            return instance;
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
        public Validator GetValidator(
            CultureInfo cultureInfo,
            SymbolTable symbolTable,
            ValidatorConfiguration validatorConfiguration)
        {
            // MEMO: Validatorの引数付きコンストラクタの引数定義は実装時の要注意事項として全Validatorで共通とする。
            var args = new object[] { cultureInfo, symbolTable, validatorConfiguration };

            object v = Activator.CreateInstance(
                validatorConfiguration.Type.GetTypeAsValidatorClass(),
                BindingFlags.CreateInstance,
                null,
                args,
                null);

            // 型チェックを行い、正しくValidatorを生成出来ていなかった場合はExceptionを投げる。
            if (v == null)
            {
                LOG.Warn($"Failed to create Validator instance for {validatorConfiguration.ValidationName}");
                return null;
            }
            else if (v is Validator && validatorConfiguration.Type.EqualsAsValidator(v as Validator))
            {
                return v as Validator;
            }
            else
            {
                LOG.Warn($"Failed to set Validator type for {validatorConfiguration.ValidationName}");
                return null;
            }
        }
    }
}
