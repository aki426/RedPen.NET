using RedPen.Net.Core.Validator;
using System;
using System.Collections.Generic;
using System.IO;

namespace RedPen.Net.Core.Config
{
    /// <summary>
    /// The configuration builder.
    /// </summary>
    public class ConfigurationBuilder
    {
        private List<ValidatorConfiguration> validatorConfigs = new List<ValidatorConfiguration>();
        private List<Symbol> customSymbols = new List<Symbol>();
        private bool built = false;
        private string lang = "en";
        private string? variant = null;
        private DirectoryInfo? baseDir;
        private bool isSecure;

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
        /// Sets the base dir.
        /// MEMO: BaseDirはNullの場合実行時のカレントディレクトリを使用する。
        /// </summary>
        /// <param name="baseDir">The base dir.</param>
        /// <returns>A ConfigurationBuilder.</returns>
        public ConfigurationBuilder SetBaseDir(DirectoryInfo? baseDir)
        {
            CheckBuilt();
            this.baseDir = baseDir;
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
        /// Adds the available validator configs.
        /// </summary>
        /// <returns>A ConfigurationBuilder.</returns>
        public ConfigurationBuilder AddAvailableValidatorConfigs()
        {
            CheckBuilt();
            validatorConfigs.AddRange(ValidatorFactory.GetConfigurations(lang));
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
        /// Enables isSecure mode suitable for servers, where validator properties can come from end-users.
        /// </summary>
        /// <returns></returns>
        public ConfigurationBuilder SetSecure()
        {
            CheckBuilt();
            isSecure = true;
            return this;
        }

        /// <summary>
        /// Gets a value indicating whether secure.
        /// </summary>
        public bool Secure => isSecure;

        /// <summary>
        /// Builds the.
        /// </summary>
        /// <returns>A Configuration.</returns>
        public Configuration Build()
        {
            CheckBuilt();
            built = true;
            return new Configuration(
                baseDir ?? new DirectoryInfo(@".\"),
                new SymbolTable(lang, variant, customSymbols),
                this.validatorConfigs,
                this.lang,
                this.isSecure);
        }
    }
}
