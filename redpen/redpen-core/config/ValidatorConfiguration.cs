namespace redpen_core.config
{
    /// <summary>Validation結果のレベル</summary>
    public enum Level
    {
        INFO,
        WARN,
        ERROR
    }

    /// <summary>Level Enumの拡張クラス。Enumに重要度とテキスト表現を付加する。</summary>
    public static class LevelExtend
    {
        private static Dictionary<Level, int> severity = new Dictionary<Level, int>()
        {
            { Level.INFO, 0 },
            { Level.WARN, 1 },
            { Level.ERROR, 2 }
        };

        private static Dictionary<Level, string> mapping = new Dictionary<Level, string>()
        {
            { Level.INFO, "Info" },
            { Level.WARN, "Warn" },
            { Level.ERROR, "Error" }
        };

        /// <summary>
        /// Are the worse than.
        /// </summary>
        /// <param name="param">The param.</param>
        /// <param name="other">The other.</param>
        /// <returns>A bool.</returns>
        public static bool IsWorseThan(this Level param, Level other)
        {
            return severity[param] >= severity[other];
        }

        /// <summary>
        /// Converts the from.
        /// </summary>
        /// <param name="severity">The severity.</param>
        /// <returns>A Level.</returns>
        public static Level ConvertFrom(int severity)
        {
            if (severity <= 0)
            {
                return Level.INFO;
            }
            else if (severity == 1)
            {
                return Level.WARN;
            }
            else
            {
                return Level.ERROR;
            }
        }

        public static Level ConvertFrom(string level)
        {
            switch (level.ToUpper())
            {
                case "INFO":
                    return Level.INFO;

                case "WARN":
                    return Level.WARN;

                case "ERROR":
                    return Level.ERROR;

                default:
                    throw new ArgumentException("No such a error level as " + level);
            }
        }

        /// <summary>
        /// Tos the string.
        /// </summary>
        /// <param name="param">The param.</param>
        /// <returns>A string.</returns>
        public static string ToString(this Level param)
        {
            return mapping[param];
        }
    }

    public class ValidatorConfiguration : ICloneable, IEquatable<ValidatorConfiguration>
    {
        public string ConfigurationName { get; private set; }

        public Dictionary<string, string> Properties { get; private set; }

        /// <summary>エラーLevel。デフォルトはERROR</summary>
        public Level Level { get; private set; } = Level.ERROR;

        public ValidatorConfiguration(string configurationName) :
            this(configurationName, new Dictionary<string, string>())
        {
        }

        public ValidatorConfiguration(string configurationName, Dictionary<string, string> properties) :
            this(configurationName, properties, Level.ERROR)
        {
        }

        public ValidatorConfiguration(string configurationName, Dictionary<string, string> properties, Level level)
        {
            this.ConfigurationName = configurationName;
            this.Properties = properties;
            this.Level = level;
        }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A string.</returns>
        public string GetProperty(string name)
        {
            // TODO: Dictionaryのキーに対するアクセスはTryGetValueを使うべき。
            return this.Properties[name];
        }

        public ValidatorConfiguration SetLevel(string level)
        {
            try
            {
                this.Level = LevelExtend.ConvertFrom(level);
            }
            catch (ArgumentException e)
            {
                // MEMO: JAVA実装ではRuntimeExceptionに変更して再throwしているが、C#ではそのような例外は存在しない。
                throw;
            }
            return this;
        }

        public ValidatorConfiguration SetLevel(Level level)
        {
            this.Level = level;
            return this;
        }

        public string GetValidatorClassName()
        {
            return ConfigurationName + "Validator";
        }

        public ValidatorConfiguration AddProperty(string name, object value)
        {
            // JAVAの実装ではObject型に対してString.valueOfを使っている。一旦ToStringを使う。
            Properties.Add(name, value.ToString());
            return this;
        }

        // MEMO: JAVAの実装ではaddAttributeがあったが使用されていなかったので実装しない。
        // 中身はaddPropertyと同じだった。

        public override bool Equals(object? obj)
        {
            if (this == obj) return true;
            if (obj == null || obj is not ValidatorConfiguration) return false;

            return this.Equals(obj as ValidatorConfiguration);
        }

        public bool Equals(ValidatorConfiguration? other)
        {
            if (this == other) return true;
            if (other == null) return false;

            // JAVA版では同値性判定にObjects.equalsを使っているが、これはnull許容型に対しても使えるためと思われる。
            return ConfigurationName == other.ConfigurationName &&
                   Properties.SequenceEqual(other.Properties) &&
                   Level == other.Level;
        }

        public override int GetHashCode()
        {
            // ConfigurationNameのみのHash値を返す。
            return HashCode.Combine(ConfigurationName);
        }

        public override string ToString()
        {
            return ConfigurationName;
        }

        public ValidatorConfiguration Clone()
        {
            try
            {
                return new ValidatorConfiguration(
                    this.ConfigurationName,
                    new Dictionary<string, string>(this.Properties),
                    this.Level);
            }
            catch (Exception e)
            {
                // JAVA版ではCloneNotSupportedExceptionをRuntimeExceptionにラップして再throwしているが、
                // C#では暫定的にExceptionの再スローとした。
                throw;
            }
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}