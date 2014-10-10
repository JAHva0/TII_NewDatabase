// <summary> Contains the class for interacting with the contact tables in the SQL server </summary>

namespace Database
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    
    /// <summary>
    /// Class that is used to hold, parse, update and insert data from the Contact table.
    /// </summary>
    public class Contact : BaseObject
    {
        /// <summary> The name of the contact. </summary>
        private string name;

        /// <summary> The office telephone number of the contact. </summary>
        private TelephoneNumber officephone;

        /// <summary> The cellphone number of the contact. </summary>
        private TelephoneNumber cellphone;

        /// <summary> The fax number of the contact. </summary>
        private TelephoneNumber fax;

        /// <summary> The e-mail address of the contact. </summary>
        private string email;

        /// <summary> A list of companies that this contact is related to. </summary>
        private Dictionary<int, string> companies = new Dictionary<int, string>();

        /// <summary> A list of buildings that this contact is related to. </summary>
        private Dictionary<int, string> building = new Dictionary<int, string>();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Contact"/> class. Class will be blank, and may be inserted into the database as a new entry.
        /// </summary>
        public Contact()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Contact"/> class.
        /// </summary>
        /// <param name="contact_info">Data Row containing information from the Contact table.</param>
        public Contact(DataRow contact_info)
        {
            // Test the ID because I don't know why it seems like a nice thing to do.
            int id;
            if (int.TryParse(contact_info["Contact_ID"].ToString(), out id))
            {
                this.ID = id;
            }

            this.name = contact_info["Name"].ToString();
            this.officephone = new TelephoneNumber(contact_info["OfficePhone"].ToString(), contact_info["OfficeExt"].ToString());
            this.cellphone = new TelephoneNumber(contact_info["CellPhone"].ToString());
            this.fax = new TelephoneNumber(contact_info["Fax"].ToString());
            this.email = contact_info["Email"].ToString();

            // The SQL query to get company relations for this contact.
            string query_format = "SELECT DISTINCT {0}.{0}_ID, {1} FROM {0} " +
                                  "JOIN {0}_Contact_Relations ON {0}_Contact_Relations.{0}_ID = {0}.{0}_ID " +
                                  "WHERE Contact_ID = " + this.ID.ToString();

            // Fill the company dictionary with a list of companies this contact has a relation with.
            foreach (DataRow c in SQL.Query.Select(string.Format(query_format, "Company", "Name")).Rows)
            {
                this.companies.Add(Convert.ToInt32(c["Company_ID"].ToString()), c["Name"].ToString());
            }

            // Fill the building dictionary with a list of buildings this contact has a relation with.
            foreach (DataRow b in SQL.Query.Select(string.Format(query_format, "Building", "Address")).Rows)
            {
                this.building.Add(Convert.ToInt32(b["Building_ID"].ToString()), b["Address"].ToString());
            }
        }

        /// <summary> Gets or sets the Name associated with this contact. </summary>
        /// <value> A string value of the contact's name.</value>
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (value != string.Empty && value != this.name)
                {
                    this.BaseObject_Edited(this, "Name", this.name, value);
                    this.name = value;
                }
            }
        }

        /// <summary> Gets or sets the Office Phone Number associated with this contact. </summary>
        /// <value> A Telephone Number struct containing the contact's office phone.</value>
        public TelephoneNumber OfficePhone
        {
            get
            {
                return this.officephone;
            }

            set
            {
                if (value != this.officephone && value.Number != string.Empty)
                {
                    // Office numbers are stored in two seperate columns in the database - "OfficePhone" and "OfficeExt".
                    // Since we've already decided that the new value is different from the old, check and see if one or both
                    // parts of the number have changed. No reason to make an extra database commit if only the extension changed.
                    if (value.Number != this.officephone.Number)
                    {
                        this.BaseObject_Edited(this, "OfficePhone", this.officephone.Number, value.Number);
                    }

                    if (value.Ext != this.officephone.Ext)
                    {
                        this.BaseObject_Edited(this, "OfficeExt", this.officephone.Ext, value.Ext);
                    }

                    this.officephone = value;
                }
            }
        }

        /// <summary> Gets or sets the Cell Phone Number associated with this contact. </summary>
        /// <value> A string value of the contact's cell phone.</value>
        public TelephoneNumber CellPhone
        {
            get
            {
                return this.cellphone;
            }

            set
            {
                if (value != this.cellphone && value.Number != string.Empty)
                {
                    this.BaseObject_Edited(this, "CellPhone", this.cellphone.Number.ToString(), value.Number.ToString());
                    this.cellphone = value;
                }
            }
        }

        /// <summary> Gets or sets the Fax Number associated with this contact. </summary>
        /// <value> A string value of the contact's fax number.</value>
        public TelephoneNumber Fax
        {
            get
            {
                return this.fax;
            }

            set
            {
                if (value != this.fax && value.Number != string.Empty)
                {
                    this.BaseObject_Edited(this, "Fax", this.fax.Number.ToString(), value.Number.ToString());
                    this.fax = value;
                }
            }        
        }

        /// <summary> Gets or sets the e-mail associated with this contact. </summary>
        /// <value> A string value of the contact's e-mail.</value>
        public string Email
        {
            get
            {
                return this.email;
            }

            set
            {         
                if (value != this.email && value != string.Empty)
                {
                    if (!value.Contains("@") || !value.Contains("."))
                    {
                        throw new ArgumentException("Invalid E-mail format");
                    }
                    
                    this.BaseObject_Edited(this, "Email", this.email, value);
                    this.email = value;
                }
            }
        }

        /// <summary> Gets a list of companies this contact is related to via the Company Contact Relations table. </summary>
        /// <value>A string array of Company names.</value>
        public string[] CompanyList
        {
            get
            {
                return this.companies.Values.ToArray();
            }
        }

        /// <summary> Gets a list of buildings this contact is related to via the Building Contact Relations table. </summary>
        /// <value>A string array of Building addresses.</value>
        public string[] BuildingList
        {
            get
            {
                return this.building.Values.ToArray();
            }
        }

        /// <summary>
        /// Submits the data enclosed in the class to the SQL server as either an Insert or and Update statement dependant on the presence of an ID in the base Class.
        /// </summary>
        /// <returns>True, if all operations completed successfully.</returns>
        public override bool CommitToDatabase()
        {
            bool success = false;

            SQLColumn[] classData = new SQLColumn[]
                                                    {
                                                        new SQLColumn("Name", this.name),
                                                        new SQLColumn("OfficePhone", this.officephone.Number),
                                                        new SQLColumn("OfficeExt", this.officephone.Ext),
                                                        new SQLColumn("CellPhone", this.cellphone.Number),
                                                        new SQLColumn("Fax", this.fax.Number),
                                                        new SQLColumn("Email", this.email)
                                                    };

            if (this.ID == null)
            {
                // If ID is null, then this is a new entry to the database and we should insert it.
                success = SQL.Query.Insert("Contact", classData);
            }
            else
            {
                // If ID is not null, then we are updating an existing record which has this ID number
                success = SQL.Query.Update("Contact", classData, string.Format("Contact_ID = {0}", this.ID));
            }

            // Return the value of the success of this operation, as well as the base operation of the same name.
            // The base operation will run an Insert Query on any database edits it was alerted of.
            return success && base.CommitToDatabase();
        }

        /// <summary>
        /// Method to add a company with which to associate this contact.
        /// </summary>
        /// <param name="company_name">The exact company name to add.</param>
        public void AddCompany(string company_name)
        {
            DataRow row = BaseObject.AffirmOneRow(SQL.Query.Select("Company_ID", "Company", string.Format("Name = '{0}'", company_name)));
            this.companies.Add(Convert.ToInt32(row["Company_ID"].ToString()), company_name);
        }

        /// <summary>
        /// Method to add a building with which to associate this contact.
        /// </summary>
        /// <param name="building_address">The exact building address to add.</param>
        public void AddBuilding(string building_address)
        {
            DataRow row = BaseObject.AffirmOneRow(SQL.Query.Select("Building_ID", "Building", string.Format("Address = '{0}'", building_address)));
            this.building.Add(Convert.ToInt32(row["Building_ID"].ToString()), building_address);
        }

        /// <summary>
        /// Removes the Database relation between the current contact and a company.
        /// </summary>
        /// <param name="company_id">The database assigned company ID.</param>
        public void RemoveFromCompany(int company_id)
        {
            SQL.Query.Delete("Company_Contact_Relations", string.Format("Contact_ID = {0} AND Company_ID = {1}", this.ID, company_id));
        }

        /// <summary>
        /// Removes the Database relation between the current contact and a company.
        /// </summary>
        /// <param name="company_name">The exact name of a company within the database.</param>
        public void RemoveFromCompany(string company_name)
        {
            int company_id;
            if (int.TryParse(SQL.Query.Select(string.Format("SELECT Company_ID FROM Company WHERE Name = '{0}'", company_name)).Rows[0]["Company_ID"].ToString(), out company_id))
            {
                this.RemoveFromCompany(company_id);
            }
        }

        /// <summary>
        /// Removes the Database relation between the current contact and a building.
        /// </summary>
        /// <param name="building_id">The database assigned building ID.</param>
        public void RemoveFromBuilding(int building_id)
        {
            SQL.Query.Delete("Building_Contact_Relations", string.Format("Contact_ID = {0} AND Building_ID = {1}", this.ID, building_id));
        }

        /// <summary>
        /// Removes the Database relation between the current contact and a building.
        /// </summary>
        /// <param name="building_address">The exact address of a building.</param>
        public void RemoveFromBuilding(string building_address)
        {
            int building_id;
            if (int.TryParse(SQL.Query.Select(string.Format("SELECT Building_ID FROM Building WHERE Address = '{0}'", building_address)).Rows[0]["Building_ID"].ToString(), out building_id))
            {
                this.RemoveFromBuilding(building_id);
            }
        }
    }
}
