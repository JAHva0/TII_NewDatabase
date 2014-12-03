// <summary> Provides methods reltaed to the backing up and restoring of the database. </summary>

namespace TII_NewDatabase.HelperClasses
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    
    /// <summary>
    /// Static class that holds methods for creating and reading backup files.
    /// </summary>
    public static class DatabaseBackup
    {
        /// <summary>
        /// Creates a backup file in a custom format within the provided folder.
        /// </summary>
        /// <param name="filepath">The folder to save this backup file in.</param>
        public static void CreateBackup(string filepath)
        {
            // Create a filename based on the date and hour, i.e. "141202-16-DB_Backup.tiibackup"
            string filename = filepath + @"\" + string.Format("{0}-{1}-DB_Backup.tiibackup", DateTime.Now.ToYYMMDDString(), DateTime.Now.Hour.ToString().PadLeft(2, '0'));
            
            // Make sure we aren't trying to write to a report file that already exists
            if (File.Exists(filename))
            {
                throw new Exception("Specified report File already exists:\n" + filepath + filename);
            }

            // Start a stopwatch so that we know how long the whole thing took.
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Get all of the information needed and save it to a temporary file.
            using (StreamWriter writer = new StreamWriter(filename + "_temp"))
            {
                try 
                {
                    // Get the names of every table in the database
                    foreach (DataRow tablename in SQL.Query.Select("SELECT name FROM sys.tables").Rows)
                    {
                        writer.WriteLine("$" + tablename["name"]);
                        bool columnsSaved = false;
                        
                        // Select every value that exists in the table
                        DataTable tbl = SQL.Query.Select(string.Format("SELECT * FROM {0}", tablename["name"]));
                        foreach (DataRow row in tbl.Rows)
                        {
                            // Before we start adding information, we want to know the colum names
                            if (!columnsSaved)
                            {
                                string key_col = string.Empty;

                                // Load the Column information
                                foreach (DataRow col in SQL.Query.Select(SQLColumnInformation.QueryString(tablename["name"].ToString())).Rows)
                                {
                                    SQLColumnInformation columnInfo = new SQLColumnInformation(col);
                                    writer.Write(columnInfo.ToSQLString() + "|");

                                    if (columnInfo.IsIdentity)
                                    {
                                        key_col = columnInfo.Name;
                                    }
                                }

                                writer.Write(string.Format("PRIMARY KEY({0})", key_col));

                                writer.Write(writer.NewLine);
                                columnsSaved = true;
                            }

                            for (int i = 0; i < row.ItemArray.Length; i++)
                            {
                                if (i != 0)
                                {
                                    writer.Write("|");
                                }

                                writer.Write(row.ItemArray[i].ToString().Trim());
                            }

                            writer.Write(writer.NewLine);
                        }
                    }
                }
                catch (Exception ex)
                {
                    writer.Write(writer.NewLine + writer.NewLine);
                    writer.Write("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" + writer.NewLine);
                    writer.WriteLine("Error occured during backup operation.  Details:" + writer.NewLine);
                    writer.Write(ex);
                }
            }

            sw.Stop();

            // Write the actual file, prepending the backup data to the start of the file
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine(string.Format("##Backup created at {0} - Completed in {1}ms.", DateTime.Now, sw.ElapsedMilliseconds));
                writer.Write(File.ReadAllText(filename + "_temp"));
            }

            File.Delete(filename + "_temp");
        }

        /// <summary>
        /// Creates a struct that parses and holds information regarding a table's column.
        /// </summary>
        private struct SQLColumnInformation
        {
            /// <summary> The Column name. </summary>
            private string name;

            /// <summary> The value type of the column. </summary>
            private string type;

            /// <summary> The maximum length of the value. </summary>
            private int maxlength;

            /// <summary> True if this column is the Primary Key.</summary>
            private bool identity;

            /// <summary> True if this column can be null. </summary>
            private bool nullable;

            /// <summary>
            /// Initializes a new instance of the <see cref="SQLColumnInformation"/> struct.
            /// </summary>
            /// <param name="columnInformation"> A properly formed data row with the column information present.</param>
            public SQLColumnInformation(DataRow columnInformation)
            {
                this.name = columnInformation["name"].ToString().ToUpper();
                this.type = columnInformation["coltype"].ToString().ToUpper();
                this.maxlength = Convert.ToInt32(columnInformation["max_length"].ToString());
                this.identity = Convert.ToBoolean(columnInformation["is_identity"].ToString());
                this.nullable = Convert.ToBoolean(columnInformation["is_nullable"].ToString());
            }

            /// <summary>
            /// Gets the name of this column.
            /// </summary>
            public string Name
            {
                get
                {
                    return this.name;
                }
            }

            /// <summary>
            /// Gets a value indicating whether or not this column is a primary key.
            /// </summary>
            public bool IsIdentity
            {
                get
                {
                    return this.identity;
                }
            }

            /// <summary>
            /// Returns the SQL Query used to obtain the information for the SQL Table Column in the proper format.
            /// </summary>
            /// <param name="tableName"> The name of the table to select for. </param>
            /// <returns>A properly formatted SQL Query. </returns>
            public static string QueryString(string tableName)
            {
                return string.Format(
                    "SELECT cols.name, types.name as coltype, cols.max_length, cols.is_identity, cols.is_nullable FROM sys.tables AS tables " +
                    "JOIN sys.columns AS cols ON tables.object_id = cols.object_id " +
                    "JOIN sys.types AS types ON cols.user_type_id = types.user_type_id " +
                    "WHERE tables.name = '{0}'", 
                    tableName);
            }

            /// <summary>
            /// Returns the string value of this struct, formatted as would be used in an SQL Create Table Query.
            /// </summary>
            /// <returns>An SQL compatible string representing this column.</returns>
            public string ToSQLString()
            {
                string return_string = string.Format("{0} {1}({2})", this.name, this.type, this.maxlength.ToString());
                if (!this.nullable)
                {
                    return_string += " NOT NULL";
                }

                return return_string;
            }
        }
    }
}
