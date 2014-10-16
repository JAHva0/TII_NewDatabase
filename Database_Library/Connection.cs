//-------------------------------------------
// <summary>SQL related methods and functions.</summary>
//------------------------------------------

namespace SQL
{
    using System;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Security;

    /// <summary>
    /// Methods to facilitate connecting to a SQL Server.
    /// </summary>
    public static class Connection
    {
        /// <summary>
        /// Connection information for the SQL Server, holding the connection string and credentials.
        /// </summary>
        private static SqlConnection server_connection;

        /// <summary>
        /// Gets a value indicating whether there is an open connection with the server, provided there is a connection set up.
        /// </summary>
        /// <value>True, if there is a valid connection.</value>
        public static bool ConnectionUp
        {
            get
            {
                try
                {
                    server_connection.Open();
                    server_connection.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    // The Server is either disabled, or the network path is incorrect.
                    if (ex.Message.Contains("The server was not found"))
                    {
                        Debug.WriteLine("Unable to locate server at " + server_connection.DataSource);
                        return false;
                    }

                    // If the User or Password is incorrect
                    if (ex.Message.Contains("Login failed for user"))
                    {
                        Debug.WriteLine("Unable to Log In. Check User/Password");
                        return false;
                    }

                    // Catchall for other errors - implement as they arise
                    throw new Exception("SQL Server Connection Error: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Gets a copy of the server connection, provided the connection is still up and valid.
        /// </summary>
        /// <value>A valid SQL Connection if one exists, an empty SQL Connection if not.</value>
        public static SqlConnection GetConnection
        {
            get { return server_connection; }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateConnection"/> class. Use <see cref="CheckConnection"/> to confirm that the credentials are valid.
        /// </summary>
        /// <param name="user_name">The Login User Name.</param>
        /// <param name="password">The Login Password.</param>
        /// <param name="server_address">The IP Address of the server. Default is Local Host.</param>
        /// <param name="database_name">The Name of the Database to connect to. Default is TIIDatabase.</param>
        public static void CreateConnection(string user_name, string password, string server_address = "localhost", string database_name = "TIIDatabase")
        {
            // Create a new connection to store in this instance
            server_connection = new SqlConnection();
            server_connection.ConnectionString = string.Format("server={0};database={1};connection timeout=1", server_address, database_name);
            server_connection.Credential = CreateCredentials(user_name, password);
            server_connection.StatisticsEnabled = true;
            Query.AllConnectionStats = new ConnectionStatistics(server_connection.RetrieveStatistics());
        }

        /// <summary>
        /// Create SQL Credentials for use in connecting to a server.
        /// </summary>
        /// <param name="user_name">The Login User Name.</param>
        /// <param name="password">The Login Password.</param>
        /// <returns>Valid Secure SQLCredentials.</returns>
        private static SqlCredential CreateCredentials(string user_name, string password)
        {
            // Create a new SecureString by appending each char from the given password
            SecureString secure_password = new SecureString();
            foreach (char c in password)
            {
                secure_password.AppendChar(c);
            }

            secure_password.MakeReadOnly();
            return new SqlCredential(user_name, secure_password);
        }
    }
}
