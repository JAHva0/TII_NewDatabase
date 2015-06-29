// <summary> Methods for creating and loading backup files. </summary>
namespace Database_Library
{
    using System;
    using System.Data;
    using System.IO;

    /// <summary>
    /// Static class for creating and loading backup files. 
    /// </summary>
    public static class Backup
    {
        /// <summary>
        /// Gets a file name generated from the current date and time.
        /// </summary>
        /// <value> A string filename. </value>
        public static string GenerateFileName
        {
            get
            {
                return string.Format(
                    "{0}{1}{2}-{3}-DB_Backup.tiibackup",
                    DateTime.Now.Year.ToString(),
                    DateTime.Now.Month.ToString().PadLeft(2, '0'),
                    DateTime.Now.Day.ToString().PadLeft(2, '0'),
                    DateTime.Now.Hour.ToString().PadLeft(2, '0'));
            }
        }

        /// <summary>
        /// Creates a backup with a Generated filename.
        /// </summary>
        public static void Create()
        {
            Create(GenerateFileName);
        }

        /// <summary>
        /// Creates a backup with a given filename.
        /// </summary>
        /// <param name="filename">The name and path of the file to create. Must not already exist.</param>
        public static void Create(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    // Get a list of the tables on the server
                    foreach (DataRow tableName in SQL.Query.Select("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'").Rows)
                    {
                        bool firstrow = true;
                        writer.WriteLine(tableName.ItemArray[0].ToString());

                        // Roll through each of the tables
                        foreach (DataRow tableRow in SQL.Query.Select("SELECT * FROM " + tableName.ItemArray[0].ToString()).Rows)
                        {
                            if (firstrow)
                            {
                                // Roll through the column schema for this table
                                foreach (
                                    DataRow col in SQL.Query.Select(string.Format("SELECT * FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = '{0}'", tableName.ItemArray[0].ToString())).Rows)
                                {
                                    writer.Write(GetColumnSchema(col) + "|");
                                }

                                writer.WriteLine();
                                firstrow = false;
                            }

                            // Write the data out, separated by pipes
                            foreach (object datacol in tableRow.ItemArray)
                            {
                                writer.Write(datacol.ToString() + "|");
                            }

                            writer.WriteLine();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generates a string based off of colum schema loaded from the server. 
        /// </summary>
        /// <param name="columnInfo">A <see cref="DataRow"/> from the Column Schema table.</param>
        /// <returns>A string indicating the column Schema.</returns>
        private static string GetColumnSchema(DataRow columnInfo)
        {
            string schema = columnInfo["COLUMN_NAME"].ToString() + "[" + columnInfo["DATA_TYPE"].ToString();

            if (columnInfo["DATA_TYPE"].ToString().ToString() == "varchar")
            {
                schema += string.Format("({0})", columnInfo["CHARACTER_MAXIMUM_LENGTH"].ToString());
            }
            else if (columnInfo["DATA_TYPE"].ToString().ToString() == "decimal")
            {
                schema += string.Format("({0},{1})", columnInfo["NUMERIC_PRECISION"].ToString(), columnInfo["NUMERIC_SCALE"].ToString());
            }

            if (columnInfo["IS_NULLABLE"].ToString() == "NO")
            {
                schema += "|NOT NULL";
            }

            schema += "]";

            return schema;
        }

        ////public static void Load(string filename)
        ////{
        ////    if (!File.Exists(filename))
        ////    {
        ////        MessageBox.Show(string.Format("Backup File {0} does not exist.", filename));
        ////        return;
        ////    }

        ////    if (MessageBox.Show("Loading a backup file will overwrite any changes that have been made since the backup was made. Continue?", "Are You Sure?", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
        ////    {

        ////    }
        ////}
    }
}
