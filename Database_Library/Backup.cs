namespace Database
{
    using System;
    using System.Data;
    using System.IO;
    
    public static class Backup
    {
        public static string BackupLocation { get; set; }

        public static void CreateNew()
        {
            string filename = string.Format(
                "{0}{1}{2}-{3}-DB_Backup.tiibackup",
                DateTime.Now.Year.ToString(),
                DateTime.Now.Month.ToString().PadLeft(2, '0'),
                DateTime.Now.Day.ToString().PadLeft(2, '0'),
                DateTime.Now.Hour.ToString().PadLeft(2, '0'));
            
            using (StreamWriter writer = new StreamWriter(BackupLocation + @"\" + filename))
            {
                writer.WriteLine("TABLE:Company");
                foreach (DataRow companyData in SQL.Query.Select("*", "Company", "1=1").Rows)
                {
                    Company c = new Company(companyData);
                    writer.WriteLine(c.ToCondensedString);
                }

                writer.WriteLine("TABLE:Building");
                foreach (DataRow buildingData in SQL.Query.Select("*", "Building", "1=1").Rows)
                {
                    Building b = new Building(buildingData);
                    writer.WriteLine(b.ToCondensedString);
                }

                writer.WriteLine("TABLE:Contact");
                foreach (DataRow contactData in SQL.Query.Select("*", "Contact", "1=1").Rows)
                {
                    Contact c = new Contact(contactData);
                    writer.WriteLine(c.ToCondensedString);
                }

                writer.WriteLine("TABLE:Company_Contact_Relations");
                foreach (DataRow r in SQL.Query.Select("*", "Company_Contact_Relations", "1=1").Rows)
	            {
		            writer.WriteLine(r["Company_ID"].ToString() + "|" + r["Contact_ID"].ToString());
	            }

                writer.WriteLine("TABLE:Building_Contact_Relations");
                foreach (DataRow r in SQL.Query.Select("*", "Building_Contact_Relations", "1=1").Rows)
	            {
		            writer.WriteLine(r["Building_ID"].ToString() + "|" + r["Contact_ID"].ToString());
	            }

                writer.WriteLine("TABLE:Elevator");
                foreach (DataRow elevatorData in SQL.Query.Select("*", "Elevator", "1=1").Rows)
                {
                    Elevator e = new Elevator(elevatorData);
                    writer.WriteLine(e.ToCondensedString);
                }

                writer.WriteLine("TABLE:Inspection");
                foreach (DataRow inspectionData in SQL.Query.Select("*", "Inspection", "1=1").Rows)
                {
                    Inspection i = new Inspection(inspectionData);
                    writer.WriteLine(i.ToCondensedString);
                }
            }

            
        }
    }
}