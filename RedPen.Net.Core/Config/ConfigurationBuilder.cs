using System;
using System.Collections.Generic;

namespace RedPen.Net.Core.Config
{
    /// <summary>
    /// ConfigurationのBuilderクラス
    /// TODO: コード上で書きやすくするだけであまり要らない気もするのでどこかで見直す。
    /// </summary>
    public class ConfigurationBuilder
    {
        private string lang = "en-US";
        private string variant = "";
        private List<ValidatorConfiguration> validatorConfigs = new List<ValidatorConfiguration>();
        private List<Symbol> customSymbols = new List<Symbol>();

        private bool built = false; // ビルド処理が行われたかどうかを表すフラグ。つまり、ビルド処理は一度しか行えない。

        /// <summary>
        /// Checks the built.
        /// </summary>
        private void CheckBuilt()
        {
            if (built)
            {
                throw new InvalidOperationException("Configuration already built.");
            }
        }

        /// <summary>
        /// Sets the lang.
        /// </summary>
        /// <param name="lang">The lang.</param>
        /// <returns>A ConfigurationBuilder.</returns>
        public ConfigurationBuilder SetLang(string lang)
        {
            CheckBuilt();
            this.lang = lang;
            return this;
        }

        /// <summary>
        /// Sets the variant.
        /// </summary>
        /// <param name="variant">The variant.</param>
        /// <returns>A ConfigurationBuilder.</returns>
        public ConfigurationBuilder SetVariant(string variant)
        {
            CheckBuilt();
            this.variant = variant;
            return this;
        }

        /// <summary>
        /// Adds the validator config.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns>A ConfigurationBuilder.</returns>
        public ConfigurationBuilder AddValidatorConfig(ValidatorConfiguration config)
        {
            CheckBuilt();
            validatorConfigs.Add(config);
            return this;
        }

        /// <summary>
        /// Adds the symbol.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <returns>A ConfigurationBuilder.</returns>
        public ConfigurationBuilder AddSymbol(Symbol symbol)
        {
            CheckBuilt();
            customSymbols.Add(symbol);
            return this;
        }

        ///// <summary>
        ///// Adds the available validator configs.
        ///// </summary>
        ///// <returns>A ConfigurationBuilder.</returns>
        //public ConfigurationBuilder AddAvailableValidatorConfigs()
        //{
        //    CheckBuilt();
        //    validatorConfigs.AddRange(ValidatorFactory.GetConfigurations(lang));
        //    return this;
        //}

        /// <summary>
        /// Builds the.
        /// </summary>
        /// <returns>A Configuration.</returns>
        public Configuration Build()
        {
            CheckBuilt();
            built = true;
            // MEMO: Build時点でTokenizerは決定済み。
            //return new Configuration(
            //    baseDir ?? new DirectoryInfo(@".\"),
            //    new SymbolTable(lang, variant, customSymbols),
            //    this.validatorConfigs,
            //    this.lang,
            //    this.isSecure);

            // TODO: 動作確認して型を調整する。
            return new Configuration()
            {
                Lang = lang,
                Variant = variant,
                ValidatorConfigurations = validatorConfigs,
                Symbols = customSymbols
            };
        }
    }
}
