// <summary> Creates an access database in the old format from the SQL data. </summary>

namespace Database
{
    using System;
    using System.Data;
    using System.Data.OleDb;
    using System.IO;

    /// <summary>
    /// Creates an access database from the SQL data.
    /// </summary>
    public static class MDBConverter
    {
        /// <summary>
        /// Creates a access database file with the given file path.
        /// </summary>
        /// <param name="filepath">The file name and path to create. Must not already exist.</param>
        public static void Create(string filepath)
        {
            OleDbConnection connection;

            File.Delete(filepath);

            // Create the database via ADOX if it does not already exist.
            if (!File.Exists(filepath))
            {
                ADOX.Catalog cat = new ADOX.Catalog();
                cat.Create("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filepath);
                cat = null;
            }

            // Connect to the Database
            connection = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filepath);
            connection.Open();

            // Create the company table in the OLE database
            string createCompanyTableQuery =
                "CREATE TABLE Company " +
                "( " +
                "   ID  int NOT NULL, " +
                "   Name varchar(50) NOT NULL, " +
                "   Street varchar(50) NOT NULL, " +
                "   City varchar(30) NOT NULL, " +
                "   State varchar(2) NOT NULL, " +
                "   Zip int NOT NULL, " +
                "   Contact varchar(100)" +
                ");";

            using (OleDbCommand command = new OleDbCommand(createCompanyTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            foreach (DataRow companyRow in SQL.Query.Select(
                "SELECT Company.ID, " +
                "Name, " +
                "Street, " +
                "(SELECT Name FROM City WHERE City_ID = ID) AS City, " +
                "(SELECT Abbreviation FROM State WHERE State_ID = ID) as State, " +
                "Zip " +
                "FROM Company " +
                "JOIN Address ON Address_ID = Address.ID " +
                "WHERE Company.ID IN " +
                "( " +
                "    SELECT Company_ID " +
                "    FROM Building " +
                "    JOIN Address ON Address_ID = Address.ID " +
                "    WHERE State_ID = (SELECT ID FROM State WHERE Name = 'Maryland') " +
                "    AND Active = 'True' " +
                ")").Rows)
            {
                DataTable contacts = SQL.Query.Select(
                    string.Format(
                        "SELECT Top 1 ID, Name, OfficePhone, OfficeExt, CellPhone, Fax, Email " +
                        "FROM Contact " +
                        "LEFT JOIN Company_Contact_Relations ON Contact.ID = Contact_ID " +
                        "WHERE Company_ID = {0}",
                        companyRow["ID"].ToString()));

                string ContactInfo;
                if (contacts.Rows.Count != 1)
                {
                    ContactInfo = null;
                }
                else
                {
                    ContactInfo = ConvertContactInfoToString(contacts.Rows[0]);
                }

                string insertCompanyCommand = string.Format(
                    "INSERT INTO Company (ID, Name, Street, City, State, Zip, Contact) VALUES ({0}, '{1}', '{2}', '{3}', '{4}', {5}, '{6}')",
                    companyRow["ID"].ToString(),
                    companyRow["Name"].ToString().Replace("'", "''"),
                    companyRow["Street"].ToString(),
                    companyRow["City"].ToString(),
                    companyRow["State"].ToString(),
                    companyRow["Zip"].ToString(),
                    ContactInfo);
                using (OleDbCommand command = new OleDbCommand(insertCompanyCommand, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            // Add in the primary Key constraint
            using (OleDbCommand command = new OleDbCommand("ALTER TABLE Company ALTER COLUMN ID int PRIMARY KEY", connection))
            {
                command.ExecuteNonQuery();
            }

            // Create the building table in the OLE Database
            string createBuildingTableQuery =
                "CREATE TABLE Building " +
                "( " +
                "   ID int NOT NULL, " +
                "   Company_ID int NOT NULL, " +
                "   Name varchar(50) NOT NULL, " +
                "   Street varchar(50) NOT NULL, " +
                "   City varchar(30) NOT NULL, " +
                "   State varchar(2) NOT NULL, " +
                "   Zip int NOT NULL, " +
                "   County varchar(20) NOT NULL, " +
                "   Firm_Fee money, " +
                "   Hourly_Fee money, " +
                "   Anniversary tinyint, " +
                "   Contractor varchar(30), " +
                "   Active bit, " +
                "   Contact varchar(100), " +
                "   FOREIGN KEY (Company_ID) REFERENCES Company(ID) " +
                ");";

            using (OleDbCommand command = new OleDbCommand(createBuildingTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            foreach (DataRow buildingRow in SQL.Query.Select(
                "SELECT " +
                "Building.ID, " +
                "Company_ID, " +
                "Name, " +
                "Street, " +
                "(SELECT Name FROM City WHERE City_ID = ID) as City, " +
                "(SELECT Abbreviation FROM State WHERE State_ID = ID) as State, " +
                "Zip, " +
                "(SELECT Name FROM County WHERE County_ID = ID) as County, " +
                "Firm_Fee, " +
                "Hourly_Fee, " +
                "Anniversary, " +
                "(SELECT Name FROM Contractor WHERE Contractor_ID = ID) as Contractor, " +
                "Active " +
                "FROM Building " +
                "JOIN Address ON Address_ID = Address.ID " +
                "WHERE Active = 'True' " +
                "AND State_ID = 20 " +
                "ORDER BY Building.ID").Rows)
            {
                DataTable contacts = SQL.Query.Select(
                    string.Format(
                        "SELECT Top 1 ID, Name, OfficePhone, OfficeExt, CellPhone, Fax, Email " +
                        "FROM Contact " +
                        "LEFT JOIN Company_Contact_Relations ON Contact.ID = Contact_ID " +
                        "WHERE Company_ID = {0}",
                        buildingRow["ID"].ToString()));

                string ContactInfo;
                if (contacts.Rows.Count != 1)
                {
                    ContactInfo = null;
                }
                else
                {
                    ContactInfo = ConvertContactInfoToString(contacts.Rows[0]);
                }

                string insertBuildingCommand = string.Format(
                    "INSERT INTO BUILDING (ID, Company_ID, Name, Street, City, State, Zip, County, Firm_Fee, Hourly_Fee, Anniversary, Contractor, Active, Contact)" +
                    "VALUES ({0}, {1}, '{2}', '{3}', '{4}', '{5}', {6}, '{7}', {8}, {9}, {10}, '{11}', {12}, '{13}')",
                    ConvertDBObjectToString(buildingRow["ID"]),
                    ConvertDBObjectToString(buildingRow["Company_ID"]),
                    ConvertDBObjectToString(buildingRow["Name"]),
                    ConvertDBObjectToString(buildingRow["Street"]),
                    ConvertDBObjectToString(buildingRow["City"]),
                    ConvertDBObjectToString(buildingRow["State"]),
                    ConvertDBObjectToString(buildingRow["Zip"]),
                    ConvertDBObjectToString(buildingRow["County"]),
                    ConvertDBObjectToString(buildingRow["Firm_Fee"]),
                    ConvertDBObjectToString(buildingRow["Hourly_Fee"]),
                    ConvertDBObjectToString(buildingRow["Anniversary"]),
                    ConvertDBObjectToString(buildingRow["Contractor"]),
                    ConvertDBObjectToString(buildingRow["Active"]),
                    ContactInfo);

                using (OleDbCommand command = new OleDbCommand(insertBuildingCommand, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            // Add in the primary Key constraint
            using (OleDbCommand command = new OleDbCommand("ALTER TABLE Building ALTER COLUMN ID int PRIMARY KEY", connection))
            {
                command.ExecuteNonQuery();
            }

            string createElevatorTableQuery =
                "CREATE TABLE Elevator " +
                "( " +
                "   ID int NOT NULL, " +
                "   Building_ID int NOT NULL, " +
                "   ElevNumber varchar(20) NOT NULL, " +
                "   Type varchar(50) NOT NULL, " +
                "   Nickname varchar(20) NOT NULL, " +
                "   PreviousAnnual date, " +
                "   MostRecentAnnual date, " +
                "   FiveYearTest date, " +
                "   LastVisit date, " +
                "   Status bit, " +
                "   FOREIGN KEY (Building_ID) REFERENCES Building(ID) " +
                ");";

            using (OleDbCommand command = new OleDbCommand(createElevatorTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            foreach (DataRow elevatorRow in SQL.Query.Select(
                "SELECT " +
                "ID, " +
                "Building_ID, " +
                "Number, " +
                "(SELECT Name FROM ElevatorTypes WHERE Type_ID = ID) as Type, " +
                "Nickname " +
                "FROM Elevator " +
                "WHERE Building_ID IN " +
                "( " +
                "SELECT Building.ID " +
                "FROM Building " +
                "JOIN Address ON Address_ID = Address.ID " +
                "WHERE State_ID = " +
                "    ( " +
                "    SELECT ID " +
                "    FROM State " +
                "    WHERE Name = 'Maryland' " +
                "    ) " +
                "AND Active = 'True' " +
                ")").Rows)
            {
                DateTime previous = DateTime.MinValue;
                DateTime recent = DateTime.MinValue;
                DateTime visit = DateTime.MinValue;
                DateTime fiveYear = DateTime.MinValue;
                bool status = false;
                
                // Get the two most recent inspections for this elevator
                string inspectionHistoryQuery = string.Format(
                    "SELECT " +
                    "ID, " +
                    "Date, " +
                    "(SELECT Name FROM InspectionType WHERE InspectionType_ID = ID) as InspectionType, " +
                    "Clean " +
                    "FROM Inspection " +
                    "WHERE Elevator_ID = {0} " +
                    "AND " +
                    "( " +
                    "    SELECT State_ID FROM Building JOIN Address ON Address.ID = Address_ID WHERE Building.ID = " +
                    "    ( " +
                    "        SELECT Building_ID FROM Elevator WHERE ID = {0} " +
                    "    ) " +
                    ") = (SELECT ID FROM State WHERE Name = 'Maryland') " +
                    "ORDER BY Date DESC",
                    ConvertDBObjectToString(elevatorRow["ID"]));
                DataTable inspectionHistory = SQL.Query.Select(inspectionHistoryQuery);

                // If there's at least one inspection result, store the top date as the most 
                for (int i = 0; i < inspectionHistory.Rows.Count; i++)
                {
                    if (visit == DateTime.MinValue)
                    {
                        // The most recent visit will always be the top item
                        visit = Convert.ToDateTime(inspectionHistory.Rows[i]["Date"].ToString());
                        bool.TryParse(inspectionHistory.Rows[i]["Clean"].ToString(), out status);
                    }

                    // if the current row is an annual and recent is still 
                    if (inspectionHistory.Rows[i]["InspectionType"].ToString() == "Annual")
                    {
                        if (recent == DateTime.MinValue)
                        {
                            recent = Convert.ToDateTime(inspectionHistory.Rows[i]["Date"].ToString());
                        }
                        else if (previous == DateTime.MinValue)
                        {
                            previous = Convert.ToDateTime(inspectionHistory.Rows[i]["Date"].ToString());
                        }
                    }

                    // If the current row is a five year and we have no other recent five year dates
                    if (inspectionHistory.Rows[i]["InspectionType"].ToString() == "Five Year Test" && fiveYear == DateTime.MinValue)
                    {
                        fiveYear = Convert.ToDateTime(inspectionHistory.Rows[i]["Date"].ToString());
                    }
                }

                string insertElevatorCommand = string.Format(
                    "INSERT INTO Elevator (ID, Building_ID, ElevNumber, Type, Nickname, PreviousAnnual, MostRecentAnnual, FiveYearTest, LastVisit, Status) " +
                    "VALUES ({0}, {1}, '{2}', '{3}', '{4}', {5}, {6}, {7}, {8}, {9})",
                    ConvertDBObjectToString(elevatorRow["ID"]),
                    ConvertDBObjectToString(elevatorRow["Building_ID"]),
                    ConvertDBObjectToString(elevatorRow["Number"]),
                    ConvertDBObjectToString(elevatorRow["Type"]),
                    ConvertDBObjectToString(elevatorRow["Nickname"]),
                    ConvertDateTimeToDBString(previous),
                    ConvertDateTimeToDBString(recent),
                    ConvertDateTimeToDBString(fiveYear),
                    ConvertDateTimeToDBString(visit),
                    status.ToString());

                using (OleDbCommand command = new OleDbCommand(insertElevatorCommand, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            // Add in the primary Key constraint
            using (OleDbCommand command = new OleDbCommand("ALTER TABLE Elevator ALTER COLUMN ID int PRIMARY KEY", connection))
            {
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Converts an object retrieved from a <see cref="DataTable"/> to a string which can be used to create a SQL query.
        /// </summary>
        /// <param name="o"> The Object to convert.</param>
        /// <returns> A SQL ready string. </returns>
        private static string ConvertDBObjectToString(object o)
        {
            if (o.ToString() == string.Empty)
            {
                return "NULL";
            }
            else
            {
                return o.ToString().Replace("'", "''");
            }
        }

        /// <summary>
        /// Converts an object retrieved from a <see cref="DataTable"/> to a string which can be used to create a SQL query.
        /// </summary>
        /// <param name="t"> A <see cref="DateTime"/> to convert. </param>
        /// <returns> A SQL ready string. </returns>
        private static string ConvertDateTimeToDBString(DateTime t)
        {
            if (t == DateTime.MinValue)
            {
                return "NULL";
            }
            else
            {
                return string.Format("'{0}'", t.ToShortDateString());
            }
        }

        private static string ConvertContactInfoToString(DataRow row)
        {
            Contact c = new Contact(row);
            string returnstring = string.Empty;
            returnstring += c.Name;
            if (c.OfficePhone != null)
            {
                returnstring += " Office: " + c.OfficePhone.ToString();
            }
            if (c.CellPhone != null)
            {
                returnstring += " Cell: " + c.CellPhone.ToString();
            }
            if (c.Email != string.Empty)
            {
                returnstring += " " + c.Email;
            }

            return returnstring;
        }
    }
}
