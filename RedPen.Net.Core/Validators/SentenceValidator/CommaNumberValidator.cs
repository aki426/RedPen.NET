using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentecneValidator
{
    public sealed class CommaNumberValidator : Validator
    {
        public CommaNumberValidator() : base(new object[] { "max_num", 3 })
        {
        }

        private char comma;

        private int getMaxNum() => GetInt("max_num");

        public override void Validate(Sentence sentence)
        {
            string content = sentence.Content;
            int commaCount = 0;
            int position = 0;
            while ((position = content.IndexOf(this.comma)) != -1)
            {
                commaCount++;
                // position + 1 から最後までの文字列を取得。
                content = content.Substring(position + 1);
            }
            if (getMaxNum() < commaCount)
            {
                addLocalizedError(sentence, commaCount, getMaxNum());
            }
        }

        /// <summary>
        /// Inits the.
        /// </summary>
        protected override void Init()
        {
            this.comma = this.SymbolTable.GetValueOrFallbackToDefault(SymbolType.COMMA);
        }
    }
}
