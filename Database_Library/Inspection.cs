// <summary> Provides a class structure with which to parse, store, and update information from the database related to inspections.</summary>

namespace Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;

    /// <summary>
    /// Initializes a class object that can be used to load and/or modify inspection information from the database.
    /// </summary>
    public class Inspection : BaseObject
    {
        /// <summary>
        /// Loaded the first time anything tries to access the inspection class. Is a copy of the InspectionTypes table.
        /// </summary>
        private static Dictionary<int, string[]> inspectionType;
        
        /// <summary> The elevator ID associated with this inspection. </summary>
        private int elevator_ID;

        /// <summary> The date this inspection occurred on. </summary>
        private DateTime date;

        /// <summary> The IType_ID (Inspection Type ID) of the inspection performed. </summary>
        private int itype_id;

        /// <summary> The status of the inspection. </summary>
        private Insp_Status status;

        /// <summary> The inspector involved. </summary>
        private string inspector;

        /// <summary> The file path for the report associated with this inspection. </summary>
        private string report;

        /// <summary>
        /// Initializes a new instance of the <see cref="Inspection"/> class.
        /// </summary>
        public Inspection()
        {
            if (inspectionType == null)
            {
                inspectionType = new Dictionary<int, string[]>();

                // The first entry will be for 0, {"No Inspection", "NULL"} so that new classes will assume this 
                inspectionType.Add(0, new string[] { "No Inspection", "NULL" });

                // If this is the first time the Inspection class has been accessed, load the inspection types dictionary. 
                foreach (DataRow row in SQL.Query.Select("SELECT * FROM InspectionTypes").Rows)
                {
                    inspectionType.Add(Convert.ToInt32(row["IType_ID"].ToString()), new string[] { row["Name"].ToString().Trim(), row["Abbv"].ToString().Trim() });
                }
            }
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
        /// A list of the various inspection statuses. Prevents spelling errors and enforces consistency in the database.
        /// </summary>
        public enum Insp_Status
        {
            /// <summary> Clean Inspection. </summary>
            [Description("Clean")]
            CLEAN,

            /// <summary> Inspection has items remaining. </summary>
            [Description("Outstanding Items")]
            OUTSANDING,

            /// <summary> Inspection has items, but they are paperwork only. </summary>
            [Description("Paperwork Only")]
            PAPERWORK,

            /// <summary> No Inspection was performed. </summary>
            [Description("No Inspection")]
            NO_INSPECT
        }

        /// <summary> Gets a string array containing all possible Inspection statuses listed in the enumerator. </summary>
        /// <value> A string array containing all possible Inspection statuses. </value>
        public static string[] Statuses
        {
            get
            {
                List<string> statusList = new List<string>();

                foreach (Insp_Status t in Enum.GetValues(typeof(Insp_Status)))
                {
                    statusList.Add(BaseObject.GetEnumDescription(t));
                }

                return statusList.ToArray();
            }
        }

        /// <summary> Gets a string array containing all possible Inspection types listed in the enumerator. </summary>
        /// <value> A string array containing all possible Inspection types. </value>
        public static string[] Types
        {
            get
            {
                List<string> typeList = new List<string>();

                foreach (string[] s in inspectionType.Values)
                {
                    typeList.Add(s[0]);
                }

                return typeList.ToArray();
            }
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
                return inspectionType[this.itype_id][0];
            }

            set
            {
                if (value != inspectionType[this.itype_id][0] && value != string.Empty)
                {
                    // Select the key associated with the value we passed in in store it to the itype_id integer
                    // If no such value exists in the dictionary, we've done something wrong and an exception will throw.
                    int id = inspectionType.Where(v => v.Value[0] == value).Single().Key;

                    this.BaseObject_Edited(this, "IType_ID", this.itype_id, id);
                    this.itype_id = id;
                }
            }
        }

        /// <summary> Gets or sets the Status for the inspection. </summary>
        /// <value> The status of the inspection. </value>
        public string Status
        {
            get
            {
                return BaseObject.GetEnumDescription(this.status);
            }

            set
            {
                if (value != BaseObject.GetEnumDescription(this.status) && value != string.Empty)
                {
                    this.BaseObject_Edited(this, "Status", BaseObject.GetEnumDescription(this.status), value);
                    this.status = Inspection.StringToStatusEnum(value);
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
        /// Checks to see if there is a report file, and if so, attempts to open it. 
        /// </summary>
        /// <param name="path"> The path to the desired report file. </param>
        public void OpenReportFile(string path)
        {
            if (this.report != string.Empty)
            {
                System.Diagnostics.Process.Start(path + this.report);
            }
        }

        public static string GetInspectionTypeAbbv(string value)
        {
            return inspectionType.Where(v => v.Value[0] == value).Single().Value[1];
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
                new SQLColumn("IType_ID", this.itype_id),
                new SQLColumn("Status", BaseObject.GetEnumDescription(this.status)),
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
        /// <param name="status">Inspection status string.</param>
        /// <returns>An Enumerator related to the status string.</returns>
        private static Insp_Status StringToStatusEnum(string status)
        {
            switch (status)
            {
                case "Clean": return Insp_Status.CLEAN;
                case "Outstanding Items": return Insp_Status.OUTSANDING;
                case "Paperwork Only": return Insp_Status.PAPERWORK;
                case "No Inspection": return Insp_Status.NO_INSPECT;
                default: throw new ArgumentException("Invalid Inspection Status: " + status);
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

                // And the IType_ID
                if (int.TryParse(row["IType_ID"].ToString(), out id))
                {
                    this.itype_id = id;
                }

                // Make sure we have a parseable date
                DateTime d;
                if (DateTime.TryParse(row["Date"].ToString(), out d))
                {
                    this.date = d;
                }

                // We are assigning all of the strings directly, under the assumption that they were checked for compatibility prior to being entered into the database.
                this.status = Inspection.StringToStatusEnum(row["Status"].ToString());
                this.inspector = row["Inspector"].ToString();
                this.report = row["Report"].ToString();
            }
            catch (Exception ex)
            {
                // Implement exceptions as they arise.
                throw new Exception("Error Loading Inspection from Database", ex);
            }
        }
    }
}