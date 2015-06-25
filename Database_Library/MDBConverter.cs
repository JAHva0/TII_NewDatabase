namespace Database_Library
{
    using System;
    using System.Data;
    using System.Data.OleDb;
    using System.IO;

    public static class MDBConverter
    {
        public static void Create(string filename)
        {
            OleDbConnection connection;
            OleDbDataAdapter adapter;

            File.Delete(filename);
            // Create the database via ADOX if it does not already exist.
            if (!File.Exists(filename))
            {
                ADOX.Catalog cat = new ADOX.Catalog();
                cat.Create("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filename);
                cat = null;
            }

            // Connect to the Database
            connection = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filename);
            connection.Open();

            #region Create Company Table
            // Create the company table in the OLE database
            string CreateCompanyTableQuery =
                "CREATE TABLE Company " +
                "( " +
                "   ID  int NOT NULL, " +
                "   Name varchar(50) NOT NULL, " +
                "   Street varchar(50) NOT NULL, " +
                "   City varchar(30) NOT NULL, " +
                "   State varchar(2) NOT NULL, " +
                "   Zip int NOT NULL " +
                ");";

            using (OleDbCommand command = new OleDbCommand(CreateCompanyTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            foreach (DataRow CompanyRow in SQL.Query.Select(
                "SELECT Company.ID, " +
                "Name, " +
                "Street, " +
                "(SELECT Name FROM City WHERE City_ID = ID) AS City, " +
                "(SELECT Abbreviation FROM State WHERE State_ID = ID) as State, " +
                "Zip " +
                "FROM Company " +
                "JOIN Address ON Address_ID = Address.ID").Rows)
            {
                string InsertCompanyCommand = string.Format(
                    "INSERT INTO Company (ID, Name, Street, City, State, Zip) VALUES ({0}, '{1}', '{2}', '{3}', '{4}', {5})",
                    CompanyRow["ID"].ToString(),
                    CompanyRow["Name"].ToString().Replace("'", "''"),
                    CompanyRow["Street"].ToString(),
                    CompanyRow["City"].ToString(),
                    CompanyRow["State"].ToString(),
                    CompanyRow["Zip"].ToString());
                using (OleDbCommand command = new OleDbCommand(InsertCompanyCommand, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            // Add in the primary Key constraint
            using (OleDbCommand command = new OleDbCommand("ALTER TABLE Company ALTER COLUMN ID int PRIMARY KEY", connection))
            {
                command.ExecuteNonQuery();
            }
            #endregion

            #region Create Building Table
            // Create the building table in the OLE Database
            string CreateBuildingTableQuery =
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
                "   FOREIGN KEY (Company_ID) REFERENCES Company(ID) " +
                ");";

            using (OleDbCommand command = new OleDbCommand(CreateBuildingTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            foreach (DataRow BuildingRow in SQL.Query.Select(
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
                "ORDER BY Building.ID").Rows )
            {
                string InsertBuildingCommand = string.Format(
                    "INSERT INTO BUILDING (ID, Company_ID, Name, Street, City, State, Zip, County, Firm_Fee, Hourly_Fee, Anniversary, Contractor, Active)" +
                    "VALUES ({0}, {1}, '{2}', '{3}', '{4}', '{5}', {6}, '{7}', {8}, {9}, {10}, '{11}', {12})",
                    ConvertDBObjectToString(BuildingRow["ID"]),
                    ConvertDBObjectToString(BuildingRow["Company_ID"]),
                    ConvertDBObjectToString(BuildingRow["Name"]),
                    ConvertDBObjectToString(BuildingRow["Street"]),
                    ConvertDBObjectToString(BuildingRow["City"]),
                    ConvertDBObjectToString(BuildingRow["State"]),
                    ConvertDBObjectToString(BuildingRow["Zip"]),
                    ConvertDBObjectToString(BuildingRow["County"]),
                    ConvertDBObjectToString(BuildingRow["Firm_Fee"]),
                    ConvertDBObjectToString(BuildingRow["Hourly_Fee"]),
                    ConvertDBObjectToString(BuildingRow["Anniversary"]),
                    ConvertDBObjectToString(BuildingRow["Contractor"]),
                    ConvertDBObjectToString(BuildingRow["Active"]));

                using (OleDbCommand command = new OleDbCommand(InsertBuildingCommand, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            // Add in the primary Key constraint
            using (OleDbCommand command = new OleDbCommand("ALTER TABLE Building ALTER COLUMN ID int PRIMARY KEY", connection))
            {
                command.ExecuteNonQuery();
            }
            #endregion

            #region Create Elevator Table
            string CreateElevatorTableQuery =
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

            using (OleDbCommand command = new OleDbCommand(CreateElevatorTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            foreach (DataRow ElevatorRow in SQL.Query.Select(
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
                DateTime Previous = DateTime.MinValue;
                DateTime Recent = DateTime.MinValue;
                DateTime Visit = DateTime.MinValue;
                DateTime FiveYear = DateTime.MinValue;
                bool status = false;
                
                // Get the two most recent inspections for this elevator
                string InspectionHistoryQuery = string.Format(
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
                    ConvertDBObjectToString(ElevatorRow["ID"]));
                DataTable InspectionHistory = SQL.Query.Select(InspectionHistoryQuery);

                // If there's at least one inspection result, store the top date as the most 
                for (int i = 0; i < InspectionHistory.Rows.Count; i++)
                {
                    if (Visit == DateTime.MinValue)
                    {
                        // The most recent visit will always be the top item
                        Visit = Convert.ToDateTime(InspectionHistory.Rows[i]["Date"].ToString());
                        bool.TryParse(InspectionHistory.Rows[i]["Clean"].ToString(), out status);
                    }
                    // if the current row is an annual and recent is still 
                    if (InspectionHistory.Rows[i]["InspectionType"].ToString() == "Annual")
                    {
                        if (Recent == DateTime.MinValue)
                        {
                            Recent = Convert.ToDateTime(InspectionHistory.Rows[i]["Date"].ToString());
                        }
                        else if (Previous == DateTime.MinValue)
                        {
                            Previous = Convert.ToDateTime(InspectionHistory.Rows[i]["Date"].ToString());
                        }
                    }

                    // If the current row is a five year and we have no other recent five year dates
                    if (InspectionHistory.Rows[i]["InspectionType"].ToString() == "Five Year Test" && FiveYear == DateTime.MinValue)
                    {
                        FiveYear = Convert.ToDateTime(InspectionHistory.Rows[i]["Date"].ToString());
                    }
                }

                string InsertElevatorCommand = string.Format(
                    "INSERT INTO Elevator (ID, Building_ID, ElevNumber, Type, Nickname, PreviousAnnual, MostRecentAnnual, FiveYearTest, LastVisit, Status) " +
                    "VALUES ({0}, {1}, '{2}', '{3}', '{4}', {5}, {6}, {7}, {8}, {9})",
                    ConvertDBObjectToString(ElevatorRow["ID"]),
                    ConvertDBObjectToString(ElevatorRow["Building_ID"]),
                    ConvertDBObjectToString(ElevatorRow["Number"]),
                    ConvertDBObjectToString(ElevatorRow["Type"]),
                    ConvertDBObjectToString(ElevatorRow["Nickname"]),
                    ConvertDateTimeToDBString(Previous),
                    ConvertDateTimeToDBString(Recent),
                    ConvertDateTimeToDBString(FiveYear),
                    ConvertDateTimeToDBString(Visit),
                    status.ToString());

                using (OleDbCommand command = new OleDbCommand(InsertElevatorCommand, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            // Add in the primary Key constraint
            using (OleDbCommand command = new OleDbCommand("ALTER TABLE Elevator ALTER COLUMN ID int PRIMARY KEY", connection))
            {
                command.ExecuteNonQuery();
            }
            #endregion

            return;

            #region Create Inspection Table
            string CreateInspectionTableQuery =
                "CREATE TABLE Inspection " +
                "( " +
                "   ID int NOT NULL, " +
                "   Elevator_ID int NOT NULL, " +
                "   InspDate date NOT NULL, " +
                "   Type varchar(40) NOT NULL, " +
                "   Clean bit, " +
                "   Inspector varchar(25), " +
                "   FOREIGN KEY (Elevator_ID) REFERENCES Elevator(ID) " +
                ");";

            using (OleDbCommand command = new OleDbCommand(CreateInspectionTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            foreach (DataRow InspectionRow in SQL.Query.Select(
                "SELECT " +
                "ID, " +
                "Elevator_ID, " +
                "Date, " +
                "(SELECT Name FROM InspectionType WHERE InspectionType_ID = ID) as Type, " +
                "Clean, " +
                "(SELECT Name FROM Inspector WHERE Inspector_ID = ID) as Inspector " +
                "FROM Inspection").Rows)
            {
                string InsertInspectionCommand = string.Format(
                    "INSERT INTO Inspection (ID, Elevator_ID, InspDate, Type, Clean, Inspector) VALUES ({0}, {1}, '{2}', '{3}', {4}, '{5}')",
                    ConvertDBObjectToString(InspectionRow["ID"]),
                    ConvertDBObjectToString(InspectionRow["Elevator_ID"]),
                    ConvertDBObjectToString(InspectionRow["Date"]),
                    ConvertDBObjectToString(InspectionRow["Type"]),
                    ConvertDBObjectToString(InspectionRow["Clean"]),
                    ConvertDBObjectToString(InspectionRow["Inspector"]));

                using (OleDbCommand command = new OleDbCommand(InsertInspectionCommand, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            // Add in the primary Key constraint
            using (OleDbCommand command = new OleDbCommand("ALTER TABLE Inspection ALTER COLUMN ID int PRIMARY KEY", connection))
            {
                command.ExecuteNonQuery();
            }
            #endregion
        }

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
    }
}
