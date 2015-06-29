// <summary> Custom Extension methods used in this project to make my life easier. </summary>

namespace TII_NewDatabase
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    
    /// <summary>
    /// Custom Extension methods to make my life easier.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Extension method for determining if a given term appears in a string array.
        /// </summary>
        /// <param name="string_array">The string array to check.</param>
        /// <param name="search_term">The term to search for.</param>
        /// <returns>True, if the search term was found in the array.</returns>
        public static bool Contains(this string[] string_array, string search_term)
        {
            foreach (string s in string_array)
            {
                if (s == search_term)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Converts a DateTime to a string Formatted "YYMMDD".
        /// </summary>
        /// <param name="date">The DateTime to Convert.</param>
        /// <returns> A string representation of the value.</returns>
        public static string ToYYMMDDString(this DateTime date)
        {
            return date.Year.ToString().Substring(2) + date.Month.ToString().PadLeft(2, '0') + date.Day.ToString().PadLeft(2, '0');
        }

        /// <summary>
        /// Converts a string in the format "YYMMDD" to it's <see cref="DateTime"/> representation.
        /// </summary>
        /// <param name="str_date">The string to convert.</param>
        /// <returns> A Date generated from the string value. </returns>
        public static DateTime ToYYMMDD(this string str_date)
        {
            try
            {
                return new DateTime(
                    Convert.ToInt32("20" + str_date.Substring(0, 2)),
                    Convert.ToInt32(str_date.Substring(2, 2)),
                    Convert.ToInt32(str_date.Substring(4, 2)));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new ArgumentOutOfRangeException("String must be exactly 6 chars long", ex);
            }
            catch (FormatException ex)
            {
                throw new FormatException("String must be exactly 6 integers", ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Compares this string to another, and returns a percentage value indicating how similar the two strings are.
        /// </summary>
        /// <param name="str">The string calling the function.</param>
        /// <param name="stringToCompare">The string to be compared against.</param>
        /// <returns>A double indicating how similar this string is to another. </returns>
        public static double SimilarityFactor(this string str, string stringToCompare)
        {
            List<string> pairs1 = WordLetterPairs(str.ToUpper());
            List<string> pairs2 = WordLetterPairs(stringToCompare.ToUpper());

            int intersection = 0;
            int union = pairs1.Count + pairs2.Count;

            for (int i = 0; i < pairs1.Count; i++)
            {
                for (int j = 0; j < pairs2.Count; j++)
                {
                    if (pairs1[i] == pairs2[j])
                    {
                        intersection++;
                        pairs2.RemoveAt(j);
                        break;
                    }
                }
            }

            return (2.0 * intersection) / union;
        }

        /// <summary>
        /// Creates a formatted string from a list.
        /// </summary>
        /// <param name="l">A list of strings to combine.</param>
        /// <param name="ampersand">Should the last two items be separated by an ampersand.</param>
        /// <returns>A string, formatted as directed.</returns>
        public static string ToFormattedList(this List<string> l, bool ampersand = true)
        {
            string str = string.Empty;

            // Sort the list into default order, just in case.
            l.Sort();

            // Add to the return string depending on the number of items in the list:
            // 1 - just the one item that was passed
            // 2 - the two items seperated by either an ampersand or a comma
            // 3 or more - items seperated by a comma until
            if (l.Count == 1)
            {
                str = l[0];
            }
            else if (l.Count == 2)
            {
                if (ampersand)
                {
                    str = l[0] + " & " + l[1];
                }
                else
                {
                    str = l[0] + ", " + l[1];
                }
            }
            else if (l.Count > 2)
            {
                for (int i = 0; i < l.Count - 2; i++)
                {
                    str += l[i] + ", ";
                }

                if (ampersand)
                {
                    str += l[l.Count - 2] + " & " + l[l.Count - 1];
                }
                else
                {
                    str += l[l.Count - 2] + ", " + l[l.Count - 1];
                }
            }

            return str;
        }

        /// <summary>
        /// Used by <see cref="SimilartyFactor"/>. Splits the string passed into two letter pairs by word.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <returns> A collection of two letter pairs. </returns>
        private static List<string> WordLetterPairs(string str)
        {
            List<string> allPairs = new List<string>();

            string[] words = Regex.Split(str, @"\s");

            for (int w = 0; w < words.Length; w++)
            {
                if (!string.IsNullOrEmpty(words[w]))
                {
                    string[] pairsInWord = LetterPairs(words[w]);

                    for (int p = 0; p < pairsInWord.Length; p++)
                    {
                        allPairs.Add(pairsInWord[p]);
                    }
                }
            }

            return allPairs;
        }

        /// <summary>
        /// Used by <see cref="SimilartyFactor"/>. Splits the word passed into two letter pairs, i.e. ABCD becomes AB BC CD.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <returns> An array of two letter pairs. </returns>
        private static string[] LetterPairs(string str)
        {
            int numPairs = str.Length - 1;

            string[] pairs = new string[numPairs];

            for (int i = 0; i < numPairs; i++)
            {
                pairs[i] = str.Substring(i, 2);
            }

            return pairs;
        }
    }
}
