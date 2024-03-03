using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace redpen_core.parser.common
{
    public class Line
    {
        /// <summary>
        /// value returned for comparison if a character is escaped
        /// </summary>
        internal static readonly char EscapedCharacterValue = 'ø';

        /// <summary>
        /// a list of offsets for each character
        /// </summary>
        internal List<int> offsets = new List<int>();

        /// <summary>
        /// the character list for the line
        /// </summary>
        internal List<char> characters = new List<char>();

        /// <summary>
        /// the text of the line
        /// </summary>
        internal string text;

        /// <summary>
        /// marks erased characters as invalid
        /// </summary>
        internal List<bool> valid = new List<bool>();

        /// <summary>
        /// remembers which characters were escaped in the original string
        /// </summary>
        internal List<bool> escaped = new List<bool>();

        /// <summary>
        /// Whole line is erased.
        /// </summary>
        internal bool erased = false;

        internal int lineNo = 0;
        internal bool allSameCharacter = false;
        internal bool inBlock = false;
        internal int sectionLevel = 0;
        internal int listLevel = 0;
        internal bool listStart = false;
        internal string inlineMarkupDelimiters = " ";

        /// <summary>
        /// The erase style.
        /// </summary>
        public enum EraseStyle
        {
            All,
            None,
            Markers,
            InlineMarkup,
            PreserveLabel,
            PreserveAfterLabel,
            CloseMarkerContainsDelimiters
        }

        public Line(string str, int lineNo)
        {
            this.lineNo = lineNo;
            this.text = str;
        }

        /// <summary>
        /// Erase length characters in the line, starting at pos
        /// </summary>
        /// <param name="pos">start position</param>
        /// <param name="length">length to erase</param>
        public void Erase(int pos, int length)
        {
            if ((pos >= 0) && (pos < valid.Count))
            {
                for (int i = pos; (i < valid.Count) && (i < pos + length); i++)
                {
                    valid[i] = false;
                }
            }
        }

        /// <summary>
        /// Erase the whole line
        /// </summary>
        public void Erase()
        {
            this.valid = this.valid.Select(x => false).ToList();
            erased = true;
        }

        /**
 * Erase all occurrences of the given string
 *
 * @param segment segment to be erased
 */

        public void Erase(string segment)
        {
            for (int i = 0; i < characters.Count; i++)
            {
                bool found = true;
                for (int j = 0; j < segment.Length; j++)
                {
                    if (CharAt(j + i) != segment[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    Erase(i, segment.Length);
                    i += segment.Length;
                }
            }
        }

        /// <summary>
        /// Return the length of the line
        /// </summary>
        /// <returns></returns>
        public int Length()
        {
            return characters.Count;
        }

        public char CharAt(int i)
        {
            if ((i >= 0) && (i < characters.Count))
            {
                return characters[i];
            }
            return '\0';
        }

        /// <summary>
        /// Return the character at position i, or the escape character if the character is escaped
        /// </summary>
        /// <param name="i"></param>
        /// <param name="IncludeInvalid">true if include invalid</param>
        /// <returns></returns>
        public char CharAt(int i, bool IncludeInvalid = false)
        {
            if ((i >= 0) && (i < characters.Count))
            {
                if (escaped[i])
                {
                    return EscapedCharacterValue;
                }
                if (IncludeInvalid || valid[i])
                {
                    return characters[i];
                }
            }

            return '\0';
        }

        public bool StartsWith(string s)
        {
            // this.textの先頭がstring sと一致する場合Trueを返す。

            if (s == null)
            {
                return false;
            }

            return text.StartsWith(s);
        }

        public bool IsValid(int i)
        {
            if ((i >= 0) && (i < valid.Count))
            {
                return valid[i];
            }
            return false;
        }
    }
}