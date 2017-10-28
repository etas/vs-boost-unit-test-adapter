// https://github.com/csoltenborn/GoogleTestAdapter

using System;
using System.Text;

namespace BoostTestAdapter.Utility
{
    /// <summary>
    /// Byte-pattern utilities which allow fast lookup/search
    /// </summary>
    public static class ByteUtilities
    {
        /// <summary>
        /// Representation of a Boyer-Moore 'byte' search pattern (needle)
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public class BoyerMooreBytePattern
        {
            #region Members

            private int[] byteBasedJumpTable;
            private int[] offsetBasedJumpTable;
            private readonly byte[] _pattern = null;

            #endregion

            #region Constructors

            public BoyerMooreBytePattern(string pattern, Encoding encoding)
                : this(encoding?.GetBytes(pattern))
            {
            }

            public BoyerMooreBytePattern(byte[] pattern)
            {
                this._pattern = pattern;

                this.byteBasedJumpTable = CreateByteBasedJumpTable(pattern);
                this.offsetBasedJumpTable = CreateOffsetBasedJumpTable(pattern);
            }

            #endregion

            public byte[] GetPattern()
            {
                return _pattern;
            }

            public int CalculateJumpOffset(int offset, byte value)
            {
                int jumpOffset = Math.Max(GetPattern().Length - 1 - offset, 0);
                return Math.Max(offsetBasedJumpTable[jumpOffset], byteBasedJumpTable[value]);
            }

            private static int[] CreateByteBasedJumpTable(byte[] pattern)
            {
                int[] table = new int[byte.MaxValue + 1];

                for (int i = 0; i < table.Length; ++i)
                {
                    table[i] = pattern.Length;
                }

                if (pattern.Length > 0)
                {
                    for (int i = 0; i < pattern.Length - 1; ++i)
                    {
                        table[pattern[i]] = pattern.Length - 1 - i;
                    }
                }

                return table;
            }

            private static int[] CreateOffsetBasedJumpTable(byte[] pattern)
            {
                int[] table = new int[Math.Max(pattern.Length, 1)];

                int lastPrefixPosition = pattern.Length;

                for (int i = pattern.Length; i > 0; i--)
                {
                    if (IsPrefix(pattern, i))
                    {
                        lastPrefixPosition = i;
                    }

                    table[pattern.Length - i] = lastPrefixPosition - i + pattern.Length;
                }

                for (int i = 0; i < pattern.Length - 1; i++)
                {
                    int suffixLength = GetSuffixLength(pattern, i);
                    table[suffixLength] = pattern.Length - 1 - i + suffixLength;
                }

                return table;
            }

            private static bool IsPrefix(byte[] pattern, int position)
            {
                for (int i = position, j = 0; i < pattern.Length; i++, j++)
                {
                    if (pattern[i] != pattern[j])
                    {
                        return false;
                    }
                }

                return true;
            }

            private static int GetSuffixLength(byte[] pattern, int position)
            {
                int length = 0;

                for (int i = position, j = (pattern.Length - 1); (i >= 0) && (pattern[i] == pattern[j]); i--, j--)
                {
                    length++;
                }

                return length;
            }
        }

        /// <summary>
        /// Implementation of the Boyer-Moore algorithm 
        /// (after https://en.wikipedia.org/wiki/Boyer%E2%80%93Moore_string_search_algorithm, Java version)
        /// </summary>
        /// <param name="value">The haystack in which to search the pattern of interest</param>
        /// <param name="pattern">The needle to search for</param>
        /// <returns>Index of the first occurence of <code>pattern</code>, or <code>-1</code> if <code>pattern</code> is not contained in <code>bytes</code></returns>
        public static int IndexOf(this byte[] value, byte[] pattern)
        {
            return IndexOf(value, new BoyerMooreBytePattern(pattern));
        }

        /// <summary>
        /// Implementation of the Boyer-Moore algorithm 
        /// (after https://en.wikipedia.org/wiki/Boyer%E2%80%93Moore_string_search_algorithm, Java version)
        /// </summary>
        /// <param name="value">The haystack in which to search the pattern of interest</param>
        /// <param name="pattern">The needle to search for</param>
        /// <returns>Index of the first occurence of <code>pattern</code>, or <code>-1</code> if <code>pattern</code> is not contained in <code>bytes</code></returns>
        public static int IndexOf(this byte[] value, BoyerMooreBytePattern pattern)
        {
            if ((pattern == null) || (pattern.GetPattern().Length == 0))
            {
                return 0;
            }

            for (int posInBytes = (pattern.GetPattern().Length - 1); posInBytes < value.Length;)
            {
                int posInPattern;
                for (posInPattern = (pattern.GetPattern().Length - 1); pattern.GetPattern()[posInPattern] == value[posInBytes]; --posInBytes, --posInPattern)
                {
                    if (posInPattern == 0)
                    {
                        return posInBytes;
                    }
                }

                posInBytes += pattern.CalculateJumpOffset(posInPattern, value[posInBytes]);
            }

            return -1;
        }
    }
}