// <summary> Contains the class for interacting with the contact tables in the SQL server </summary>

namespace Database
{
    using System;
    using System.Data;
    
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
