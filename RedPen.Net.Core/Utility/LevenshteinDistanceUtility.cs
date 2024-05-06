using System;
using System.Collections.Generic;
using System.Text;

namespace RedPen.Net.Core.Utility
{
    /// <summary>
    /// Levenshtein Distance(a.k.a. Edit Distance)
    /// <p>
    /// For given two strings, provide the minimum number
    /// of single-character edits (i.e. insertions, deletions
    /// or substitutions). The default cost for each edit
    /// is 1, and each value is configurable.
    /// </summary>
    public class LevenshteinDistanceUtility
    {
        /// <summary>
        /// A constant holding the default insertion cost.
        /// </summary>
        public const int DEFAULT_INSERTION_COST = 1;

        /// <summary>
        /// A constant holding the default deletion cost.
        /// </summary>
        public const int DEFAULT_DELETION_COST = 1;

        /// <summary>
        /// A constant holding the default substitution cost.
        /// </summary>
        public const int DEFAULT_SUBSTITUTION_COST = 1;

        private static int INSERTION_COST;
        private static int DELETION_COST;
        private static int SUBSTITUTION_COST;

        static LevenshteinDistance()
        {
            INSERTION_COST = DEFAULT_INSERTION_COST;
            DELETION_COST = DEFAULT_DELETION_COST;
            SUBSTITUTION_COST = DEFAULT_SUBSTITUTION_COST;
        }

        /// <summary>
        /// Default Constructor.
        /// </summary>
        private LevenshteinDistance()
        {
        }

        /// <summary>
        /// Get the cost for "insertion".
        /// </summary>
        /// <returns>the cost for "insertion"</returns>
        public static int GetInsertionCost()
        {
            return INSERTION_COST;
        }

        /// <summary>
        /// Set the cost for "insertion".
        /// </summary>
        /// <param name="cost">a cost for "insertion"</param>
        public static void SetInsertionCost(int cost)
        {
            INSERTION_COST = cost;
        }

        /// <summary>
        /// Get the cost for "deletion".
        /// </summary>
        /// <returns>the cost for "deletion"</returns>
        public static int GetDeletionCost()
        {
            return DELETION_COST;
        }

        /// <summary>
        /// Set the cost for "deletion".
        /// </summary>
        /// <param name="cost">a cost for "deletion"</param>
        public static void SetDeletionCost(int cost)
        {
            DELETION_COST = cost;
        }

        /// <summary>
        /// Get the cost for "substitution".
        /// </summary>
        /// <returns>the cost for "substitution"</returns>
        public static int GetSubstitutionCost()
        {
            return SUBSTITUTION_COST;
        }

        /// <summary>
        /// Set the cost for "substitution".
        /// </summary>
        /// <param name="cost">a cost for "substitution"</param>
        public static void SetSubstitutionCost(int cost)
        {
            SUBSTITUTION_COST = cost;
        }

        /// <summary>
        /// Get the Levenshtein distance for given two strings.
        /// </summary>
        /// <param name="a">a string</param>
        /// <param name="b">one another string</param>
        /// <returns>Levenshtein distance</returns>
        /// <see>
        /// <a href="http://en.wikipedia.org/wiki/Levenshtein_distance">http://en.wikipedia.org/wiki/Levenshtein_distance</a>
        /// </see>
        public static int GetDistance(string a, string b)
        {
            if (a == null && b == null)
            {
                return 0;
            }
            if (a == null && b != null)
            {
                return b.Length * INSERTION_COST;
            }
            if (a != null && b == null)
            {
                return a.Length * INSERTION_COST;
            }

            int lengthA = a.Length;
            int lengthB = b.Length;
            int[,] distance = new int[lengthA + 1, lengthB + 1];

            // Initialization
            for (int i = 0; i < lengthA + 1; i++)
            {
                distance[i, 0] = i * DELETION_COST;
            }
            for (int j = 0; j < lengthB + 1; j++)
            {
                distance[0, j] = j * INSERTION_COST;
            }

            for (int i = 1; i < lengthA + 1; i++)
            {
                for (int j = 1; j < lengthB + 1; j++)
                {
                    if (a[i - 1] == b[j - 1])
                    {
                        distance[i, j] = distance[i - 1, j - 1];
                    }
                    else
                    {
                        distance[i, j] = System.Math.Min(System.Math.Min(
                                            distance[i - 1, j] + DELETION_COST,
                                            distance[i, j - 1] + INSERTION_COST),
                                distance[i - 1, j - 1] + SUBSTITUTION_COST);
                    }
                }
            }

            return distance[lengthA, lengthB];
        }
    }
}
