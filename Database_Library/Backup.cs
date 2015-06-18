namespace Database_Library
{
    using System;
    using System.Data;
    using System.IO;

    public static class Backup
    {
        public static void Create(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    // Get a list of the tables on the server
                    foreach (DataRow TableName in SQL.Query.Select("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'").Rows)
                    {
                        bool firstrow = true;
                        writer.WriteLine();
                        writer.WriteLine(TableName.ItemArray[0].ToString());
                        // Roll through each of the tables
                        foreach (DataRow TableRow in SQL.Query.Select("SELECT * FROM " + TableName.ItemArray[0].ToString()).Rows)
                        {
                            if (firstrow)
                            {
                                // Roll through each column in the table first to set the schema
                                foreach (DataColumn col in TableRow.Table.Columns)
                                {
                                    writer.Write(string.Format("{0}[{1}]|", col.ColumnName, col.DataType.Name));
                                }
                                writer.WriteLine();
                                firstrow = false;
                            }

                            foreach (object datacol in TableRow.ItemArray)
                            {
                                writer.Write(datacol.ToString() + "|");
                            }
                            writer.WriteLine();
                        }
                    }
                }
            }
        }
    }
}
