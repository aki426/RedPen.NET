using System;
using System.Collections.Generic;
using System.Linq;
using RedPen.Net.Core.Tokenizer;

namespace RedPen.Net.Core.Validator
{
    /// <summary>
    /// トークン分割された文のSurfaceレベルでの表現パターン。
    /// </summary>
    [ToString]
    public class ExpressionRule
    {
        private List<TokenElement> _elements;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionRule"/> class.
        /// </summary>
        public ExpressionRule()
        {
            this._elements = new List<TokenElement>();
        }

        /// <summary>
        /// Adds the.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>An ExpressionRule.</returns>
        public ExpressionRule Add(TokenElement element)
        {
            this._elements.Add(element);
            return this;
        }

        /// <summary>
        /// Tos the surface.
        /// </summary>
        /// <returns>A string.</returns>
        public string ToSurface()
        {
            return string.Join("", this._elements.Select(e => e.Surface));
        }

        /// <summary>
        /// Equals the.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>A bool.</returns>
        public override bool Equals(object? obj)
        {
            if (this == obj) return true;
            if (obj == null || obj is not ExpressionRule) return false;

            ExpressionRule? that = obj as ExpressionRule;
            if (that == null && this == null) return true;
            if (that == null || this == null) return false;

            return _elements.Equals(that._elements);
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>An int.</returns>
        public override int GetHashCode()
        {
            return _elements == null ? 0 : HashCode.Combine(_elements);
        }

        /// <summary>
        /// 指定されたトークンリスト内に、ExpressionRuleと同じ順序のSurfaceのパターンが存在するかどうかを判定する。
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>A bool.</returns>
        public bool Match(List<TokenElement> tokens)
        {
            if (tokens.Count == 0)
            {
                // トークンリストが空の場合マッチすべき要素が無いのでFalse。
                return false;
            }

            // TODO: JAVAの実装ではこのようなループを使っていたが、LINQを使って見通しの良いコードへ置き換える。
            for (int i = 0; i < tokens.Count; i++)
            {
                bool result = true;
                for (int j = 0; j < _elements.Count; j++)
                {
                    if (tokens.Count <= i + j)
                    {
                        result = false;
                        break;
                    }
                    else if (!tokens[i + j].Surface.Equals(_elements[j].Surface))
                    {
                        result = false;
                    }
                }
                if (result)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
