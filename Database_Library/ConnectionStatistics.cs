// <summary> Structure for maintaining statistics related to the operation of the SQL server. </summary>

namespace SQL
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    
    /// <summary>
    /// Structure for holding connection statistics.
    /// </summary>
    public struct ConnectionStatistics
    {
        /// <summary>
        /// Holds the connection statistics as passed in by Connection.GetStatistics().
        /// </summary>
        private Dictionary<string, long> stats;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionStatistics"/> structure.
        /// </summary>
        /// <param name="stats">A collection of statistics provided by Connection.GetStatistics().</param>
        public ConnectionStatistics(IDictionary stats)
        {
            this.stats = new Dictionary<string, long>();
            this.stats.Add("BuffersReceived", (long)stats["BuffersReceived"]);
            this.stats.Add("BuffersSent", (long)stats["BuffersSent"]);
            this.stats.Add("BytesReceived", (long)stats["BytesReceived"]);
            this.stats.Add("BytesSent", (long)stats["BytesSent"]);
            this.stats.Add("ConnectionTime", (long)stats["ConnectionTime"]);
            this.stats.Add("CursorOpens", (long)stats["CursorOpens"]);
            this.stats.Add("ExecutionTime", (long)stats["ExecutionTime"]);
            this.stats.Add("IduCount", (long)stats["IduCount"]);
            this.stats.Add("IduRows", (long)stats["IduRows"]);
            this.stats.Add("NetworkServerTime", (long)stats["NetworkServerTime"]);
            this.stats.Add("PreparedExecs", (long)stats["PreparedExecs"]);
            this.stats.Add("Prepares", (long)stats["Prepares"]);
            this.stats.Add("SelectCount", (long)stats["SelectCount"]);
            this.stats.Add("SelectRows", (long)stats["SelectRows"]);
            this.stats.Add("ServerRoundtrips", (long)stats["ServerRoundtrips"]);
            this.stats.Add("SumResultSets", (long)stats["SumResultSets"]);
            this.stats.Add("Transactions", (long)stats["Transactions"]);
            this.stats.Add("UnpreparedExecs", (long)stats["UnpreparedExecs"]);
        }

        /// <summary>Gets the Number of tabular data stream packets received by the provider.</summary>
        /// <value>The Number of tabular data stream packets received by the provider.</value>
        public long BuffersRecieved
        {
            get
            {
                return this.stats["BuffersReceived"];
            }
        }

        /// <summary>Gets the Number of tabular data stream packets sent to the provider.</summary>
        /// <value>The Number of tabular data stream packets sent to the provider.</value>
        public long BuffersSent
        {
            get
            {
                return this.stats["BuffersSent"];
            }
        }

        /// <summary> Gets the number of bytes of data received from the provider. </summary>
        /// <value> The bytes of data received from the provider.</value>
        public long BytesReceived
        {
            get
            {
                return this.stats["BytesReceived"];
            }
        }

        /// <summary> Gets the number of bytes of data sent to the provider. </summary>
        /// <value> The bytes of data sent to the provider.</value>
        public long BytesSent
        {
            get
            {
                return this.stats["BytesSent"];
            }
        }

        /// <summary> Gets the amount of time in milliseconds that the connection has been opened.</summary>
        /// <value> The amount of time in milliseconds the connection has been open.</value>
        public long ConnectionTime
        {
            get
            {
                return this.stats["ConnectionTime"];
            }
        }

        /// <summary>Gets the number of times a cursor was open through the connection once the application has started using the provider.</summary>
        /// <value>The number of times a cursor was open through the connection.</value>
        public long CursorOpens
        {
            get
            {
                return this.stats["CursorOpens"];
            }
        }

        /// <summary>
        /// Gets the cumulative amount of time the provider has spent processing once statistics have been enabled, 
        /// including the time spent waiting for replies from the server as well as the time spent executing code in the provider itself.
        /// </summary>
        /// <value>The cumulative amount of time the provider has spent processing.</value>
        public long ExecutionTime
        {
            get
            {
                return this.stats["ExecutionTime"];
            }
        }

        /// <summary>Gets the total number of INSERT, DELETE, and UPDATE statements executed through the connection.</summary>
        /// <value>The total number of INSERT, DELETE, and UPDATE statements executed through the connection.</value>
        public long IduCount
        {
            get
            {
                return this.stats["IduCount"];
            }
        }

        /// <summary>Gets the total number of rows affected by INSERT, DELETE, and UPDATE statements executed through the connection.</summary>
        /// <value>Returns the total number of rows affected by INSERT, DELETE, and UPDATE statements executed through the connection.</value>
        public long IduRows
        {
            get
            {
                return this.stats["IduRows"];
            }
        }

        /// <summary>Gets the cumulative amount of time the provider spent waiting for replies from the server.</summary>
        /// <value>Returns the cumulative amount of time the provider spent waiting for replies from the server. </value>   
        public long NetworkServerTime
        {
            get
            {
                return this.stats["NetworkServerTime"];
            }
        }

        /// <summary>Gets the number of prepared commands executed through the connection. </summary>
        /// <value>Returns the number of prepared commands executed through the connection. </value>       
        public long PreparedExecs
        {
            get
            {
                return this.stats["PreparedExecs"];
            }
        }

        /// <summary>Gets the number of statements prepared through the connection.</summary>
        /// <value>Returns the number of statements prepared through the connection.</value>         
        public long Prepares
        {
            get
            {
                return this.stats["Prepares"];
            }
        }

        /// <summary>Gets the number of SELECT statements executed through the connection. </summary>
        /// <value>Returns the number of SELECT statements executed through the connection. </value>              
        public long SelectCount
        {
            get
            {
                return this.stats["SelectCount"];
            }
        }

        /// <summary>
        /// Gets the number of rows selected once the application has started using the provider and has enabled statistics. 
        /// This counter reflects all the rows generated by SQL statements, even those that were not actually consumed by the caller. 
        /// For example, closing a data reader before reading the entire result set would not affect the count. 
        /// This includes the rows retrieved from cursors through FETCH statements.
        /// </summary>
        /// <value>Gets the number of rows selected.</value>                  
        public long SelectRows
        {
            get
            {
                return this.stats["SelectRows"];
            }
        }

        /// <summary>Gets the number of times the connection sent commands to the server and got a reply back.</summary>
        /// <value>Returns the number of times the connection sent commands to the server and got a reply back.</value>                      
        public long ServerRoundTrips
        {
            get
            {
                return this.stats["ServerRoundtrips"];
            }
        }

        /// <summary>Gets the number of result sets that have been used.</summary>
        /// <value>Returns the number of result sets that have been used.</value>                       
        public long SumResultSets
        {
            get
            {
                return this.stats["SumResultSets"];
            }
        }

        /// <summary>Gets the number of user transactions started once the application has started using the provider.</summary>
        /// <value>Returns the number of user transactions started once the application has started using the provider.</value>                            
        public long Transactions
        {
            get
            {
                return this.stats["Transactions"];
            }
        }

        /// <summary>Gets the number of unprepared statements executed through the connection.</summary>
        /// <value>Returns the number of unprepared statements executed through the connection.</value>
        public long UnpreparedExecs
        {
            get
            {
                return this.stats["UnpreparedExecs"];
            }
        }

        /// <summary>
        /// Combines the statistics stored in this structure with those passed in from another.
        /// </summary>
        /// <param name="stats">The statistics to add to this structure.</param>
        public void ConcatStatistics(ConnectionStatistics stats)
        {
            this.stats["BuffersReceived"] += stats.BuffersRecieved;
            this.stats["BuffersSent"] += stats.BuffersSent;
            this.stats["BytesReceived"] += stats.BytesReceived;
            this.stats["BytesSent"] += stats.BytesSent;
            this.stats["ConnectionTime"] += stats.ConnectionTime;
            this.stats["CursorOpens"] += stats.CursorOpens;
            this.stats["ExecutionTime"] += stats.ExecutionTime;
            this.stats["IduCount"] += stats.IduCount;
            this.stats["IduRows"] += stats.IduRows;
            this.stats["NetworkServerTime"] += stats.NetworkServerTime;
            this.stats["PreparedExecs"] += stats.PreparedExecs;
            this.stats["Prepares"] += stats.Prepares;
            this.stats["SelectCount"] += stats.SelectCount;
            this.stats["SelectRows"] += stats.SelectRows;
            this.stats["ServerRoundtrips"] += stats.ServerRoundTrips;
            this.stats["SumResultSets"] += stats.SumResultSets;
            this.stats["Transactions"] += stats.Transactions;
            this.stats["UnpreparedExecs"] += stats.UnpreparedExecs;
        }
    }
}