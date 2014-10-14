// <summary> Structs for use in holding information pulled from the database at run-time. </summary>

namespace TII_NewDatabase
{
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
}