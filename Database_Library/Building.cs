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

        /// <summary> Boolean for Fire Emergency Service. </summary>
        private bool fire_emergency_service;

        /// <summary> Boolean for Smoke Detectors. </summary>
        private bool emergency_power;

        /// <summary> Boolean for Heat Detectors. </summary>
        private bool heat_detectors;

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
            DataTable tbl = SQL.Query.Select(
                "SELECT Building.ID, Company_ID, Building.Name, Street, City.Name AS City, State.Abbreviation AS State, Zip, County.Name AS County, Firm_Fee, Hourly_Fee, Anniversary, Contractor.Name AS Contractor, " +
                "Active, FES, Emergency_Power, Smoke_Detectors, Heat_Detectors, Latitude, Longitude " +
                "FROM Building " +
                "JOIN Address ON Address_ID = Address.ID " +
                "JOIN City ON City_ID = City.ID " +
                "JOIN State ON State_ID = State.ID " +
                "JOIN County ON County_ID = County.ID " +
                "LEFT JOIN Contractor ON Contractor_ID = Contractor.ID " +
                "WHERE Building.ID = " + building_ID.ToString());
            Debug.Assert(tbl.Rows.Count == 1, string.Format("Building Query for Building_ID {0} has returned {1} rows", building_ID, tbl.Rows.Count));
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
            Debug.Assert(tbl.Rows.Count == 1, string.Format("Building Query for Address {0} has returned {1} rows", building_address, tbl.Rows.Count));
            this.LoadFromDatabase(tbl.Rows[0]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Building"/> class.
        /// </summary>
        /// <param name="buildingData"> A properly formatted <see cref="DataRow"/> from the SQL Table. </param>
        public Building(DataRow buildingData)
        {
            this.LoadFromDatabase(buildingData);
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
        /// Gets a list of contractors currently present in the database.
        /// </summary>
        /// <value>A list of the contractors in the database.</value>
        public static List<string> ContractorList
        {
            get
            {
                List<string> contractorList = new List<string>();
                foreach (DataRow c in SQL.Query.Select("Name", "Contractor", "1=1").Rows)
                {
                    contractorList.Add(c["Name"].ToString());
                }

                return contractorList;
            }
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
                    this.BaseObject_Edited(this, "Company_ID", this.company_id, value.ID.Value);
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

        /// <summary> Gets the address of this building. </summary>
        /// <value> The formatted address of this building. </value>
        public string FormattedAddress
        {
            get
            {
                return this.address.ToString();
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

        /// <summary>
        /// Gets or sets a value indicating whether or not the building is equipped with Fire Emergency Service.
        /// </summary>
        /// <value>The Fire Emergency Status for this building.</value>
        public bool FireEmergencyService
        {
            get
            {
                return this.fire_emergency_service;
            }

            set
            {
                // as long as this value is different from what we already have, change it.
                if (value != this.fire_emergency_service)
                {
                    this.BaseObject_Edited(this, "FES", this.fire_emergency_service.ToString(), value.ToString());
                    this.fire_emergency_service = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the building is equipped with Smoke Detectors.
        /// </summary>
        /// /// <value>The Smoke Detector Status for this building.</value>
        public bool EmergencyPower
        {
            get
            {
                return this.emergency_power;
            }

            set
            {
                // as long as this value is different from what we already have, change it.
                if (value != this.emergency_power)
                {
                    this.BaseObject_Edited(this, "Smks", this.emergency_power.ToString(), value.ToString());
                    this.emergency_power = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the building is equipped with Heat Detectors.
        /// </summary>
        /// /// <value>The Heat Detector Status for this building.</value>
        public bool HeatDetectors
        {
            get
            {
                return this.heat_detectors;
            }

            set
            {
                // as long as this value is different from what we already have, change it.
                if (value != this.heat_detectors)
                {
                    this.BaseObject_Edited(this, "FES", this.heat_detectors.ToString(), value.ToString());
                    this.heat_detectors = value;
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
                foreach (DataRow row in SQL.Query.Select(string.Format(
                    "SELECT Elevator.ID, Building_ID, Number, ElevatorTypes.Name AS Type, Nickname FROM Elevator " +
                    "JOIN ElevatorTypes ON Type_ID = ElevatorTypes.ID " +
                    "WHERE Building_ID = {0}", 
                    this.ID)).Rows)
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
                    "SELECT DISTINCT Date, InspectionType.Name AS Type, Clean, Inspector.Name as Inspector, Documents.FilePath AS Report " +
                    "FROM Inspection " +
                    "JOIN InspectionType ON InspectionType_ID = InspectionType.ID " +
                    "JOIN Inspector ON Inspection.Inspector_ID = Inspector.ID " +
                    "LEFT JOIN Documents ON Report_ID = Documents.ID " +
                    "WHERE Elevator_ID IN " +
                    "    ( " +
                    "    SELECT ID " +
                    "    FROM Elevator WHERE " +
                    "    Building_ID = {0} " +
                    "    ) " +
                    "AND Clean IS NOT NULL " +
                    "ORDER BY Date Desc", 
                    this.ID)).Rows)
                {
                    history.Add(new InspectionHistory(row));
                }

                // Pare out results where one unit was clean and another was not. Make sure just the unclean listing remains
                List<InspectionHistory> entriesToRemove = new List<InspectionHistory>();

                foreach (InspectionHistory inspection in history)
                {
                    var duplicateDates = from i in history
                                         where i.Date == inspection.Date && i.Type == inspection.Type
                                         select i;

                    if (duplicateDates.Count() > 1)
                    {
                        entriesToRemove.Add(duplicateDates.Single(x => x.Status == "Clean"));
                    }
                }

                foreach (InspectionHistory entry in entriesToRemove)
                {
                    history.Remove(entry);
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

        /// <summary> Gets a condensed string of all of the information contained within this class. </summary>
        /// <value> A string of values separated by the pipe character. </value>
        public string ToCondensedString
        {
            get
            {
                return string.Format(
                    "{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}|{15}|{16}|{17}|{18}|{19}",
                    this.ID,
                    this.company_id,
                    this.proposal_number,
                    this.proposal_file,
                    this.name,
                    this.address.Street,
                    this.address.City,
                    this.address.State,
                    this.address.Zip,
                    this.County,
                    this.firm_fee,
                    this.hourly_fee,
                    this.Anniversary,
                    this.contractor,
                    this.active,
                    this.coordinates.Latitude,
                    this.coordinates.Longitude,
                    this.fire_emergency_service,
                    this.emergency_power,
                    this.heat_detectors);
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
                new SQLColumn("Name", this.name),
                new SQLColumn("Address_ID", this.address.GetDatabaseID()),
                new SQLColumn("Proposal_ID", string.Empty),
                new SQLColumn("County_ID", string.Format("(SELECT ID FROM County WHERE Name = '{0}')", BaseObject.GetEnumDescription(this.county))),
                new SQLColumn("Firm_Fee", this.firm_fee.Value),
                new SQLColumn("Hourly_Fee", this.hourly_fee.Value),
                new SQLColumn("Anniversary", MonthToInt(this.anniversary)),
                new SQLColumn("Contractor_ID", string.Format("(SELECT ID FROM Contractor WHERE Name = '{0}')", this.contractor)),
                new SQLColumn("Active", this.active),
                new SQLColumn("Latitude", this.coordinates.Latitude),
                new SQLColumn("Longitude", this.coordinates.Longitude),
                new SQLColumn("FES", this.fire_emergency_service),
                new SQLColumn("Emergency_Power", this.emergency_power),
                new SQLColumn("Smoke_Detectors", string.Empty),
                new SQLColumn("Heat_Detectors", this.heat_detectors)
            };

            if (this.ID == null)
            {
                // If ID is null, then this is a new entry to the database and we should use an insert statement.
                success = SQL.Query.Insert("Building", classData);
            }
            else
            {
                // If the ID is not null, then we are just updating the record which has the Building_ID we pulled to start with.
                success = SQL.Query.Update("Building", classData, string.Format("ID = {0}", this.ID));
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
                default: throw new ArgumentException(string.Format("Invalid County Name: '{0}'", c));
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
                default: throw new ArgumentException(string.Format("Invalid Month Selection: '{0}'", m));
            }
        }

        /// <summary>
        /// Converts an integer value to it's corresponding month enumerator. 
        /// </summary>
        /// <param name="m"> A string, either empty or a number between 1 and 12. </param>
        /// <returns> The corresponding <see cref="Month"/>. </returns>
        private static Month InttoMonthEnum(string m)
        {
            switch (m)
            {
                case "1": return Month.JAN;
                case "2": return Month.FEB;
                case "3": return Month.MAR;
                case "4": return Month.APR;
                case "5": return Month.MAY;
                case "6": return Month.JUN;
                case "7": return Month.JUL;
                case "8": return Month.AUG;
                case "9": return Month.SEP;
                case "10": return Month.OCT;
                case "11": return Month.NOV;
                case "12": return Month.DEC;
                case "": return Month.NONE;
                default: throw new ArgumentException(string.Format("Invalid Month Selection: '{0}'", m));
            }
        }

        /// <summary>
        /// Converts a <see cref="Month"/> to it's corresponding integer value.
        /// </summary>
        /// <param name="m">The <see cref="Month"/> to convert. </param>
        /// <returns> An integer between 0 and 12. </returns>
        private static int MonthToInt(Month m)
        {
            switch (m)
            {
                case Month.JAN: return 1;
                case Month.FEB: return 2;
                case Month.MAR: return 3;
                case Month.APR: return 4;
                case Month.MAY: return 5;
                case Month.JUN: return 6;
                case Month.JUL: return 7;
                case Month.AUG: return 8;
                case Month.SEP: return 9;
                case Month.OCT: return 10;
                case Month.NOV: return 11;
                case Month.DEC: return 12;
                case Month.NONE: return 0;
            }

            throw new Exception("We should never see this exception. How did you get here?.");
        }

        /// <summary>
        /// Private method to fill the class with information.
        /// </summary>
        /// <param name="row">A Pre-filled DataRow from which to fill the class information.</param>
        private void LoadFromDatabase(DataRow row)
        {
            Debug.Assert(row != null, "Row cannot be null");

            try
            {
                // Test the ID because it seems like a nice thing to do.
                int id;
                if (int.TryParse(row["ID"].ToString(), out id))
                {
                    this.ID = id;
                }

                Debug.Assert(this.ID != 0, "Building ID cannot be 0 - this throws everything off");

                if (int.TryParse(row["Company_ID"].ToString(), out id))
                {
                    this.company_id = id;
                }

                Debug.Assert(this.company_id != 0, "Every Building must have a cooresponding owner. Company_ID cannot be 0");

                // Assign the opening strings. Pretty straightforward.
                this.name = row["Name"].ToString();
                this.address.Street = row["Street"].ToString();
                this.address.City = row["City"].ToString();
                this.address.State = row["State"].ToString();
                this.address.Zip = row["Zip"].ToString();

                // Get the county enum based on this string.
                this.county = StringToCountyEnum(row["County"].ToString());

                // Check if the 
                decimal d;
                if (decimal.TryParse(row["Firm_Fee"].ToString(), out d))
                {
                    this.firm_fee.Value = d;
                }

                if (decimal.TryParse(row["Hourly_Fee"].ToString(), out d))
                {
                    this.hourly_fee.Value = d;
                }

                this.anniversary = InttoMonthEnum(row["Anniversary"].ToString());

                this.contractor = row["Contractor"].ToString();

                bool.TryParse(row["Active"].ToString(), out this.active);
                bool.TryParse(row["FES"].ToString(), out this.fire_emergency_service);
                bool.TryParse(row["Emergency_Power"].ToString(), out this.emergency_power);
                bool.TryParse(row["Heat_Detectors"].ToString(), out this.heat_detectors);

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
                                                         "JOIN Building_Contact_Relations ON Contact.ID = Building_Contact_Relations.Contact_ID " +
                                                         "WHERE Building_Contact_Relations.Building_ID = {0}",
                                                         this.ID)).Rows)
                {
                    this.contact_list.Add(new Contact(con));
                }
            }
            catch (Exception ex)
            {
                throw new InvalidEnumArgumentException(string.Format("\r\rError Parsing Database Information:\rBuilding_ID: '{0}'\r{1}\r\r{2}", this.ID, this.address.ToString(), ex.Message));
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
                this.Type = row["Type"].ToString().Trim(); // For some reason the SQL Table returns the string with white space trailing. 
                if (row["Clean"].ToString() == "True")
                {
                    this.Status = "Clean";
                }
                else
                {
                    this.Status = "Outstanding Items";
                }

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