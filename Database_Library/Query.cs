//-------------------------------------------
// <summary>Parses and Handles SQL Queries</summary>
//------------------------------------------

namespace SQL
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using Database;

    /// <summary>
    /// Static Class that handles Query and Insert/Update statements to the Server.
    /// </summary>
    public static class Query
    {        
        /// <summary> Keeps track of the number of SQL Queries that are made. </summary>
        private static int call_count = 0;

        /// <summary> Maintains a collection of the server statistics from the last action. </summary>
        private static ConnectionStatistics lastConnection;

        /// <summary> Maintains a collection of the server statistics since the connection was started. </summary>
        private static ConnectionStatistics allConnections = new ConnectionStatistics();

        /// <summary>
        /// Event intended to fire any time there is an update or insert Query, which results in a modification of the database.
        /// Should be used to determine any time a specific table is updated in some way - e.g. A new company has been added, 
        /// so the List Box that contains all company names should be re-generated.
        /// </summary>
        public static event EventHandler<QueryEvent> DatabaseModified;

        /// <summary> Used by the <see cref="DatabaseModified"/> EventHandler to determine the type of Query committed to the database.</summary>
        public enum QueryEvent
        {
            /// <summary> Default. No Query Type Detected.</summary>
            None,

            /// <summary> Update Query. </summary>
            UPDATE,

            /// <summary> Insert Query. </summary>
            INSERT
        }

        /// <summary>
        /// Gets the current value of the Call Counter.
        /// </summary>
        /// <value>The number of SQL calls that have been made.</value>
        public static int Call_Counter 
        { 
            get { return call_count; } 
        }

        /// <summary> Gets the server statistics from the last action. </summary>
        /// <value>The Server statistics from the last action.</value>
        public static ConnectionStatistics LastConnectionStats
        {
            get
            {
                return LastConnectionStats;
            }
        }

        /// <summary> Gets or sets the server statistics since the connection was started. </summary>
        /// <value>The server statistics since the connection was started.</value>
        public static ConnectionStatistics AllConnectionStats
        {
            get
            {
                return allConnections;
            }

            set
            {
                allConnections = value;
            }
        }

        /// <summary>
        /// Creates a Select Statement using a given query.
        /// </summary>
        /// <param name="query">The query to send to the database.</param>
        /// <returns>A DataTable containing data as returned by the query.</returns>
        public static DataTable Select(string query)
        {
            return GetTable(query);
        }

        /// <summary>
        /// Creates a more safe query by restricting the inputs.
        /// </summary>
        /// <param name="column">The column to select.</param>
        /// <param name="table_name">The table to select from.</param>
        /// <param name="where_clause">A "Where" clause to restrict the data that is returned.</param>
        /// <returns>A DataTable containing data as returned by the query.</returns>
        public static DataTable Select(string column, string table_name, string where_clause)
        {
            return GetTable(string.Format("SELECT {0} FROM {1} WHERE {2}", column, table_name, where_clause));
        }

        /// <summary>
        /// Creates a safe Insert Query for any number of value pairs.
        /// </summary>
        /// <param name="table_name">The name of the table to be affected.</param>
        /// <param name="value_pairs">The set of columns and values to be included.</param>
        /// <returns>True, as long as the query was inserted correctly.</returns>
        public static bool Insert(string table_name, SQLColumn[] value_pairs)
        {
            string query = "INSERT INTO " + table_name + "(";
            foreach (SQLColumn column in value_pairs)
            {
                query += column.Column + ", ";
            }

            query = query.TrimFromEnd(2);
            query += ") VALUES (";
            foreach (SQLColumn value in value_pairs)
            {
                query += value.Value + ", ";
            }

            query = query.TrimFromEnd(2);
            query += ")";
            return NonQuery(query);
        }

        /// <summary>
        /// Creates a safe Update Query for any number of value pairs.
        /// </summary>
        /// <param name="table_name">The name of the table to be affected.</param>
        /// <param name="value_pairs">The set of columns and values to be included.</param>
        /// <param name="where_clause">The statement that specifies which records are to be updated.</param>
        /// <returns>True, as long as the query was updated correctly.</returns>
        public static bool Update(string table_name, SQLColumn[] value_pairs, string where_clause)
        {
            string query = "UPDATE " + table_name + " SET ";
            foreach (SQLColumn pair in value_pairs)
            {
                query += pair.Column + "='" + pair.Value + "', ";
            }

            query = query.TrimFromEnd(2);
            query += " WHERE " + where_clause;
            return NonQuery(query);
        }

        /// <summary>
        /// Creates a Delete Query for a table. 
        /// For safety, we aren't allowing deletes outside of the relations tables.
        /// </summary>
        /// <param name="table_name">The name of the table to delete from.</param>
        /// <param name="where_clause">The statement specifying the restrictions on the delete.</param>
        /// <returns>True, if the query was submitted successfully.</returns>
        public static bool Delete(string table_name, string where_clause)
        {
            Debug.Assert(table_name.Contains("_Relations"), "No Delete queries on anything outside of the contact relation tables");
            string query = "DELETE FROM " + table_name + " WHERE " + where_clause;
            return NonQuery(query);
        }

        /// <summary>
        /// An Extension method for strings that trims the specified number of characters from the end.
        /// </summary>
        /// <param name="str">The string to be trimmed.</param>
        /// <param name="count">The number of characters to trim.</param>
        /// <returns>An appropriately trimmed string.</returns>
        private static string TrimFromEnd(this string str, int count)
        {
            return str.Remove(str.Length - count);
        }

        /// <summary>
        /// Submits a Query to the Server.
        /// </summary>
        /// <param name="query">The SQL query to be resolved.</param>
        /// <returns>A Data table containing the requested data.</returns>
        private static DataTable GetTable(string query)
        {
            try
            {
                call_count++;
                DataTable tbl = new DataTable();
                SqlCommand select_command = new SqlCommand(query, Connection.GetConnection); // Represents a Transact SQL Statement
                SqlDataAdapter data_adapter = new SqlDataAdapter(select_command); // Holds the Command and Connection used to fill a dataset
                Connection.GetConnection.Open();
                data_adapter.Fill(tbl); // Fills the table with the query results

                // Get the stats from the last connection and store them in this class.
                lastConnection = new ConnectionStatistics(Connection.GetConnection.RetrieveStatistics());
                allConnections.ConcatStatistics(lastConnection);
                Connection.GetConnection.ResetStatistics();
                Connection.GetConnection.Close();
                return tbl;
            }
            catch (InvalidOperationException ex)
            {
                // Cannot open a connection without specifying a data source or server.
                // or 
                // The connection is already open.
                throw ex;
            }
            catch (SqlException ex)
            {
                // A connection-level error occoured while opening the connection. If Number contains 18487 or 18488, the password expired or needs to be reset.
                throw new Exception("An error occured in the following SQL Statement: \n" + query + "\nDetails: " + ex.Message, ex);
            }
            catch (System.Configuration.ConfigurationException ex)
            {
                // There are two entries with the same name in the <localdbinstances> section.
                throw ex;
            }
            finally
            {
                if (Connection.GetConnection.State != ConnectionState.Closed)
                {
                    Connection.GetConnection.Close();
                }
            }
        }

        /// <summary>
        /// Submits a Non-Query (Insert or Update) to the SQL Server.
        /// </summary>
        /// <param name="query">The SQL query to be resolved.</param>
        /// <returns>True or False, depending on if the Query was successful or not.</returns>
        private static bool NonQuery(string query)
        {
            try
            {
                call_count++;
                SqlCommand nonquery_command = new SqlCommand(query, Connection.GetConnection);
                Connection.GetConnection.Open();
                if (System.Windows.Forms.MessageBox.Show(query, "Insert Query", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    nonquery_command.ExecuteNonQuery();
                }

                Connection.GetConnection.Close();

                // Check if the Database Modifier has been assigned
                if (DatabaseModified != null)
                {
                    string sender = string.Empty;
                    QueryEvent e = QueryEvent.None;

                    // Pull out the Query type and table affected
                    if (query.Contains("UPDATE"))
                    {
                        // Pulls out the table name from an update Query: "UPDATE sometable SET something=X"
                        sender = query.Substring(7, query.IndexOf("SET") - 8);
                        e = QueryEvent.UPDATE;
                    }

                    if (query.Contains("INSERT"))
                    {
                        // Pulls out the table name from an insert Query: "INSERT INTO sometable VALUES x, y, z"
                        sender = query.Substring(12, query.IndexOf("VALUES") - 13);
                        e = QueryEvent.INSERT;
                    }

                    DatabaseModified(sender, e);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Add Handling and Debugging for Exceptions as they arise
                throw ex;
            }
        }
    }
}