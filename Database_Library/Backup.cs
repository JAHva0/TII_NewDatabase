namespace Database_Library
{
    using System;
    using System.Data;
    using System.IO;
    using System.Windows.Forms;

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
                        writer.WriteLine(TableName.ItemArray[0].ToString());
                        // Roll through each of the tables
                        foreach (DataRow TableRow in SQL.Query.Select("SELECT * FROM " + TableName.ItemArray[0].ToString()).Rows)
                        {
                            if (firstrow)
                            {
                                // Roll through the column schema for this table
                                foreach (
                                    DataRow col in SQL.Query.Select(string.Format("SELECT * FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = '{0}'", TableName.ItemArray[0].ToString())).Rows)
                                {
                                    writer.Write(GetColumnSchema(col) + "|");
                                }
                                writer.WriteLine();
                                firstrow = false;
                            }

                            // Write the data out, separated by pipes
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

        public static void Load(string filename)
        {
            if (!File.Exists(filename))
            {
                MessageBox.Show(string.Format("Backup File {0} does not exist.", filename));
                return;
            }

            if (MessageBox.Show("Loading a backup file will overwrite any changes that have been made since the backup was made. Continue?", "Are You Sure?", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
            {

            }
        }
    }
}
