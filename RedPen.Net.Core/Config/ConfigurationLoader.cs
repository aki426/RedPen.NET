using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NLog;
using System.Xml;

namespace RedPen.Net.Core.Config
{
    /// <summary>
    /// Load the central configuration of {@link cc.redpen.RedPen}.
    /// </summary>
    public class ConfigurationLoader
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();
        private bool secure;

        /// <summary>
        /// Creates the symbol.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>A Symbol.</returns>
        private static Symbol CreateSymbol(XmlElement element)
        {
            if (!element.HasAttribute("name") || !element.HasAttribute("value"))
            {
                throw new InvalidOperationException("Found element does not have name and value attribute...");
            }
            string value = element.GetAttribute("value");
            if (value.Length != 1)
            {
                throw new RedPenException("value should be one character, specified: " + value);
            }
            char charValue = value[0];
            return new Symbol(
                (SymbolType)Enum.Parse(typeof(SymbolType), element.GetAttribute("name")),
                charValue,
                element.GetAttribute("invalid-chars"),
                bool.Parse(element.GetAttribute("before-space")),
                bool.Parse(element.GetAttribute("after-space")));
        }

        /// <summary>
        /// parse the input stream. stream will be closed.
        /// </summary>
        /// <param name="input">stream</param>
        /// <returns>document object</returns>
        /// <exception cref="RedPenException">when failed to parse</exception>
        private static XmlDocument ToDocument(Stream input)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.XmlResolver = null; // Disable external entity resolution
                using (BufferedStream bis = new BufferedStream(input))
                {
                    doc.Load(bis);
                }
                return doc;
            }
            catch (Exception e)
            {
                // TODO: JAVA版を踏襲したが、RedPenExceptionにして再スローする意味はあるか？
                throw new RedPenException(e);
            }
        }

        /// <summary>
        /// load {@link cc.redpen.RedPen} settings.
        /// </summary>
        /// <param name="configFile">input configuration file</param>
        /// <returns>Validator configuration resources</returns>
        /// <exception cref="RedPenException">when failed to load configuration from specified configuration file</exception>
        public Configuration Load(FileInfo configFile)
        {
            LOG.Info("Loading config from specified config file: \"{0}\"", configFile.FullName);
            try
            {
                using (FileStream fis = configFile.OpenRead())
                {
                    return Load(fis, configFile.Directory);
                }
            }
            catch (IOException e)
            {
                throw new RedPenException(e);
            }
        }

        /// <summary>
        /// load {@link cc.redpen.RedPen} settings.
        /// </summary>
        /// <param name="resourcePath">input configuration path</param>
        /// <returns>Validator configuration resources</returns>
        /// <exception cref="RedPenException">when failed to load configuration from specified resource</exception>
        public Configuration LoadFromResource(string resourcePath)
        {
            return LoadFromResource(resourcePath, null);
        }

        /// <summary>
        /// load {@link cc.redpen.RedPen} settings.
        /// </summary>
        /// <param name="resourcePath">input configuration path</param>
        /// <param name="base">base dir for resolving of relative resources</param>
        /// <returns>Validator configuration resources</returns>
        /// <exception cref="RedPenException">when failed to load configuration from specified resource</exception>
        public Configuration LoadFromResource(string resourcePath, DirectoryInfo? baseDirectory)
        {
            using (Stream inputConfigStream = typeof(Configuration).Assembly.GetManifestResourceStream(resourcePath))
            {
                return Load(inputConfigStream, baseDirectory);
            }
        }

        /// <summary>
        /// load {@link cc.redpen.RedPen} settings.
        /// </summary>
        /// <param name="configString">configuration as String</param>
        /// <returns>Validator configuration resources</returns>
        /// <exception cref="RedPenException">when failed to load Configuration from specified string</exception>
        public Configuration LoadFromString(string configString)
        {
            return LoadFromString(configString, null);
        }

        /// <summary>
        /// load {@link cc.redpen.RedPen} settings.
        /// </summary>
        /// <param name="configString">configuration as String</param>
        /// <param name="base">base dir for resolving of relative resources</param>
        /// <returns>Validator configuration resources</returns>
        /// <exception cref="RedPenException">when failed to load Configuration from specified string</exception>
        public Configuration LoadFromString(string configString, DirectoryInfo? baseDirectory)
        {
            return Load(new MemoryStream(Encoding.UTF8.GetBytes(configString)), baseDirectory);
        }

        /// <summary>
        /// load {@link cc.redpen.RedPen} configuration.
        /// Provided stream will be closed.
        /// </summary>
        /// <param name="stream">input configuration settings</param>
        /// <returns>Configuration loaded from input stream</returns>
        /// <exception cref="RedPenException">when failed to load configuration from specified stream</exception>
        public Configuration Load(Stream stream)
        {
            return Load(stream, null);
        }

        // TODO: JAVA版ではConfigファイルはXML形式になっているが、JSON形式への置き換えを検討する。
        // そもそもValidatorの種類もそう増えないし、Configファイルの定義もそこまで多くないので、
        // すべての設定がテキスト化されているJSON形式で良い可能性がある。
        // XML形式ではValidatorのON/OFFをタグの有無で判断しているが、例えばLevelの値にOFFを設定し、
        // 設定ファイル内から削除せずにValidatorの適用をOFFにできればその方が設定ファイルの管理が楽になる。

        /// <summary>
        /// load {@link cc.redpen.RedPen} configuration.
        /// Provided stream will be closed.
        /// </summary>
        /// <param name="stream">input configuration settings</param>
        /// <param name="base">base dir for resolving of relative resources</param>
        /// <returns>Configuration loaded from input stream</returns>
        /// <exception cref="RedPenException">when failed to load configuration from specified stream</exception>
        public Configuration Load(Stream stream, DirectoryInfo? baseDirectory)
        {
            XmlDocument doc = ToDocument(stream);

            ConfigurationBuilder configBuilder = new ConfigurationBuilder().SetBaseDir(baseDirectory);
            if (secure)
            {
                configBuilder.SetSecure();
            }

            XmlElement rootElement = GetRootNode(doc, "redpen-conf");

            string language = rootElement.GetAttribute("lang");
            if (!string.IsNullOrEmpty(language))
            {
                LOG.Info("Language is set to \"{0}\"", language);
            }
            else
            {
                LOG.Warn("No language configuration...");
                LOG.Info("Set language to en");
                // TODO: デフォルトがenで良いのかen-USへ変更するべきか？
                language = "en";
            }

            // TODO: 古いtype属性をJAVA版から踏襲しなくてもよいので、どこかのタイミングで削除する。
            string variant = rootElement.GetAttribute("variant");
            if (string.IsNullOrEmpty(variant))
            {
                variant = rootElement.GetAttribute("type");
                if (!string.IsNullOrEmpty(variant)) LOG.Info("Deprecated: use \"variant\" attribute instead of \"type\"");
            }

            if (string.IsNullOrEmpty(variant))
            {
                LOG.Warn("No variant configuration...");
            }
            else
            {
                LOG.Info("Variant is set to \"{0}\"", variant);
            }

            // extract validator configurations
            XmlNodeList validatorConfigElementList = GetSpecifiedNodeList(rootElement, "validators");

            if (validatorConfigElementList == null)
            {
                LOG.Warn("There is no validators block");
            }
            else
            {
                XmlNodeList validatorElementList = validatorConfigElementList[0].ChildNodes;
                ExtractValidatorConfigurations(configBuilder, validatorElementList);
            }

            // extract symbol configurations
            XmlNodeList symbolTableConfigElementList =
                GetSpecifiedNodeList(rootElement, "symbols");
            configBuilder.SetLang(language);
            if (!string.IsNullOrEmpty(variant))
            {
                configBuilder.SetVariant(variant);
            }

            if (symbolTableConfigElementList != null)
            {
                ExtractSymbolConfig(configBuilder, symbolTableConfigElementList, language);
            }
            return configBuilder.Build();
        }

        /// <summary>
        /// Extracts the validator configurations.
        /// </summary>
        /// <param name="configBuilder">The config builder.</param>
        /// <param name="validatorElementList">The validator element list.</param>
        private void ExtractValidatorConfigurations(ConfigurationBuilder configBuilder, XmlNodeList validatorElementList)
        {
            ValidatorConfiguration currentConfiguration;
            Dictionary<string, ValidatorConfiguration> validatorConfigurations = new Dictionary<string, ValidatorConfiguration>();
            foreach (XmlNode nNode in validatorElementList)
            {
                if (nNode.NodeType != XmlNodeType.Element)
                {
                    continue;
                }
                XmlElement element = (XmlElement)nNode;
                if (element.Name == "validator")
                {
                    string currentValidatorName = element.GetAttribute("name");
                    currentConfiguration =
                        new ValidatorConfiguration(currentValidatorName);
                    // あるValidatorについての設定が複数ある場合は、後勝ちになる。
                    if (validatorConfigurations.ContainsKey(currentValidatorName))
                    {
                        LOG.Warn("Duplicated validator configuration was found: " + currentValidatorName);
                    }
                    validatorConfigurations[currentValidatorName] = currentConfiguration;
                    XmlNodeList propertyElementList = nNode.ChildNodes;
                    ExtractProperties(currentConfiguration, propertyElementList);
                    ExtractLevel(currentConfiguration, element);
                }
                else
                {
                    LOG.Warn("Invalid block: \"{0}\"", element.Name);
                    LOG.Warn("Skip this block ...");
                }
            }
            foreach (ValidatorConfiguration config in validatorConfigurations.Values)
            {
                configBuilder.AddValidatorConfig(config);
            }
        }

        /// <summary>
        /// Extracts the level.
        /// </summary>
        /// <param name="currentConfiguration">The current configuration.</param>
        /// <param name="element">The element.</param>
        private void ExtractLevel(ValidatorConfiguration currentConfiguration, XmlElement element)
        {
            string level = element.GetAttribute("level");
            if (!string.IsNullOrEmpty(level))
            {
                currentConfiguration.SetLevel(level);
            }
        }

        /// <summary>
        /// Extracts the properties.
        /// </summary>
        /// <param name="currentConfiguration">The current configuration.</param>
        /// <param name="propertyElementList">The property element list.</param>
        private void ExtractProperties(ValidatorConfiguration currentConfiguration,
                                       XmlNodeList propertyElementList)
        {
            foreach (XmlNode pNode in propertyElementList)
            {
                if (pNode.NodeType != XmlNodeType.Element)
                {
                    continue;
                }
                XmlElement propertyElement = (XmlElement)pNode;
                if (propertyElement.Name == "property" && currentConfiguration != null)
                {
                    currentConfiguration.AddProperty(
                        propertyElement.GetAttribute("name"),
                        propertyElement.GetAttribute("value"));
                }
            }
        }

        /// <summary>
        /// Extracts the symbol config.
        /// </summary>
        /// <param name="configBuilder">The config builder.</param>
        /// <param name="symbolTableConfigElementList">The symbol table config element list.</param>
        /// <param name="language">The language.</param>
        private void ExtractSymbolConfig(
            ConfigurationBuilder configBuilder,
            XmlNodeList symbolTableConfigElementList,
            string language)
        {
            configBuilder.SetLang(language);

            // TODO: Cast前の型チェックとしてnode.NodeType == XmlNodeType.Elementを追加し例外処理を行う。
            XmlNodeList? symbolTableElementList =
                GetSpecifiedNodeList((XmlElement)symbolTableConfigElementList[0], "symbol");
            if (symbolTableElementList == null)
            {
                LOG.Warn("there is no character block");
                return;
            }
            foreach (XmlNode nNode in symbolTableElementList)
            {
                if (nNode.NodeType == XmlNodeType.Element)
                {
                    XmlElement element = (XmlElement)nNode;
                    Symbol currentSymbol = CreateSymbol(element);
                    configBuilder.AddSymbol(currentSymbol);
                }
            }
        }

        /// <summary>
        /// Gets the specified node list.
        /// </summary>
        /// <param name="rootElement">The root element.</param>
        /// <param name="elementName">The element name.</param>
        /// <returns>A XmlNodeList? .</returns>
        private XmlNodeList? GetSpecifiedNodeList(XmlElement rootElement, string elementName)
        {
            XmlNodeList elementList =
                rootElement.GetElementsByTagName(elementName);
            if (elementList.Count == 0)
            {
                LOG.Info("No \"{0}\" block found in the configuration", elementName);
                return null;
            }
            else if (elementList.Count > 1)
            {
                LOG.Info("More than one \"{0}\" blocks in the configuration", elementName);
            }
            return elementList;
        }

        /// <summary>
        /// Gets the root node.
        /// </summary>
        /// <param name="doc">The doc.</param>
        /// <param name="rootTag">The root tag.</param>
        /// <returns>A XmlElement.</returns>
        private XmlElement GetRootNode(XmlDocument doc, string rootTag)
        {
            XmlNodeList rootConfigElementList =
                doc.GetElementsByTagName(rootTag);
            if (rootConfigElementList.Count == 0)
            {
                throw new InvalidOperationException("No \"" + rootTag
                    + "\" block found in the configuration");
            }
            else if (rootConfigElementList.Count > 1)
            {
                LOG.Warn("More than one \"{0}\" blocks in the configuration", rootTag);
            }
            XmlNode root = rootConfigElementList[0];
            XmlElement rootElement = (XmlElement)root;
            LOG.Info("Succeeded to load configuration file");
            return rootElement;
        }

        /// <summary>
        /// Update Secure to true.
        /// </summary>
        /// <returns>A ConfigurationLoader.</returns>
        public ConfigurationLoader Secure()
        {
            secure = true;
            return this;
        }
    }
}
