// <summary> Provides a class structure with which to load items out of the company table. </summary>
namespace Database
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using Database;

    /// <summary>
    /// Initializes a class object that can be used to load and/or modify company information from the database.
    /// </summary>
    public class Company : BaseObject
    {
        /// <summary> Variable that contains the Company Name in string form.</summary>
        private string name;

        /// <summary> Struct that contains the address information for the company.</summary>
        private Address address;

        /// <summary> A list of the contacts associated with this company. </summary>
        private List<Contact> contact_list = new List<Contact>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Company"/> class for use in adding new companies to the database.
        /// </summary>
        public Company() 
            : base()
        { 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Company"/> class. Loads information related to the provided Company ID and
        /// stores them so that they may be quickly selected/edited.
        /// </summary>
        /// <param name="company_ID">The Database assigned Company_ID.</param>
        public Company(int company_ID) 
            : this()
        {
            DataTable tbl = SQL.Query.Select(
                "SELECT Company.ID, Company.Name, Street, City.Name as City, State.Abbreviation as State, Zip FROM Company " +
                "JOIN Address ON Address_ID = Address.ID " +
                "JOIN City ON City_ID = City.ID " +
                "JOIN State ON State_ID = State.ID " +
                "WHERE Company.ID = " + company_ID.ToString());
            this.LoadFromDatabase(BaseObject.AffirmOneRow(tbl));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Company"/> class. Loads information related to the provided Company ID and
        /// stores them so that they may be quickly selected/edited.
        /// </summary>
        /// <param name="companyName">A Company name that appears in the database.</param>
        public Company(string companyName)
        {
            DataTable tbl = SQL.Query.Select(
                "SELECT Company.ID, Company.Name, Street, City.Name as City, State.Abbreviation as State, Zip FROM Company " +
                "JOIN Address ON Address_ID = Address.ID " +
                "JOIN City ON City_ID = City.ID " +
                "JOIN State ON State_ID = State.ID " +
                "WHERE Company.Name = '" + companyName + "'");
            this.LoadFromDatabase(BaseObject.AffirmOneRow(tbl));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Company"/> class.
        /// </summary>
        /// <param name="companyData">A properly formatted <see cref="DataRow"/> from the SQL Table. </param>
        public Company(DataRow companyData)
        {
            this.LoadFromDatabase(companyData);
        }

        /// <summary> Gets or sets the Company Name Variable. Also triggers the edited Event.</summary>
        /// <value> The Company Name.</value>
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                // Check to be sure the new name is not empty, and is not exactly the same as the old value.
                if (value != string.Empty && value != this.name)
                {
                    // If it is a new insert, this.name will be blank anyways. No need to check ID.
                    this.BaseObject_Edited(this, "Name", this.name, value);
                    this.name = value;
                }
            }
        }

        /// <summary> Gets or sets the Company Address.Street variable. </summary>
        /// <value> The Street Address for the Company.</value>
        public string Street
        {
            get
            {
                return this.address.Street;
            }

            set
            {
                // Check to make sure we're not adding a blank entry, and that this is at least different from the old value.
                if (value != string.Empty && value != this.address.Street)
                {
                    this.BaseObject_Edited(this, "Street", this.address.Street, value);
                    this.address.Street = value;
                }
            }
        }

        /// <summary> Gets or sets the Company Address.City variable. </summary>
        /// <value> The City for the Company.</value>
        public string City
        {
            get
            {
                return this.address.City;
            }

            set
            {
                // Check to make sure we're not adding a blank entry, and that this is at least different from the old value.
                if (value != string.Empty && value != this.address.City)
                {
                    this.BaseObject_Edited(this, "City", this.address.City, value);
                    this.address.City = value;
                }
            }
        }

        /// <summary> Gets or sets the Company Address.State variable. </summary>
        /// <value> The State for the Company.</value>
        public string State
        {
            get
            {
                return this.address.State;
            }

            set
            {
                // Check to make sure we're not adding a blank entry, and that this is at least different from the old value.
                if (value != string.Empty && value != this.address.State)
                {
                    this.BaseObject_Edited(this, "State", this.address.State, value);
                    this.address.State = value;
                }
            }
        }

        /// <summary> Gets or sets the Company Address.Zip variable. </summary>
        /// <value> The Zip Code for the Company as a string.</value>
        public string Zip
        {
            get
            {
                return this.address.Zip;
            }

            set
            {
                // Check to make sure we're not adding a blank entry, and that this is at least different from the old value.
                if (value != string.Empty && value != this.address.Zip)
                {
                    this.BaseObject_Edited(this, "Zip", this.address.Zip, value);
                    this.address.Zip = value;
                }
            }
        }

        /// <summary> Gets the Address of all buildings associated with this company.</summary>
        /// <value> A string array with all associated building addresses. </value>
        public string[] AssociatedBuildings
        {
            get
            {
                List<string> buildings = new List<string>();
                foreach (DataRow r in SQL.Query.Select(
                    "SELECT Street FROM Building " +
                    "JOIN Address ON Address_ID = Address.ID " +
                    "WHERE Company_ID = " + this.ID.ToString()).Rows)
                {
                    buildings.Add(r["Street"].ToString());
                }

                return buildings.ToArray();
            }
        }

        /// <summary> Gets a list of contacts associated with this company. </summary>
        /// <value> A list of contacts that are assigned to this building.</value>
        public List<Contact> ContactList
        {
            get
            {
                return this.contact_list;
            }
        }

        /// <summary> Gets a condensed string of all of the information contained within this class. </summary>
        /// <value> A string of values separated by the pipe character. </value>
        public string ToCondensedString
        {
            get
            {
                return string.Format(
                    "{0}|{1}|{2}|{3}|{4}|{5}",
                    this.ID,
                    this.name,
                    this.address.Street,
                    this.address.City,
                    this.address.State,
                    this.address.Zip);
            }
        }

        /// <summary>
        /// Submits the data enclosed in the class to the SQL Server as either an Insert or an Update dependant on the presence of an ID in the base class.
        /// </summary>
        /// <returns>True if the operation completed successfully.</returns>
        public override bool CommitToDatabase() 
        {
            // Boolean for determining if the operation was successfull.
            bool success;

            // Group the data from the class into a single variable
            SQLColumn[] classData = new SQLColumn[] 
                                                    {
                                                     new SQLColumn("Name", this.name),
                                                     new SQLColumn("Address_ID", this.address.GetDatabaseID())
                                                    };

            if (this.ID == null)
            {
                // If ID is null, then this is a new entry to the database and we should use an insert statement.
                success = SQL.Query.Insert(
                                           "Company", 
                                           classData);
            }
            else
            {
                // If the ID is not null, then we are just updating the record which has the Company_ID we pulled to start with.
                success = SQL.Query.Update(
                                           "Company", 
                                           classData,
                                           string.Format("ID = {0}", this.ID));
            }

            // Return the value of the success of this operation, as well as the base operation of the same name. 
            // The base will also run an insert to update the database on any edits or updates it was alerted of.
            return success && base.CommitToDatabase();
        }

        /// <summary>
        /// Creates a Pop up dialog listing the changes made to the object so that the user can view the changes. Refer to the base object for details.
        /// </summary>
        /// <returns>True, if the user clicks on the "OK" button.</returns>
        public bool SaveConfirmation()
        {
            return base.SaveConfirmation(this.name);
        }

        /// <summary>
        /// Private method to fill the class with information.
        /// </summary>
        /// <param name="row">The data row to parse.</param>
        private void LoadFromDatabase(DataRow row)
        {
            try
            {
                // Test the ID because I don't know why it seems like a nice thing to do.
                int id;
                if (int.TryParse(row["ID"].ToString(), out id))
                {
                    this.ID = id;
                }

                Debug.Assert(this.ID != 0, "Company_ID cannot be 0 - this throws everything off");

                // Populate the remainder of the fields directly
                this.name = row["Name"].ToString();
                this.address.Street = row["Street"].ToString();
                this.address.City = row["City"].ToString();
                this.address.State = row["State"].ToString();
                this.address.Zip = row["Zip"].ToString();

                this.contact_list = new List<Contact>();
                ////foreach (DataRow con in SQL.Query.Select(string.Format(
                ////                                         "SELECT DISTINCT * FROM Contact " +
                ////                                         "JOIN Company_Contact_Relations ON Contact.Contact_ID = Company_Contact_Relations.Contact_ID " +
                ////                                         "WHERE Company_Contact_Relations.Company_ID = {0}",
                ////                                         this.ID)).Rows)
                ////{
                ////    this.contact_list.Add(new Contact(con));
                ////}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}