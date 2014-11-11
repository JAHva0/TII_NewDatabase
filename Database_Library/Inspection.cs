// <summary> Provides a class structure with which to parse, store, and update information from the database related to inspections.</summary>

namespace Database
{
    using System;
    using System.ComponentModel;
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
        private Type type;

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

        /// <summary>
        /// A list of the various types of inspections. Prevents spelling errors and enforces consistency in the database.
        /// </summary>
        public enum Type
        {
            /// <summary> Periodic Inspection. </summary>
            [Description("Periodic")]
            PER,

            /// <summary> Periodic Re-inspection. </summary>
            [Description("Periodic Reinspection")]
            PER_RE,

            /// <summary> Category 1 and Periodic Inspection. </summary>
            [Description("Category 1 & Periodic")]
            CAT1_PER,

            /// <summary> Category 1 and Periodic Re-inspection. </summary>
            [Description("Category 1 & Periodic Reinspection")]
            CAT1_PER_RE,

            /// <summary> Category 5 and Periodic Inspection. </summary>
            [Description("Category 5 & Periodic")]
            CAT5_PER,

            /// <summary> Category 5 and Periodic Re-inspection. </summary>
            [Description("Category 5 & Periodic Reinspection")]
            CAT5_PER_RE,

            /// <summary> Annual Inspection. </summary>
            [Description("Annual")]
            ANNUAL,

            /// <summary> A Re-inspection. </summary>
            [Description("Reinspection")]
            REINSPECTION,

            /// <summary> Five Year Test and Inspection. </summary>
            [Description("Five Year Test")]
            FIVE_YEAR
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
                    this.BaseObject_Edited(this, "Date", this.date, value);
                    this.date = value;
                }
            }
        }

        /// <summary> Gets or sets the Elevator_ID associated with this inspection. </summary>
        /// <value> The database assigned Elevator ID of the unit that was inspected. </value>
        public int ElevatorID
        {
            get
            {
                return this.elevator_ID;
            }

            set
            {
                if (value != this.elevator_ID && value != 0)
                {
                    this.BaseObject_Edited(this, "Elevator_ID", this.elevator_ID, value);
                    this.elevator_ID = value;
                }
            }
        }

        /// <summary> Gets or sets the Type of inspection which was performed. </summary>
        /// <value> The type of inspection performed. </value>
        public string InspectionType
        {
            get
            {
                return BaseObject.GetEnumDescription(this.type);
            }

            set
            {
                if (value != BaseObject.GetEnumDescription(this.type) && value != string.Empty)
                {
                    // Check to make sure that the string we've recieved can be made into an enum
                    // The method will throw an exception if it is not included in the list, which is
                    // important to know before we try to either add the Edited event or assign to the value.
                    Inspection.StringToTypeEnum(value);

                    this.BaseObject_Edited(this, "Type", BaseObject.GetEnumDescription(this.type), value);
                    this.type = Inspection.StringToTypeEnum(value);
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
        /// Submits the data enclosed in the class to the SQL Server as either an Insert or an Update dependant on the presence of an ID in the base class.
        /// </summary>
        /// <returns>True, if the operation completed successfully.</returns>
        public override bool CommitToDatabase()
        {
            // Boolean for determining if the operation was sucessful.
            bool success;

            // Group the data from the class together into a single variable.
            SQLColumn[] classData = new SQLColumn[] 
            {
                new SQLColumn("Elevator_ID", this.elevator_ID),
                new SQLColumn("Date", this.date),
                new SQLColumn("Type", BaseObject.GetEnumDescription(this.type)),
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

        /// <summary>
        /// Converts the elevator type provided by the string to the corresponding enumerator.
        /// </summary>
        /// <param name="type">Inspection type string.</param>
        /// <returns>An Enumerator related tot he type string.</returns>
        private static Type StringToTypeEnum(string type)
        {
            switch (type)
            {
                case "Periodic": return Type.PER;
                case "Periodic Reinspection": return Type.PER_RE;
                case "Category 1 / Periodic": return Type.CAT1_PER;
                case "Category 1 / Periodic Reinspection": return Type.CAT1_PER_RE;
                case "Category 5 / Periodic": return Type.CAT5_PER;
                case "Category 5 / Periodic Reinspection": return Type.CAT5_PER_RE;
                case "Annual": return Type.ANNUAL;
                case "Reinspection": return Type.REINSPECTION;
                case "Category 5": return Type.FIVE_YEAR;
                default: throw new ArgumentException("Invalid Inspection Type: " + type);
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
                this.type = Inspection.StringToTypeEnum(row["Type"].ToString());
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
    }
}