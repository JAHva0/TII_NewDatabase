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
    }
}
