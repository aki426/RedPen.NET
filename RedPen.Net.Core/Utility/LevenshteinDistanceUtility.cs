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
    /// <p>
    /// 1文字の挿入、削除、置換によって2つの文字列が同じになる最小回数を求める。
    /// 挿入、削除、置換のコストはデフォルトで1であり、それぞれのコスト値は設定により変更可能。
    /// </summary>
    public record LevenshteinDistanceUtility
    {
        /// <summary>A constant holding the default insertion cost.</summary>
        public static readonly int DEFAULT_INSERTION_COST = 1;

        /// <summary>A constant holding the default deletion cost.</summary>
        public static readonly int DEFAULT_DELETION_COST = 1;

        /// <summary>A constant holding the default substitution cost.</summary>
        public static readonly int DEFAULT_SUBSTITUTION_COST = 1;

        /// <summary>挿入コスト</summary>
        public int InsertionCost { get; init; }

        /// <summary>削除コスト</summary>
        public int DeletionCost { get; init; }

        /// <summary>置換コスト</summary>
        public int SubstitutionCost { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LevenshteinDistanceUtility"/> class.
        /// MEMO: デフォルトコンストラクタ。編集コストはデフォルト値を使用する。
        /// </summary>
        public LevenshteinDistanceUtility() : this(DEFAULT_INSERTION_COST, DEFAULT_DELETION_COST, DEFAULT_SUBSTITUTION_COST)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LevenshteinDistanceUtility"/> class.
        /// </summary>
        /// <param name="insertionCost">The insertion cost.</param>
        /// <param name="deletionCost">The deletion cost.</param>
        /// <param name="substitutionCost">The substitution cost.</param>
        public LevenshteinDistanceUtility(int insertionCost, int deletionCost, int substitutionCost)
        {
            InsertionCost = insertionCost;
            DeletionCost = deletionCost;
            SubstitutionCost = substitutionCost;

            Cache = new Dictionary<(string a, string b), int>();

            // 編集コストがすべて同じなら、文字列aとbを入れ替えても計算結果は同じなので、メモ化キャッシュから引く時にも順番を考慮しなくてよい。
            isIgnoreSequence = InsertionCost == DeletionCost && DeletionCost == SubstitutionCost;
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
        public int GetDistance(string a, string b)
        {
            if (a == null && b == null)
            {
                return 0;
            }
            if (a == null && b != null)
            {
                return b.Length * InsertionCost;
            }
            if (a != null && b == null)
            {
                return a.Length * InsertionCost;
            }

            int lengthA = a.Length;
            int lengthB = b.Length;
            int[,] distance = new int[lengthA + 1, lengthB + 1];

            // Initialization
            for (int i = 0; i < lengthA + 1; i++)
            {
                distance[i, 0] = i * DeletionCost;
            }
            for (int j = 0; j < lengthB + 1; j++)
            {
                distance[0, j] = j * InsertionCost;
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
                        distance[i, j] = Math.Min(
                            Math.Min(distance[i - 1, j] + DeletionCost, distance[i, j - 1] + InsertionCost),
                            distance[i - 1, j - 1] + SubstitutionCost);
                    }
                }
            }

            return distance[lengthA, lengthB];
        }

        /// <summary>LevenshteinDistanceメモ化のためのキャッシュ</summary>
        public Dictionary<(string a, string b), int> Cache { get; }

        /// <summary>LevenStein距離が与える文字列の順番によらず一定であることのフラグ。順番無視できる場合True。</summary>
        private bool isIgnoreSequence;

        /// <summary>
        /// メモ化キャッシュを用いた計算コスト削減版関数。
        /// </summary>
        /// <param name="a">The a.</param>
        /// <param name="b">The b.</param>
        /// <returns>LevenSteinDistance.</returns>
        public int GetDistanceMemoize(string a, string b)
        {
            if (isIgnoreSequence)
            {
                if (Cache.ContainsKey((a, b)))
                {
                    return Cache[(a, b)];
                }
                else if (Cache.ContainsKey((b, a)))
                {
                    return Cache[(b, a)];
                }
                else
                {
                    int distance = GetDistance(a, b);
                    Cache[(a, b)] = distance;
                    return distance;
                }
            }
            else
            {
                if (Cache.ContainsKey((a, b)))
                {
                    return Cache[(a, b)];
                }
                else
                {
                    int distance = GetDistance(a, b);
                    Cache[(a, b)] = distance;
                    return distance;
                }
            }
        }
    }
}
