using System.Text.RegularExpressions;
using NLog;

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

        //private EndOfSentenceDetector endOfSentenceDetector;

        //    // reference to the symbol table used to create us
        //    private SymbolTable symbolTable = null;

        //    /**
        //     * Constructor.
        //     *
        //     * @param fullStopList set of end of sentence characters
        //     */
        //    SentenceExtractor(char...fullStopList)
        //    {
        //        this(fullStopList, extractRightQuotations(Configuration.builder().build().getSymbolTable()));
        //    }

        //    /**
        //     * Constructor.
        //     *
        //     * @param symbolTable symbolTable
        //     */
        //    public SentenceExtractor(SymbolTable symbolTable)
        //    {
        //        this(extractPeriods(symbolTable), extractRightQuotations(symbolTable));
        //        this.symbolTable = symbolTable;
        //    }

        //    /**
        //     * Constructor.
        //     */
        //    SentenceExtractor(char[] fullStopList, char[] rightQuotationList)
        //    {
        //        this.fullStopList = fullStopList;
        //        this.rightQuotationList = rightQuotationList;
        //        this.fullStopPattern = this.constructEndSentencePattern();
        //        this.endOfSentenceDetector = new EndOfSentenceDetector(
        //                this.fullStopPattern, WHITE_WORDS);
        //    }

        //    private static char[] extractPeriods(SymbolTable symbolTable)
        //    {
        //        char[] periods = new char[]{
        //            symbolTable.getValueOrFallbackToDefault(FULL_STOP),
        //            symbolTable.getValueOrFallbackToDefault(QUESTION_MARK),
        //            symbolTable.getValueOrFallbackToDefault(EXCLAMATION_MARK)
        //    };
        //        LOG.info("\"" + Arrays.toString(periods) + "\" are added as a end of sentence characters");
        //        return periods;
        //    }

        //    private static char[] extractRightQuotations(SymbolTable symbolTable)
        //    {
        //        char[] rightQuotations = new char[]{
        //            symbolTable.getValueOrFallbackToDefault(RIGHT_SINGLE_QUOTATION_MARK),
        //            symbolTable.getValueOrFallbackToDefault(RIGHT_DOUBLE_QUOTATION_MARK)
        //    };
        //        LOG.info("\"" + Arrays.toString(rightQuotations) + "\" are added as a right quotation characters");
        //        return rightQuotations;
        //    }

        //    private void generateSimplePattern(char[] endCharacters, StringBuilder patternString)
        //    {
        //        patternString.append("[");
        //        for (char endChar : endCharacters)
        //        {
        //            patternString.append(handleSpecialCharacter(endChar));
        //        }
        //        patternString.append("]");
        //    }

        //    private static String handleSpecialCharacter(char endChar)
        //    {
        //        if (endChar == '.')
        //        {
        //            return "\\.";
        //        }
        //        else if (endChar == '?')
        //        {
        //            return "\\?";
        //        }
        //        else if (endChar == '!')
        //        {
        //            return "\\!";
        //        }
        //        else
        //        {
        //            return String.valueOf(endChar);
        //        }
        //    }

        //    private static <E> List<E> generateUmList(E...args)
        //    {
        //        return new ArrayList<>(Arrays.asList(args));
        //    }

        //    /**
        //     * Get Sentence lists.
        //     *
        //     * @param line              Input line which can contain more than one sentences
        //     * @param sentencePositions List of extracted sentences
        //     * @return remaining line
        //     */
        //    public int extract(String line, List<Pair<Integer, Integer>> sentencePositions)
        //    {
        //        int startPosition = 0;
        //        int periodPosition = endOfSentenceDetector.getSentenceEndPosition(line, 0);
        //        while (periodPosition >= 0)
        //        {
        //            sentencePositions.add(new Pair<>(startPosition, periodPosition + 1));
        //            startPosition = periodPosition + 1;
        //            periodPosition = endOfSentenceDetector.getSentenceEndPosition(line, startPosition);
        //        }
        //        return startPosition;
        //    }

        //    /**
        //     * Given string, return sentence end position.
        //     *
        //     * @param str input string
        //     * @return position of full stop when there is a full stop, -1 otherwise
        //     */
        //    public int getSentenceEndPosition(String str)
        //    {
        //        return endOfSentenceDetector.getSentenceEndPosition(str, 0);
        //    }

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
        //    public String getBrokenLineSeparator()
        //    {
        //        return (symbolTable != null) && (symbolTable.getLang().equals("ja")) ? "" : " ";
        //    }

        //    /**
        //     * Given a set of sentence end characters, construct the
        //     * regex to detect end sentences.
        //     * This method is protected permission just for testing.
        //     *
        //     * @return regex periodPattern to detect end sentences
        //     */
        //    Pattern constructEndSentencePattern()
        //    {
        //        if (this.fullStopList == null || this.fullStopList.length == 0)
        //        {
        //            throw new IllegalArgumentException("No end character is specified");
        //        }
        //        StringBuilder patternString = new StringBuilder();
        //        generateSimplePattern(this.fullStopList, patternString);
        //        patternString.append("[");
        //        for (char rightQuotation : rightQuotationList)
        //        {
        //            patternString.append(rightQuotation);
        //        }
        //        patternString.append("]?");
        //        return Pattern.compile(patternString.toString());
        //    }
    }
}
