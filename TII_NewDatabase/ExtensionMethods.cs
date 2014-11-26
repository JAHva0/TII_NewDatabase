// <summary> Custom Extension methods used in this project to make my life easier. </summary>

namespace TII_NewDatabase
{
    using System;
    
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
            return date.Year.ToString().PadLeft(2, '0') + date.Month.ToString().PadLeft(2, '0') + date.Day.ToString().PadLeft(2, '0');
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
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
