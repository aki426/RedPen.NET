using RedPen.Net.Core.Validator;
using System;
using System.Collections.Generic;
using System.IO;

namespace RedPen.Net.Core.Config
{
    public class ConfigurationBuilder
    {
        private List<ValidatorConfiguration> validatorConfigs = new List<ValidatorConfiguration>();
        private List<Symbol> customSymbols = new List<Symbol>();
        private bool built = false;
        private string lang = "en";
        private string? variant = null;
        private FileInfo baseDir;
        private bool isSecure;

        private void CheckBuilt()
        {
            if (built)
            {
                throw new InvalidOperationException("Configuration already built.");
            }
        }

        public ConfigurationBuilder SetLang(string lang)
        {
            CheckBuilt();
            this.lang = lang;
            return this;
        }

        public ConfigurationBuilder SetBaseDir(FileInfo baseDir)
        {
            CheckBuilt();
            this.baseDir = baseDir;
            return this;
        }

        public ConfigurationBuilder AddSymbol(Symbol symbol)
        {
            CheckBuilt();
            customSymbols.Add(symbol);
            return this;
        }

        public ConfigurationBuilder AddValidatorConfig(ValidatorConfiguration config)
        {
            CheckBuilt();
            validatorConfigs.Add(config);
            return this;
        }

        public ConfigurationBuilder AddAvailableValidatorConfigs()
        {
            CheckBuilt();
            validatorConfigs.AddRange(ValidatorFactory.getConfigurations(lang));
            return this;
        }

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

        public Configuration Build()
        {
            CheckBuilt();
            built = true;
            return new Configuration(
                baseDir,
                new SymbolTable(lang, variant, customSymbols),
                this.validatorConfigs,
                this.lang,
                this.isSecure);
        }
    }
}