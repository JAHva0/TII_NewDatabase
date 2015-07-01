// <summary> Loads an item from the database's contact log table </summary>

namespace Database
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    /// <summary>
    /// Class for loading contact log entries from the server.
    /// </summary>
    public class ContactLog : BaseObject
    {
        /// <summary> The Database assigned ID for the contact. </summary>
        private int contact_id;

        /// <summary> The name of the contact. </summary>
        private string contact;

        /// <summary> The Database assigned ID for the company. </summary>
        private int company_id;

        /// <summary> The name of the company. </summary>
        private string company;

        /// <summary> The Database assigned ID for the building. </summary>
        private int building_id;

        /// <summary> The address of the building. </summary>
        private string building;

        /// <summary> The date and time of this entry. </summary>
        private DateTime date;

        /// <summary> The body test of this entry. </summary>
        private string notes;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactLog"/> class.
        /// </summary>
        /// <param name="row">A properly formatted row from the database. </param>
        private ContactLog(DataRow row)
        {
            try
            {
                int id;
                if (int.TryParse(row["ID"].ToString(), out id))
                {
                    this.ID = id;
                }

                // Get the contact info
                int.TryParse(row["Contact_ID"].ToString(), out this.contact_id);
                this.contact = row["Contact"].ToString();

                // Get the company info
                int.TryParse(row["Company_ID"].ToString(), out this.contact_id);
                this.company = row["Company"].ToString();

                // Get the Building info
                int.TryParse(row["Building_ID"].ToString(), out this.contact_id);
                this.building = row["Building"].ToString();

                // Get the note information
                DateTime.TryParse(row["Date"].ToString(), out this.date);
                this.notes = row["Notes"].ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary> Gets the name of the contact associated with this entry. </summary>
        /// <value> A string of the Contact Name. </value>
        public string Contact
        {
            get
            {
                return this.contact;
            }
        }

        /// <summary> Gets the date of the addition of this entry. </summary>
        /// <value> A <see cref="DateTime"/> value for this entry. </value>
        public DateTime Date
        {
            get
            {
                return this.date;
            }
        }

        /// <summary> Gets the body text for this entry. </summary>
        /// <value> A string containing the body text. </value>
        public string Notes
        {
            get
            {
                return this.notes;
            }
        }

        /// <summary>
        /// Retrieves a list of contact entries associated with this company. 
        /// </summary>
        /// <param name="company"> A company class to query. </param>
        /// <returns> An array of <see cref="ContactLog"/> entries loaded from the Server. </returns>
        public static ContactLog[] GetCompanyContactLog(Company company)
        {
            List<ContactLog> logs = new List<ContactLog>();

            string query = string.Format(
                "SELECT " +
                "ID, " +
                "Contact_ID, " +
                "(SELECT Name FROM Contact WHERE ID = Contact_ID) as Contact, " +
                "Company_ID, " +
                "(SELECT Name FROM Company WHERE ID = Company_ID) as Company, " +
                "Building_ID, " +
                "(SELECT(SELECT Street FROM Address WHERE ID = Address_ID) as Street FROM Building WHERE ID = Building_ID) as Building, " +
                "Date, " +
                "Notes " +
                "FROM ContactLog " +
                "WHERE Company_ID = {0} " +
                "ORDER BY DATE Desc",
                company.ID);

            foreach (DataRow row in SQL.Query.Select(query).Rows)
            {
                logs.Add(new ContactLog(row));
            }

            return logs.ToArray();
        }

        /// <summary>
        /// Retrieves a list of contact entries associated with this building. 
        /// </summary>
        /// <param name="building"> A company class to query. </param>
        /// <returns> An array of <see cref="ContactLog"/> entries loaded from the Server. </returns>
        public static ContactLog[] GetBuildingContactLog(Building building)
        {
            List<ContactLog> logs = new List<ContactLog>();

            string query = string.Format(
                "SELECT " +
                "ID, " +
                "Contact_ID, " +
                "(SELECT Name FROM Contact WHERE ID = Contact_ID) as Contact, " +
                "Company_ID, " +
                "(SELECT Name FROM Company WHERE ID = Company_ID) as Company, " +
                "Building_ID, " +
                "(SELECT(SELECT Street FROM Address WHERE ID = Address_ID) as Street FROM Building WHERE ID = Building_ID) as Building, " +
                "Date, " +
                "Notes " +
                "FROM ContactLog " +
                "WHERE Building_ID = {0} " +
                "ORDER BY DATE Desc",
                building.ID);

            foreach (DataRow row in SQL.Query.Select(query).Rows)
            {
                logs.Add(new ContactLog(row));
            }

            return logs.ToArray();
        }
    }
}
