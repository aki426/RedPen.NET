﻿using System.Collections.Generic;

namespace RedPen.Net.Core.Tokenizer
{
    /// <summary>
    /// The red pen Tokenizer.
    /// </summary>
    public interface IRedPenTokenizer
    {
        /// <summary>
        /// Tokenize input sentence into tokens.
        /// </summary>
        /// <param name="sentence">input sentence</param>
        /// <returns>a set of tokens in the input sentence</returns>
        public List<TokenElement> Tokenize(string sentence);
    }
}