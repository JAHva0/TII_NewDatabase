// <summary> Provides a class structure with which to parse, store, and update information from the database related to inspections.</summary>

namespace Database
{
    using System;
    using System.Data;

    /// <summary>
    /// Initializes a class object that can be used to load and/or modify inspection information from the database.
    /// </summary>
    public class Inspection : BaseObject
    {
        /// <summary> The elevator ID associated with this inspection. </summary>
        private int elevator_ID;

        /// <summary> The date this inspection occurred on. </summary>
        private DateTime date;

        /// <summary> The type of inspection performed. </summary>
        private string type;

        /// <summary> The status of the inspection. </summary>
        private string status;

        /// <summary> The inspector involved. </summary>
        private string inspector;

        /// <summary> The file path for the report associated with this inspection. </summary>
        private string report;

        /// <summary>
        /// Initializes a new instance of the <see cref="Inspection"/> class.
        /// </summary>
        public Inspection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Inspection"/> class.
        /// </summary>
        /// <param name="row"> The data row to load the inspection from.</param>
        public Inspection(DataRow row)
        {
            this.LoadFromDatabase(row);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Inspection"/> class.
        /// </summary>
        /// <param name="inspection_ID">The Inspection ID to load from.</param>
        public Inspection(int inspection_ID)
        {
            this.LoadFromDatabase(BaseObject.AffirmOneRow(SQL.Query.Select(string.Format("SELECT * FROM Inspection WHERE Inspection_ID = {0}", inspection_ID.ToString()))));
        }

        /// <summary> Gets or sets the Date for the inspection. </summary>
        /// <value> The date this inspection occurred on. </value>
        public DateTime Date
        {
            get
            {
                return this.date;
            }

            set
            {
                if (value != this.date)
                {
                    this.BaseObject_Edited(this, "Date", this.date.ToShortDateString(), value.ToShortDateString());
                    this.date = value;
                }
            }
        }

        /// <summary> Gets or sets the Type of inspection which was performed. </summary>
        /// <value> The type of inspection performed. </value>
        public string Type
        {
            get
            {
                return this.type;
            }

            set
            {
                if (value != this.type && value != string.Empty)
                {
                    this.BaseObject_Edited(this, "Type", this.type, value);
                    this.type = value;
                }
            }
        }

        /// <summary> Gets or sets the Status for the inspection. </summary>
        /// <value> The status of the inspection. </value>
        public string Status
        {
            get
            {
                return this.status;
            }

            set
            {
                if (value != this.status && value != string.Empty)
                {
                    this.BaseObject_Edited(this, "Status", this.status, value);
                    this.status = value;
                }
            }
        }

        /// <summary> Gets or sets the Inspector for the inspection. </summary>
        /// <value> The inspector who performed this inspection. </value>
        public string Inspector
        {
            get
            {
                return this.inspector;
            }

            set
            {
                if (value != this.inspector && value != string.Empty)
                {
                    this.BaseObject_Edited(this, "Inspector", this.inspector, value);
                    this.inspector = value;
                }
            }
        }

        /// <summary> Gets or sets the Report File Location for the inspection. </summary>
        /// <value> The report associated with this inspection. </value>
        public string ReportFile
        {
            get
            {
                return this.report;
            }

            set
            {
                if (value != this.report && value != string.Empty)
                {
                    this.BaseObject_Edited(this, "Report", this.report, value);
                    this.report = value;
                }
            }
        }

        /// <summary>
        /// Loads information from the database into the class structure.
        /// </summary>
        /// <param name="row"> The applicable row out of the inspection table. </param>
        private void LoadFromDatabase(DataRow row)
        {
            try
            {
                // Test the ID because it's a nice thing to do
                int id;
                if (int.TryParse(row["Inspection_ID"].ToString(), out id))
                {
                    this.ID = id;
                }

                // Test the Elevator ID as well
                if (int.TryParse(row["Elevator_ID"].ToString(), out id))
                {
                    this.elevator_ID = id;
                }

                // Make sure we have a parseable date
                DateTime d;
                if (DateTime.TryParse(row["Date"].ToString(), out d))
                {
                    this.date = d;
                }

                // We are assigning all of the strings directly, under the assumption that they were checked for compatibility prior to being entered into the database.
                this.type = row["Type"].ToString();
                this.status = row["Status"].ToString();
                this.inspector = row["Inspector"].ToString();
                this.report = row["Report"].ToString();
            }
            catch
            {
                // Implement exceptions as they arise.
                throw new Exception("Error Loading Inspection from Database");
            }
        }

        /// <summary>
        /// Submits the data enclosed in the class to the SQL Server as either an Insert or an Update dependant on the presence of an ID in the base class.
        /// </summary>
        /// <returns>True, if the operation completed sucessfully.</returns>
        public override bool CommitToDatabase()
        {
            // Boolean for determining if the operation was sucessful.
            bool success;

            // Group the data from the class together into a single variable.
            SQLColumn[] classData = new SQLColumn[] 
            {
                new SQLColumn("Elevator_ID", this.elevator_ID),
                new SQLColumn("Date", this.date),
                new SQLColumn("Type", this.type),
                new SQLColumn("Status", this.status),
                new SQLColumn("Inspector", this.inspector),
                new SQLColumn("Report", this.report)
            };

            if (this.ID == null)
            {
                success = SQL.Query.Insert("Inspection", classData);
            }
            else
            {
                success = SQL.Query.Update("Inspection", classData, string.Format("Inspection_ID = {0}", this.ID));
            }

            return success && base.CommitToDatabase();
        }
    }
}