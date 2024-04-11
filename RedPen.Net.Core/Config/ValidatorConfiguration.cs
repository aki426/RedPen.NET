using System;
using System.Collections.Generic;
using System.Linq;

namespace RedPen.Net.Core.Config
{
    public record ValidatorConfiguration(ValidationLevel Level = ValidationLevel.OFF)
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorConfiguration"/> class.
        /// </summary>
        /// <param name="level">ValidationLevelのテキスト表現</param>
        public ValidatorConfiguration(string level) : this(ValidationLevelExtend.ConvertFrom(level)) { }

        // public string GetValidatorClassName() => ConfigurationName + "Validator";
    }

    ///// <summary>
    ///// The validator configuration.
    ///// </summary>
    //public class ValidatorConfigurationOld : ICloneable, IEquatable<ValidatorConfiguration>
    //{
    //    // TODO: むしろValidationNameなどに改めた方が良い。移植の都合上Ver 1.0まではこのままとする。
    //    /// <summary>Configuration name string</summary>
    //    public string ConfigurationName { get; private set; }

    //    /// <summary>Property dictionary</summary>
    //    public Dictionary<string, string> Properties { get; private set; }

    //    /// <summary>エラーLevel。デフォルトはERROR</summary>
    //    public ValidationLevel Level { get; private set; } = ValidationLevel.ERROR;

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="ValidatorConfiguration"/> class.
    //    /// </summary>
    //    /// <param name="configurationName">The configuration name.</param>
    //    public ValidatorConfiguration(string configurationName) :
    //        this(configurationName, new Dictionary<string, string>())
    //    {
    //    }

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="ValidatorConfiguration"/> class.
    //    /// </summary>
    //    /// <param name="configurationName">The configuration name.</param>
    //    /// <param name="properties">The properties.</param>
    //    public ValidatorConfiguration(string configurationName, Dictionary<string, string> properties) :
    //        this(configurationName, properties, ValidationLevel.ERROR)
    //    {
    //    }

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="ValidatorConfiguration"/> class.
    //    /// </summary>
    //    /// <param name="configurationName">The configuration name.</param>
    //    /// <param name="properties">The properties.</param>
    //    /// <param name="level">The level.</param>
    //    public ValidatorConfiguration(string configurationName, Dictionary<string, string> properties, ValidationLevel level)
    //    {
    //        this.ConfigurationName = configurationName;
    //        this.Properties = properties;
    //        this.Level = level;
    //    }

    //    // MEMO: JAVAの実装ではこのメソッドが存在したが、C#ではPropertiesを公開しているので呼び出し側で対応する。
    //    ///// <summary>
    //    ///// Gets the property.
    //    ///// </summary>
    //    ///// <param name="name">The name.</param>
    //    ///// <returns>A string.</returns>
    //    //public string GetProperty(string name)
    //    //{
    //    //    // TODO: Dictionaryのキーに対するアクセスはTryGetValueを使うべき。
    //    //    return this.Properties[name];
    //    //}

    //    /// <summary>
    //    /// Sets the level.
    //    /// </summary>
    //    /// <param name="level">The level.</param>
    //    /// <returns>A ValidatorConfiguration.</returns>
    //    public ValidatorConfiguration SetLevel(string level)
    //    {
    //        try
    //        {
    //            this.Level = ValidationLevelExtend.ConvertFrom(level);
    //        }
    //        catch (ArgumentException e)
    //        {
    //            // MEMO: JAVA実装ではRuntimeExceptionに変更して再throwしているが、C#ではそのような例外は存在しない。
    //            throw;
    //        }
    //        return this;
    //    }

    //    /// <summary>
    //    /// Sets the level.
    //    /// </summary>
    //    /// <param name="level">The level.</param>
    //    /// <returns>A ValidatorConfiguration.</returns>
    //    public ValidatorConfiguration SetLevel(ValidationLevel level)
    //    {
    //        this.Level = level;
    //        return this;
    //    }

    //    /// <summary>
    //    /// Gets the validator class name.
    //    /// </summary>
    //    /// <returns>A string.</returns>
    //    public string GetValidatorClassName()
    //    {
    //        return ConfigurationName + "Validator";
    //    }

    //    /// <summary>
    //    /// Adds the property.
    //    /// </summary>
    //    /// <param name="name">The name.</param>
    //    /// <param name="value">The value.</param>
    //    /// <returns>A ValidatorConfiguration.</returns>
    //    public ValidatorConfiguration AddProperty(string name, object value)
    //    {
    //        // JAVAの実装ではObject型に対してString.valueOfを使っている。一旦ToStringを使う。
    //        Properties.Add(name, value.ToString());
    //        return this;
    //    }

    //    // MEMO: JAVAの実装ではaddAttributeがあったが使用されていなかったので実装しない。
    //    // 中身はaddPropertyと同じだった。

    //    /// <summary>
    //    /// Equals the.
    //    /// </summary>
    //    /// <param name="obj">The obj.</param>
    //    /// <returns>A bool.</returns>
    //    public override bool Equals(object? obj)
    //    {
    //        if (this == obj) return true;
    //        if (obj == null || obj is not ValidatorConfiguration) return false;

    //        return this.Equals(obj as ValidatorConfiguration);
    //    }

    //    /// <summary>
    //    /// Equals the.
    //    /// </summary>
    //    /// <param name="other">The other.</param>
    //    /// <returns>A bool.</returns>
    //    public bool Equals(ValidatorConfiguration? other)
    //    {
    //        if (this == other) return true;
    //        if (other == null) return false;

    //        // JAVA版では同値性判定にObjects.equalsを使っているが、これはnull許容型に対しても使えるためと思われる。
    //        return ConfigurationName == other.ConfigurationName &&
    //               Properties.SequenceEqual(other.Properties) &&
    //               Level == other.Level;
    //    }

    //    /// <summary>
    //    /// Gets the hash code.
    //    /// </summary>
    //    /// <returns>An int.</returns>
    //    public override int GetHashCode()
    //    {
    //        // ConfigurationNameのみのHash値を返す。
    //        return HashCode.Combine(ConfigurationName);
    //    }

    //    /// <summary>
    //    /// Tos the string.
    //    /// </summary>
    //    /// <returns>A string.</returns>
    //    public override string ToString()
    //    {
    //        return ConfigurationName;
    //    }

    //    /// <summary>
    //    /// Clones the.
    //    /// </summary>
    //    /// <returns>A ValidatorConfiguration.</returns>
    //    public ValidatorConfiguration Clone()
    //    {
    //        try
    //        {
    //            return new ValidatorConfiguration(
    //                this.ConfigurationName,
    //                new Dictionary<string, string>(this.Properties),
    //                this.Level);
    //        }
    //        catch (Exception e)
    //        {
    //            // JAVA版ではCloneNotSupportedExceptionをRuntimeExceptionにラップして再throwしているが、
    //            // C#では暫定的にExceptionの再スローとした。
    //            throw;
    //        }
    //    }

    //    /// <summary>
    //    /// Clones the.
    //    /// </summary>
    //    /// <returns>An object.</returns>
    //    object ICloneable.Clone()
    //    {
    //        return this.Clone();
    //    }
    //}
}
