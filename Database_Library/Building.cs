// <summary> Provides a class structure with which to load items out of the building table. </summary>
namespace Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using Database;

    /// <summary>
    /// Initializes a class object that can be used to safely load and/or modify building information in the database.
    /// </summary>
    public class Building : BaseObject
    {
        /// <summary> The company id associated with this building. Can be used to pull the company class when requested. </summary>
        private int company_id;

        /// <summary> The proposal number associated with this building.</summary>
        private string proposal_number;

        /// <summary> The filename for the proposal associated with this building.</summary>
        private string proposal_file;

        /// <summary> The name associated with this particular building.</summary>
        private string name;

        /// <summary> Struct that holds all of the address information for this building. </summary>
        private Address address;

        /// <summary> The county associated with this building.</summary>
        private CountyName county;

        /// <summary> The Firm fee charged for this building.</summary>
        private Money firm_fee;

        /// <summary> The Firm fee charged for this building.</summary>
        private Money hourly_fee;

        /// <summary> The Anniversary Month for this unit.</summary>
        private Month anniversary;

        /// <summary> The name of the elevator contractor who is employed by this building.</summary>
        private string contractor;

        /// <summary> Boolean that determines if the building is active or not.</summary>
        private bool active;

        /// <summary> Geographic Coordinates of the building.</summary>
        private GeographicCoordinates coordinates;

        /// <summary> A collection of contacts associated with this building. </summary>
        private List<Contact> contact_list = new List<Contact>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Building"/> class for use in adding new entries to the database.
        /// </summary>
        public Building()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Building"/> class. Loads information related to the provided Building ID and
        /// stores them so that they may be quickly selected/edited.
        /// </summary>
        /// <param name="building_ID">The Database assigned Building_ID.</param>
        public Building(int building_ID)
        {
            DataTable tbl = SQL.Query.Select("*", "Building", string.Format("Building_ID = '{0}'", building_ID));
            Debug.Assert(tbl.Rows.Count == 1, string.Format("Building Query for Building_ID {0} has returned {0} rows", building_ID, tbl.Rows.Count));
            this.LoadFromDatabase(tbl.Rows[0]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Building"/> class. Loads information related to the provided Building Address and
        /// stores them so that they may be quickly selected/edited.
        /// </summary>
        /// <param name="building_address">The Address of the building to Query.</param>
        public Building(string building_address)
        {
            DataTable tbl = SQL.Query.Select("*", "Building", string.Format("Address = '{0}'", building_address));
            Debug.Assert(tbl.Rows.Count == 1, string.Format("Building Query for Address {0} has returned {0} rows", building_address, tbl.Rows.Count));
            this.LoadFromDatabase(tbl.Rows[0]);
        }

        /// <summary>
        /// A list of every county covered by the database. Prevents spelling errors and enforces consistency in the database.
        /// </summary>
        public enum CountyName
        {
            /// <summary> Default enumerator value. </summary>
            [Description("")]
            NONE,
            
            /// <summary>Allegany County.</summary>
            [Description("Allegany")] 
            ALLEGANY, 

            /// <summary>Anne Arundel County.</summary>
            [Description("Anne Arundel")]
            ANNE_ARUNDEL,

            /// <summary>Baltimore County.</summary>
            [Description("Baltimore")]
            BALTIMORE,

            /// <summary>Baltimore City.</summary>
            [Description("Baltimore City")]
            BALTIMORE_CITY,

            /// <summary>Calvert County.</summary>
            [Description("Calvert")]
            CALVERT,

            /// <summary>Caroline County.</summary>
            [Description("Caroline")]
            CAROLINE,

            /// <summary>Carroll County.</summary>
            [Description("Carroll")]
            CARROLL,

            /// <summary>Cecil County.</summary>
            [Description("Cecil")]
            CECIL,

            /// <summary>Charles County.</summary>
            [Description("Charles")]
            CHARLES,

            /// <summary>Dorchester County.</summary>
            [Description("Dorchester")]
            DORCHESTER,

            /// <summary>Frederick County.</summary>
            [Description("Frederick")]
            FREDERICK,

            /// <summary>Garrett County.</summary>
            [Description("Garrett")]
            GARRETT,

            /// <summary>Harford County.</summary>
            [Description("Harford")]
            HARFORD,

            /// <summary>Howard County.</summary>
            [Description("Howard")]
            HOWARD,

            /// <summary>Kent County.</summary>
            [Description("Kent")]
            KENT,

            /// <summary>Montgomery County.</summary>
            [Description("Montgomery")]
            MONTGOMERY,

            /// <summary>Prince George's County.</summary>
            [Description("Prince George's")]
            PRINCE_GEORGES,

            /// <summary>Queen Anne's County.</summary>
            [Description("Queen Anne's")]
            QUEEN_ANNES,

            /// <summary>Saint Mary's County.</summary>
            [Description("Saint Mary's")]
            SAINT_MARYS,

            /// <summary>Somerset County.</summary>
            [Description("Somerset")]
            SOMERSET,

            /// <summary>Talbot County.</summary>
            [Description("Talbot")]
            TALBOT,

            /// <summary>Washington County.</summary>
            [Description("Washington")]
            WASHINGTON,

            /// <summary>Wicomico County.</summary>
            [Description("Wicomico")]
            WICOMICO,

            /// <summary>Worcester County.</summary>
            [Description("Worcester")]
            WORCESTER,

            /// <summary>Washington D.C.</summary>
            [Description("Washington D.C.")]
            WASHINGTONDC
        }

        /// <summary>
        /// A list of months of the year. Prevents spelling errors and enforces consistency in the database.
        /// </summary>
        public enum Month
        {
            /// <summary> Default enumerator value. </summary>
            [Description("")]
            NONE,
            
            /// <summary>Month of January.</summary>
            [Description("January")]
            JAN,

            /// <summary>Month of February.</summary>
            [Description("February")]
            FEB,

            /// <summary>Month of March.</summary>
            [Description("March")]
            MAR,

            /// <summary>Month of April.</summary>
            [Description("April")]
            APR,

            /// <summary>Month of May.</summary>
            [Description("May")]
            MAY,

            /// <summary>Month of June.</summary>
            [Description("June")]
            JUN,

            /// <summary>Month of July.</summary>
            [Description("July")]
            JUL,

            /// <summary>Month of August.</summary>
            [Description("August")]
            AUG,

            /// <summary>Month of September.</summary>
            [Description("September")]
            SEP,

            /// <summary>Month of October.</summary>
            [Description("October")]
            OCT,

            /// <summary>Month of November.</summary>
            [Description("November")]
            NOV,

            /// <summary>Month of December.</summary>
            [Description("December")]
            DEC,
        }

        /// <summary>
        /// Gets or sets the company associated with this building. Sets the Company_ID integer, or gets a company class based off of the stored Company_ID.
        /// </summary>
        /// <value>A Company class using the stored company_id.</value>
        public Company Owner
        {
            get
            {
                if (this.company_id == 0)
                {
                    return new Company();
                }
                else
                {
                    return new Company(this.company_id);
                }
            }

            set
            {
                if (this.company_id != value.ID.Value)
                {
                    this.BaseObject_Edited(this, "Company_ID", this.company_id.ToString(), value.ID.ToString());
                    this.company_id = value.ID.Value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the proposal number and triggers an edited event if valid.
        /// </summary>
        /// <value>The Proposal number associated with this building.</value>
        public string ProposalNumber
        {
            get
            {
                return this.proposal_number;
            }

            set
            {
                // Check to make sure it's not either blank or the same as the current proposal number.
                if (value != string.Empty && value != this.proposal_number)
                {
                    this.BaseObject_Edited(this, "ProposalNumber", this.proposal_number, value);
                    this.proposal_number = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the proposal filename and triggers an edited event if valid.
        /// </summary>
        /// <value>The Proposal file associated with this building.</value>
        public string ProposalFile
        {
            get
            {
                return this.proposal_file;
            }

            set
            {
                // Check to make sure it's not either blank or the same as the current proposal file.
                if (value != string.Empty && value != this.proposal_file)
                {
                    this.BaseObject_Edited(this, "ProposalFile", this.proposal_file, value);
                    this.proposal_file = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the name associated with this building. If no name is provided, it will return the street address instead.
        /// </summary>
        /// <value> The Name associated with this building.</value>
        public string Name
        {
            get
            {
                // If there is a name given, provide that. Otherwise, we're just going to assume the building is known by it's street address.
                if (this.name != string.Empty)
                {
                    return this.name;
                }
                else
                {
                    return this.address.Street;
                }
            }

            set
            {
                // Check to make sure it's not either blank or the same as the current name.
                if (value != string.Empty && value != this.name)
                {
                    this.BaseObject_Edited(this, "Name", this.name, value);
                    this.name = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the address associated with this building.
        /// </summary>
        /// <value> The address associated with this building.</value>
        public string Street
        {
            get
            {
                return this.address.Street;
            }

            set
            {
                // Check to make sure it's not either blank or the same as the current address.
                if (value != string.Empty && value != this.address.Street)
                {
                    this.BaseObject_Edited(this, "Address", this.address.Street, value);
                    this.address.Street = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the city associated with this building.
        /// </summary>
        /// <value> The city associated with this building.</value>
        public string City
        {
            get
            {
                return this.address.City;
            }

            set
            {
                // Check to make sure it's not either blank or the same as the current city.
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

        /// <summary> Gets or sets the county name.</summary>
        /// <value>The county associated with this building.</value>
        public string County
        {
            get
            {
                return BaseObject.GetEnumDescription(this.county);
            }

            set
            {
                if (BaseObject.GetEnumDescription(this.county) != value)
                {
                    this.BaseObject_Edited(this, "County", BaseObject.GetEnumDescription(this.county), value);
                    this.county = StringToCountyEnum(value);
                }
            }
        }

        /// <summary>Gets or sets the firm fee.</summary>
        /// <value>The firm fee in <see cref="Money"/> format.</value>
        public Money FirmFee
        {
            get
            {
                return this.firm_fee;
            }

            set
            {
                // As long as the value has changed and isn't blank, change it.
                if (this.firm_fee.Value != value.Value && value.Value != (decimal)0.0)
                {
                    this.BaseObject_Edited(this, "FirmFee", this.firm_fee.Value.ToString(), value.Value.ToString());
                    this.firm_fee = value;
                }
            }
        }

        /// <summary>Gets or sets the hourly fee.</summary>
        /// <value>The hourly fee in <see cref="Money"/> format.</value>
        public Money HourlyFee
        {
            get
            {
                return this.hourly_fee;
            }

            set
            {
                // As long as the value has changed and isn't blank, change it.
                if (this.hourly_fee.Value != value.Value && value.Value != (decimal)0.0)
                {
                    this.BaseObject_Edited(this, "HourlyFee", this.hourly_fee.Value.ToString(), value.Value.ToString());
                    this.hourly_fee = value;
                }
            }
        }

        /// <summary> Gets or sets the Anniversary for this building.</summary>
        /// <value>The anniversary month associated with this building.</value>
        public string Anniversary
        {
            get
            {
                return BaseObject.GetEnumDescription(this.anniversary);
            }

            set
            {
                // As long as it's not what we already have, change it.
                if (value != BaseObject.GetEnumDescription(this.anniversary))
                {
                    this.BaseObject_Edited(this, "Anniversary", BaseObject.GetEnumDescription(this.anniversary), value);
                    this.anniversary = StringtoMonthEnum(value);
                }
            }
        }

        /// <summary>Gets or sets the contractor name associated with this building.</summary>
        /// <value>The name of the contractor for this building.</value>
        public string Contractor
        {
            get
            {
                return this.contractor;
            }

            set
            {
                // As long as it is not empty, and it is different from what we already have, change it.
                if (value != string.Empty && value != this.contractor)
                {
                    this.BaseObject_Edited(this, "Contractor", this.contractor, value);
                    this.contractor = value;
                }
            }
        }

        /// <summary> Gets or sets a value indicating whether or not the building is listed as active.</summary>
        /// <value>The active status for this building.</value>
        public bool Active
        {
            get
            {
                return this.active;
            }

            set
            {
                // As long as it is different from what we already have, change it.
                if (value != this.active)
                {
                    this.BaseObject_Edited(this, "Active", this.active.ToString(), value.ToString());
                    this.active = value;
                }
            }
        }

        /// <summary> Gets or sets the Geographic Coordinates of a building. </summary>
        /// <value> The Geographic Coordinates of a building.</value>
        public GeographicCoordinates Coordinates
        {
            get
            {
                return this.coordinates;
            }

            set
            {
                // As long as the coordinates are not empty, or equal to the ones which are already listed, change them.
                if (value != null && value != this.coordinates)
                {
                    // Check each coordinate value to see if we actually need to update it
                    if (value.Latitude != this.coordinates.Latitude)
                    {
                        this.BaseObject_Edited(this, "Latitude", this.coordinates.Latitude.ToString(), value.Latitude.ToString());
                    }

                    if (value.Longitude != this.coordinates.Longitude)
                    {
                        this.BaseObject_Edited(this, "Longitude", this.coordinates.Longitude.ToString(), value.Longitude.ToString());
                    }

                    this.coordinates = value;
                }
            }
        }

        /// <summary> Gets a list of the Elevator Numbers associated with this unit. </summary>
        /// <value> A collection of strings as pulled from the database. Can be empty.</value>
        public List<Elevator> ElevatorList
        {
            get
            {
                // If there is no ID present, then we can't possible have elevators
                if (this.ID == null)
                {
                    return new List<Elevator>();
                }
                
                List<Elevator> elevators = new List<Elevator>();
                foreach (DataRow row in SQL.Query.Select("*", "Elevator", string.Format("Building_ID = {0}", this.ID)).Rows)
                {
                    elevators.Add(new Elevator(row));
                }

                return elevators;
            }
        }

        /// <summary> Gets a list of Inspections that are associated with this building. </summary>
        /// <value> A collection of InspectionHistory Structs as pulled from the database. Can be Empty.</value>
        public List<InspectionHistory> Inspection_History
        {
            get
            {
                // If there is no ID present, then we can't possible have an inspection history.
                if (this.ID == null)
                {
                    return new List<InspectionHistory>();
                }
                
                List<InspectionHistory> history = new List<InspectionHistory>();
                foreach (DataRow row in SQL.Query.Select(string.Format(
                                                                       "SELECT DISTINCT Date, InspectionType, Status, Inspector, Report " +
                                                                       "FROM Inspection WHERE Elevator_ID IN " +
                                                                       "(" +
                                                                           "SELECT Elevator_ID " +
                                                                           "FROM Elevator " +
                                                                           "WHERE Building_ID = {0}" +
                                                                       ")" +
                                                                       "ORDER BY Date Desc", 
                                                                       this.ID)).Rows)
                {
                    history.Add(new InspectionHistory(row));
                }

                return history;
            }
        }

        /// <summary> Gets a copy of the list of contacts associated with this building. </summary>
        /// <value> A List of contacts associated with this building.</value>
        public List<Contact> ContactList
        {
            get
            {
                return this.contact_list;
            }
        }

        /// <summary>
        /// Method to Insert or update the information in the class into the database.
        /// </summary>
        /// <returns>True if all operations were performed successfully.</returns>
        public override bool CommitToDatabase()
        {
            // Boolean for determining if the operation was successful.
            bool success; 

            // Group the data from the class into a single variable
            SQLColumn[] classData = new SQLColumn[]
                                                    {
                                                        new SQLColumn("Company_ID", this.company_id),
                                                        new SQLColumn("ProposalNumber", this.proposal_number),
                                                        new SQLColumn("ProposalFile", this.proposal_file),
                                                        new SQLColumn("Name", this.name),
                                                        new SQLColumn("Address", this.address.Street),
                                                        new SQLColumn("City", this.address.City),
                                                        new SQLColumn("State", this.address.State),
                                                        new SQLColumn("Zip", this.address.Zip),
                                                        new SQLColumn("County", BaseObject.GetEnumDescription(this.county)),
                                                        new SQLColumn("FirmFee", this.firm_fee.Value),
                                                        new SQLColumn("HourlyFee", this.hourly_fee.Value),
                                                        new SQLColumn("Anniversary", BaseObject.GetEnumDescription(this.anniversary)),
                                                        new SQLColumn("Contractor", this.contractor),
                                                        new SQLColumn("Active", this.active),
                                                        new SQLColumn("Latitude", this.coordinates.Latitude),
                                                        new SQLColumn("Longitude", this.coordinates.Longitude)
                                                    };

            if (this.ID == null)
            {
                // If ID is null, then this is a new entry to the database and we should use an insert statement.
                success = SQL.Query.Insert("Building", classData);
            }
            else
            {
                // If the ID is not null, then we are just updating the record which has the Building_ID we pulled to start with.
                success = SQL.Query.Update("Building", classData, string.Format("Building_ID = {0}", this.ID));
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
            return base.SaveConfirmation(this.address.Street);
        }

        /// <summary>
        /// Convert the county name provided to the corresponding enumerator.
        /// </summary>
        /// <param name="c">County name.</param>
        /// <returns>The appropriate enumerator for the provided string.</returns>
        private static CountyName StringToCountyEnum(string c)
        {
            switch (c)
            {
                case "Allegany": return CountyName.ALLEGANY;
                case "Anne Arundel": return CountyName.ANNE_ARUNDEL;
                case "Baltimore": return CountyName.BALTIMORE;
                case "Baltimore City": return CountyName.BALTIMORE_CITY;
                case "Calvert": return CountyName.CALVERT;
                case "Caroline": return CountyName.CAROLINE;
                case "Carroll": return CountyName.CARROLL;
                case "Cecil": return CountyName.CECIL;
                case "Charles": return CountyName.CHARLES;
                case "Dorchester": return CountyName.DORCHESTER;
                case "Frederick": return CountyName.FREDERICK;
                case "Garrett": return CountyName.GARRETT;
                case "Harford": return CountyName.HARFORD;
                case "Howard": return CountyName.HOWARD;
                case "Kent": return CountyName.KENT;
                case "Montgomery": return CountyName.MONTGOMERY;
                case "Prince George's": return CountyName.PRINCE_GEORGES;
                case "Queen Anne's": return CountyName.QUEEN_ANNES;
                case "Saint Mary's": return CountyName.SAINT_MARYS;
                case "Somerset": return CountyName.SOMERSET;
                case "Talbot": return CountyName.TALBOT;
                case "Washington": return CountyName.WASHINGTON;
                case "Washington D.C.": return CountyName.WASHINGTONDC;
                case "Wicomico": return CountyName.WICOMICO;
                case "Worcester": return CountyName.WORCESTER;
                default: throw new ArgumentException("Invalid County Name: " + c);
            }
        }

        /// <summary>
        /// Convert the month name to the corresponding enumerator.
        /// </summary>
        /// <param name="m">Month name.</param>
        /// <returns>An enumerator corresponding to the month name.</returns>
        private static Month StringtoMonthEnum(string m)
        {
            switch (m)
            {
                case "January": return Month.JAN;
                case "February": return Month.FEB;
                case "March": return Month.MAR;
                case "April": return Month.APR;
                case "May": return Month.MAY;
                case "June": return Month.JUN;
                case "July": return Month.JUL;
                case "August": return Month.AUG;
                case "September": return Month.SEP;
                case "October": return Month.OCT;
                case "November": return Month.NOV;
                case "December": return Month.DEC;
                case "": return Month.NONE;
                default: throw new ArgumentException("Invalid Month Selection");
            }
        }

        /// <summary>
        /// Private method to fill the class with information.
        /// </summary>
        /// <param name="row">A Pre-filled DataRow from which to fill the class information.</param>
        private void LoadFromDatabase(DataRow row)
        {
            Debug.Assert(row != null, "Row cannot be null");
            
            // Test the ID because it seems like a nice thing to do.
            int id;
            if (int.TryParse(row["Building_ID"].ToString(), out id))
            {
                this.ID = id;
            }

            Debug.Assert(this.ID != 0, "Building_ID cannot be 0 - this throws everything off");

            if (int.TryParse(row["Company_ID"].ToString(), out id))
            {
                this.company_id = id;
            }

            Debug.Assert(this.company_id != 0, "Every Building must have a cooresponding owner. Company_ID cannot be 0");

            // Assign the opening strings. Pretty straightforward.
            this.proposal_number = row["ProposalNumber"].ToString();
            this.proposal_file = row["ProposalFile"].ToString();
            this.name = row["Name"].ToString();
            this.address.Street = row["Address"].ToString();
            this.address.City = row["City"].ToString();
            this.address.State = row["State"].ToString();
            this.address.Zip = row["Zip"].ToString();

            // Get the county enum based on this string.
            this.county = StringToCountyEnum(row["County"].ToString());

            // Check if the 
            decimal d;
            if (decimal.TryParse(row["FirmFee"].ToString(), out d))
            {
                this.firm_fee.Value = d;
            }

            if (decimal.TryParse(row["HourlyFee"].ToString(), out d))
            {
                this.hourly_fee.Value = d;
            }

            this.anniversary = StringtoMonthEnum(row["Anniversary"].ToString());

            this.contractor = row["Contractor"].ToString();

            bool.TryParse(row["Active"].ToString(), out this.active);

            float lat, lng;
            float.TryParse(row["Latitude"].ToString(), out lat);
            float.TryParse(row["Longitude"].ToString(), out lng);

            // If bot the lat and long parsed correctly, set up the coordinates.
            if (lat != 0.0D && lng != 0.0D)
            {
                this.coordinates = new GeographicCoordinates(lat, lng);
            }

            // Pull a list of contacts (If any exist for this building)
            this.contact_list = new List<Contact>();
            foreach (DataRow con in SQL.Query.Select(string.Format(
                                                     "SELECT DISTINCT * FROM Contact " +
                                                     "JOIN Building_Contact_Relations ON Contact.Contact_ID = Building_Contact_Relations.Contact_ID " +
                                                     "WHERE Building_Contact_Relations.Building_ID = {0}",
                                                     this.ID)).Rows)
            {
                this.contact_list.Add(new Contact(con));
            }
        }

        /// <summary> A struct specifically for containing inspection data grouped by visit.</summary>
        public struct InspectionHistory
        {
            /// <summary> The date of the Inspection. </summary>
            public DateTime Date;

            /// <summary> The Type of the Inspection. </summary>
            public string Type;

            /// <summary> The Status of the Inspection. </summary>
            public string Status;

            /// <summary> The Inspector who performed the Inspection. </summary>
            public string Inspector;

            /// <summary> Is there a report available. </summary>
            public bool Report;

            /// <summary>Initializes a new instance of the <see cref="InspectionHistory"/> struct.</summary>
            /// <param name="row">A DataRow containing Date, Type, Status, Inspector, and Report Information.</param>
            public InspectionHistory(DataRow row)
            {
                DateTime.TryParse(row["Date"].ToString(), out this.Date);
                this.Type = row["InspectionType"].ToString();
                this.Status = row["Status"].ToString();
                this.Inspector = row["Inspector"].ToString();

                if (row["Report"].ToString() != string.Empty)
                {
                    this.Report = true;
                }
                else
                {
                    this.Report = false;
                }
            }
        }
    }
}