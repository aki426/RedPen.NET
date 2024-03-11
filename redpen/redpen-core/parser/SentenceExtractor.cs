using System.Text;
using System.Text.RegularExpressions;
using NLog;
using redpen_core.config;
using redpen_core.util;

namespace redpen_core.parser
{
    public class SentenceExtractor
    {
        private static Logger LOG = LogManager.GetCurrentClassLogger();

        private Regex fullStopPattern;

        private char[] fullStopList;
        private char[] rightQuotationList;

        // TODO: make white words configurable.
        private static readonly List<string> WHITE_WORDS = new List<string>(){
            "Mr.", "Mrs.", "Miss.", "Dr.",
            "genn.ai", "Co., Ltd.",
            "a.m.", "p.m.",
            "U.S.A.",
            "Jan.", "Feb.", "Mar.", "Apr.",
            "May.", "Jun.", "Jul.", "Aug.",
            "Sep.", "Oct.", "Nov.", "Dec.", "Feb.",
            "B.C", "A.D." };

        private EndOfSentenceDetector endOfSentenceDetector;

        //    // reference to the symbol table used to create us
        private SymbolTable symbolTable = null;

        //    /**
        //     * Constructor.
        //     *
        //     * @param fullStopList set of end of sentence characters
        //     */
        private SentenceExtractor(char[] fullStopList)
            : this(fullStopList, extractRightQuotations(Configuration.Builder().Build().getSymbolTable()))
        {
        }

        //    /**
        //     * Constructor.
        //     *
        //     * @param SymbolTable SymbolTable
        //     */
        public SentenceExtractor(SymbolTable symbolTable)
            : this(extractPeriods(symbolTable), extractRightQuotations(symbolTable))
        {
            this.symbolTable = symbolTable;
        }

        //    /**
        //     * Constructor.
        //     */
        private SentenceExtractor(char[] fullStopList, char[] rightQuotationList)
        {
            this.fullStopList = fullStopList;
            this.rightQuotationList = rightQuotationList;

            this.fullStopPattern = this.constructEndSentencePattern();
            this.endOfSentenceDetector = new EndOfSentenceDetector(this.fullStopPattern, WHITE_WORDS);
        }

        private static char[] extractPeriods(SymbolTable symbolTable)
        {
            char[] periods = new char[]{
                symbolTable.GetValueOrFallbackToDefault(SymbolType.FULL_STOP),
                symbolTable.GetValueOrFallbackToDefault(SymbolType.QUESTION_MARK),
                symbolTable.GetValueOrFallbackToDefault(SymbolType.EXCLAMATION_MARK)
            };
            LOG.Info("\"" + periods.ToString() + "\" are added as a end of sentence characters");
            return periods;
        }

        /// <summary>
        /// extracts the right quotations.
        /// </summary>
        /// <param name="symbolTable">The symbol table.</param>
        /// <returns>An array of char.</returns>
        private static char[] extractRightQuotations(SymbolTable symbolTable)
        {
            char[] rightQuotations = new char[]{
                    symbolTable.GetValueOrFallbackToDefault(SymbolType.RIGHT_SINGLE_QUOTATION_MARK),
                    symbolTable.GetValueOrFallbackToDefault(SymbolType.RIGHT_DOUBLE_QUOTATION_MARK)
            };
            LOG.Info("\"" + rightQuotations.ToString() + "\" are added as a right quotation characters");
            return rightQuotations;
        }

        private static string handleSpecialCharacter(char endChar)
        {
            if (endChar == '.')
            {
                return "\\.";
            }
            else if (endChar == '?')
            {
                return "\\?";
            }
            else if (endChar == '!')
            {
                return "\\!";
            }
            else
            {
                return endChar.ToString();
            }
        }

        private void generateSimplePattern(char[] endCharacters, StringBuilder patternString)
        {
            patternString.Append("[");
            foreach (char endChar in endCharacters)
            {
                patternString.Append(handleSpecialCharacter(endChar));
            }
            patternString.Append("]");
        }

        //    /**
        //     * Given a set of sentence end characters, construct the
        //     * regex to detect end sentences.
        //     * This method is protected permission just for testing.
        //     *
        //     * @return regex periodPattern to detect end sentences
        //     */
        private Regex constructEndSentencePattern()
        {
            if (this.fullStopList == null || this.fullStopList.Length == 0)
            {
                throw new ArgumentNullException("No end character is specified");
            }

            StringBuilder patternString = new StringBuilder();
            generateSimplePattern(this.fullStopList, patternString);
            patternString.Append("[");
            foreach (char rightQuotation in rightQuotationList)
            {
                patternString.Append(rightQuotation);
            }
            patternString.Append("]?");

            return new Regex(patternString.ToString()); ;
        }

        //    /**
        //     * Get Sentence lists.
        //     *
        //     * @param line              Input line which can contain more than one sentences
        //     * @param sentencePositions List of extracted sentences
        //     * @return remaining line
        //     */
        public int extract(string line, List<(int first, int second)> sentencePositions)
        {
            int startPosition = 0;
            int periodPosition = endOfSentenceDetector.GetSentenceEndPosition(line, 0);

            while (periodPosition >= 0)
            {
                sentencePositions.Add(new(startPosition, periodPosition + 1));
                startPosition = periodPosition + 1;
                periodPosition = endOfSentenceDetector.GetSentenceEndPosition(line, startPosition);
            }
            return startPosition;
        }

        //    /**
        //     * Given string, return sentence end position.
        //     *
        //     * @param str input string
        //     * @return position of full stop when there is a full stop, -1 otherwise
        //     */
        public int GetSentenceEndPosition(string str)
        {
            return this.endOfSentenceDetector.GetSentenceEndPosition(str, 0);
        }

        //    /**
        //     * Return the string that should be used to re-join lines broken with \n in
        //     * <p>
        //     * For English, this is a space.
        //     * For Japanese, it is an empty string.
        //     *
        //     * The specification of this string should be moved to part of the configuration
        //     *
        //     * @return a string used to join lines that have been 'broken'
        //     */
        public string getBrokenLineSeparator()
        {
            // 日本語設定のみのハードコーディングだが、日本語は空文字列で
            // それ以外の言語は半角空白をBlokenLineSeparatorとする、という2択で良いのか疑問が残る。
            // TODO: 多言語対応の際には、このメソッドの実装を見直す。
            return (symbolTable != null) && (symbolTable.Lang == "ja") ? "" : " ";
        }
    }
}
