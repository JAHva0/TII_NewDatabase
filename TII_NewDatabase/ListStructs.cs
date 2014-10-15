// <summary> Structs for use in holding information pulled from the database at run-time. </summary>

namespace TII_NewDatabase
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    
    /// <summary>
    /// Struct for holding Company Info used to sort/filter Companies in the ListBox.
    /// </summary>
    public struct CompanyListItem
    {
        /// <summary>Company Name.</summary>
        public string Name;

        /// <summary>Can be DC, MD or BOTH.</summary>
        public string DCorMD;

        /// <summary>Indicates if the Company has any active buildings associated or not.</summary>
        public bool IsActive;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompanyListItem"/> struct.
        /// </summary>
        /// <param name="name">Company Name.</param>
        /// <param name="dc_or_md">Can be DC, MD or BOTH.</param>
        /// <param name="active">Indicates if the Company has any active buildings associated or not.</param>
        public CompanyListItem(string name, string dc_or_md, bool active)
        {
            this.Name = name;
            this.DCorMD = dc_or_md;
            this.IsActive = active;
        }
    }

    /// <summary>
    /// Struct for holding Building Info used to sort/filter Buildings in the ListBox.
    /// </summary>
    public struct BuildingListItem
    {
        /// <summary>Building Address.</summary>
        public string Address;

        /// <summary>Can be DC, MD or BOTH.</summary>
        public string DCorMD;

        /// <summary>Indicates if the Company has any active buildings associated or not.</summary>
        public bool IsActive;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingListItem"/> struct.
        /// </summary>
        /// <param name="address">Building Address.</param>
        /// <param name="dc_or_md">Can be DC or MD.</param>
        /// <param name="active">Indicates if the building is listed as active.</param>
        public BuildingListItem(string address, string dc_or_md, bool active)
        {
            this.Address = address;
            this.DCorMD = dc_or_md;
            this.IsActive = active;
        }
    }

    /// <summary>
    /// Generic class for holding and sorting database information.
    /// </summary>
    public class DatabaseList
    {
        /// <summary> The query used to populate this class. Can be re-used when we need to re-populate the list. </summary>
        private string query;

        /// <summary> Dictionary used to hold all of the information obtained by the query. </summary>
        private Dictionary<int, DatabaseListItem> list_collection;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseList" /> class.
        /// </summary>
        /// <param name="query"> An SQL query used to populate the list. </param>
        public DatabaseList(string query)
        {
            this.query = query;
            this.Regenerate();
        }

        /// <summary>
        /// Method to refill the list collection with fresh data using the stored query.
        /// </summary>
        public void Regenerate()
        {
            if (this.query == null)
            {
                throw new Exception("Must assign a query before attempting to regenerate the Database List");
            }

            this.list_collection = new Dictionary<int, DatabaseListItem>();

            foreach (DataRow row in SQL.Query.Select(this.query).Rows)
            {
                this.list_collection.Add(
                                         Convert.ToInt32(row[0].ToString()),
                                         new DatabaseListItem(
                                                              row[1].ToString(),
                                                              row[2].ToString(),
                                                              Convert.ToBoolean(row[3].ToString())));
            }
        }

        /// <summary>
        /// Returns a string array of items pulled from the list collection, filtered per the arguments passed.
        /// </summary>
        /// <param name="state">Currently, only supports options "DC", "MD", or "BOTH".</param>
        /// <param name="name">Selects only those items which match part of this argument.</param>
        /// <param name="active">Select only items listed as active, or if false, select all items.</param>
        /// <returns>An array of strings filtered based on the parameters passed in.</returns>
        public string[] GetFilteredList(string state, string name, bool active = false)
        {
            // Roll through the list and pull out those items which are active and match state and contain any part of name.
            IEnumerable<string> filtered_list;
            if (active)
            {
                // We only want items listed as active.
                filtered_list =
                    from i in this.list_collection.Values
                    where i.IsActive
                       && i.DCorMD.Contains(state)
                       && i.Title.ToLower().Contains(name.ToLower())
                    select i.Title;
            }
            else
            {
                // We want all items, active or not.
                filtered_list =
                    from i in this.list_collection.Values
                    where i.DCorMD.Contains(state)
                       && i.Title.ToLower().Contains(name.ToLower())
                    select i.Title;
            }

            return filtered_list.ToArray();
        }

        /// <summary>
        /// Gets the ID of an item with the exact title provided.
        /// </summary>
        /// <param name="name">The exact title of an element in the list collection.</param>
        /// <returns>An integer corresponding to the ID of the requested element.</returns>
        public int GetItemID(string name)
        {
            var id = (from i in this.list_collection
                      where i.Value.Title == name
                      select i.Key).SingleOrDefault();

            if (id == 0)
            {
                throw new ArgumentNullException(string.Format("GetItemID for '{0}' returned no results", name));
            }

            return (int)id;
        }

        /// <summary>
        /// Struct for holding database information used to sort and filter.
        /// </summary>
        private struct DatabaseListItem
        {
            /// <summary>The Item title.</summary>
            public string Title;

            /// <summary>Can be DC, MD or BOTH.</summary>
            public string DCorMD;

            /// <summary>Indicates if the Item has any active buildings associated or not.</summary>
            public bool IsActive;

            /// <summary>
            /// Initializes a new instance of the <see cref="DatabaseListItem"/> struct.
            /// </summary>
            /// <param name="title">Item Title.</param>
            /// <param name="dc_or_md">Can be DC or MD.</param>
            /// <param name="active">Indicates if the item is listed as active.</param>
            public DatabaseListItem(string title, string dc_or_md, bool active)
            {
                this.Title = title;
                this.DCorMD = dc_or_md;
                this.IsActive = active;
            }
        }
    }
}