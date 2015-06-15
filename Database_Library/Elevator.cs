// <summary> Provides a class structure with which to parse, store, and update information from the database related to the elevators.</summary>

namespace Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    
    /// <summary>
    /// Initializes a class object that can be used to load and/or modify elevator information from the database.
    /// </summary>
    public class Elevator : BaseObject
    {
        /// <summary> 
        /// The database assigned id associated with this elevator. 
        /// Considered exposing this value with a property, however elevators very rarely move from
        /// one building to another, so we're going with the assumption that once an elevator is installed,
        /// it's not going anywhere.
        /// </summary>
        private int building_id;

        /// <summary> The District or State assigned elevator number. </summary>
        private string number;

        /// <summary> The type of elevator. </summary>
        private Type type;

        /// <summary> A nickname for the elevator, if one would be helpful. </summary>
        private string nickname;

        /// <summary>
        /// Initializes a new instance of the <see cref="Elevator"/> class.
        /// </summary>
        public Elevator()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Elevator"/> class.
        /// </summary>
        /// <param name="row">A data row from the Elevator Table.</param>
        public Elevator(DataRow row)
        {
            this.LoadFromDatabase(row);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Elevator"/> class.
        /// </summary>
        /// <param name="elevator_ID">The Elevator ID to load.</param>
        public Elevator(int elevator_ID)
        {
            this.LoadFromDatabase(BaseObject.AffirmOneRow(SQL.Query.Select(string.Format(
                "SELECT Elevator.ID, Building_ID, Number, ElevatorTypes.Name AS Type, Nickname FROM Elevator " +
                "JOIN ElevatorTypes ON Elevator.Type_ID = ElevatorTypes.ID", 
                elevator_ID.ToString()))));
        }

        /// <summary>
        /// A list of months of the year. Prevents spelling errors and enforces consistency in the database.
        /// </summary>
        public enum Type
        {
            /// <summary>A Hydraulic Elevator.</summary>
            [Description("Hydraulic")]
            HYDRAULIC,

            /// <summary>A Traction Elevator.</summary>
            [Description("Traction")]
            TRACTION,

            /// <summary>An Escalator.</summary>
            [Description("Escalator")]
            ESCALATOR,

            /// <summary>A Wheelchair Lift.</summary>
            [Description("Wheelchair Lift")]
            WHEELCHAIR_LIFT,

            /// <summary>A Dumbwaiter.</summary>
            [Description("Dumbwaiter")]
            DUMBWAITER,

            /// <summary>An Inclined Lift.</summary>
            [Description("Inclined Lift")]
            INCLINED_LIFT,

            /// <summary>A LULA Lift.</summary>
            [Description("LULA Lift")]
            LULA_LIFT,

            /// <summary>A Handicapped Lift.</summary>
            [Description("Handicapped Lift")]
            HANDICAPPED_LIFT,

            /// <summary>A Vertical Lift.</summary>
            [Description("Vertical Lift")]
            VERTICAL_LIFT
        }

        /// <summary> Gets a string array containing all possible elevator types listed in the enumerator. </summary>
        /// <value> A string array containing all possible elevator types. </value>
        public static string[] Types
        {
            get
            {
                List<string> typeList = new List<string>();

                foreach (Type t in Enum.GetValues(typeof(Type)))
                {
                    typeList.Add(BaseObject.GetEnumDescription(t));
                }

                return typeList.ToArray();
            }
        }

        /// <summary> Gets or sets the Building_ID for this unit's building. </summary>
        /// <value> The Building_ID for this elevator's building. </value>
        public int OwnerID
        {
            get
            {
                return this.building_id;
            }

            set
            {
                if (value != 0 && value != this.building_id)
                {
                    this.BaseObject_Edited(this, "Building_ID", this.building_id, value);
                    this.building_id = value;
                }
            }
        }

        /// <summary> Gets or sets the Elevator Number for this unit. </summary>
        /// <value> The Elevator Number for this unit. </value>
        public string ElevatorNumber
        {
            get
            {
                return this.number;
            }

            set
            {
                if (value != string.Empty && value != this.number)
                {
                    this.BaseObject_Edited(this, "Number", this.number, value);
                    this.number = value;
                }
            }
        }

        /// <summary> Gets or sets the Elevator Type for this unit. </summary>
        /// <value> The Elevator Type for this unit. </value>
        public string ElevatorType
        {
            get
            {
                return BaseObject.GetEnumDescription(this.type);
            }

            set
            {
                if (value != string.Empty && value != BaseObject.GetEnumDescription(this.type))
                {
                    // Check to make sure the string we've recieved can be made into an enum
                    // (The method will throw an Argument Exception if it's not included in the list)
                    StringToTypeEnum(value);

                    this.BaseObject_Edited(this, "Type", BaseObject.GetEnumDescription(this.type), value);
                    this.type = StringToTypeEnum(value);
                }
            }
        }

        /// <summary> 
        /// Gets or sets the Nickname for this unit - for example, Elevator Number may be 71103078, but is referred to by the building as "Elevator 2".
        /// Mostly relevant in the district, where it appears that elevator number may change occasionally. 
        /// </summary>
        /// <value> The Nickname for this unit. </value>
        public string Nickname
        {
            get
            {
                return this.nickname;
            }

            set
            {
                if (value != string.Empty && value != this.nickname)
                {
                    this.BaseObject_Edited(this, "Nick", this.nickname, value);
                    this.nickname = value;
                }
            }
        }

        /// <summary> Gets a condensed string of all of the information contained within this class. </summary>
        /// <value> A string of values separated by the pipe character. </value>
        public string ToCondensedString
        {
            get
            {
                return string.Format(
                    "{0}|{1}|{2}|{3}|{4}",
                    this.ID,
                    this.building_id,
                    this.number,
                    this.ElevatorType,
                    this.nickname);
            }
        }

        /// <summary>
        /// Submits the data enclosed in the class to the SQL Server as either an Insert or an Update dependant on the presence of an ID in the base class.
        /// </summary>
        /// <returns>True, if the operation completed successfully.</returns>
        public override bool CommitToDatabase()
        {
            // Boolean for determining if the operation was successful
            bool success;
            
            // Group the data from the class together into a single variable.
            SQLColumn[] classData = new SQLColumn[]
            {
                new SQLColumn("Building_ID", this.building_id),
                new SQLColumn("Number", this.ElevatorNumber),
                new SQLColumn("Type_ID", string.Format("(SELECT ID FROM ElevatorTypes WHERE Name = '{0}')", BaseObject.GetEnumDescription(this.type))),
                new SQLColumn("Nickname", this.nickname)
            };

            if (this.ID == null)
            {
                success = SQL.Query.Insert("Elevator", classData);
            }
            else
            {
                success = SQL.Query.Update("Elevator", classData, string.Format("ID = {0}", this.ID));
            }
            
            return success && base.CommitToDatabase();
        }

        /// <summary>
        /// Convert the elevator type provided by the string to the corresponding enumerator.
        /// </summary>
        /// <param name="type">Elevator Type string.</param>
        /// <returns>An enumerator related to the type string.</returns>
        private static Type StringToTypeEnum(string type)
        {
            switch (type)
            {
                case "Hydraulic": return Type.HYDRAULIC;
                case "Traction": return Type.TRACTION;
                case "Escalator": return Type.ESCALATOR;
                case "Wheelchair Lift": return Type.WHEELCHAIR_LIFT;
                case "Dumbwaiter": return Type.DUMBWAITER;
                case "Inclined Lift": return Type.INCLINED_LIFT;
                case "LULA Lift": return Type.LULA_LIFT;
                case "Handicapped Lift": return Type.HANDICAPPED_LIFT;
                case "Vertical Lift": return Type.VERTICAL_LIFT;
                default: throw new ArgumentException("Invalid Elevator Type: " + type);
            }
        }

        /// <summary>
        /// Private method to fill the class with information.
        /// </summary>
        /// <param name="row">A Pre-filled DataRow from which to fill the class information.</param>
        private void LoadFromDatabase(DataRow row)
        {
            try
            {
                Debug.Assert(row != null, "The row that we pass in cannot be null");

                // Test the ID because it seems like a nice thing to do.
                int id;
                if (int.TryParse(row["ID"].ToString(), out id))
                {
                    this.ID = id;
                }

                Debug.Assert(this.ID != 0, "Elevator_ID cannot be 0 - this throws everything off");

                if (int.TryParse(row["Building_ID"].ToString(), out id))
                {
                    this.building_id = id;
                }

                this.number = row["Number"].ToString();
                this.type = StringToTypeEnum(row["Type"].ToString());
                this.nickname = row["Nickname"].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}