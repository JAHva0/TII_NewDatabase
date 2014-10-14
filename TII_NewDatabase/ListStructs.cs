// <summary> Structs for use in holding information pulled from the database at run-time. </summary>

namespace TII_NewDatabase
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    
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