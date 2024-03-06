using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace redpen_core.tokenizer
{
    /// <summary>
    /// The red pen tokenizer.
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
