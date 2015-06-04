// <summary> Variations on the SQLColum struct </summary>

namespace Database
{
    using System;
    using System.Linq;
    
    /// <summary>
    /// Provides a method of sanitizing string inputs before they are sent off to the Database. 
    /// </summary>
    public struct SQLColumn
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SQLColumn"/> struct for use in inserting/updating in the SQL Database.
        /// </summary>
        /// <param name="column">A valid column name.</param>
        /// <param name="value">A string to insert/update.</param>
        public SQLColumn(string column, string value) 
            : this(column)
        {
            if (value == null || value == string.Empty)
            {
                this.Value = "NULL";
            }
            else if (value.Contains("SELECT") && value.Contains("FROM"))
            {
                this.Value = value;
            }
            else
            {
                this.Value = value.Replace("'", "''");
                this.Value = "'" + this.Value + "'";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLColumn"/> struct for use in inserting/updating in the SQL Database.
        /// </summary>
        /// <param name="column">A valid column name.</param>
        /// <param name="value">A boolean to insert/update.</param>
        public SQLColumn(string column, bool value)
            : this(column)
        {
            this.Value = "'" + value.ToString() + "'";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLColumn"/> struct for use in inserting/updating in the SQL Database.
        /// </summary>
        /// <param name="column">A valid column name.</param>
        /// <param name="value">A null-able Integer to insert/update.</param>
        public SQLColumn(string column, int value)
            : this(column)
        {
            if (value == 0)
            {
                this.Value = "NULL";
            }
            else
            {
                this.Value = value.ToString();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLColumn"/> struct for use in inserting/updating in the SQL Database.
        /// </summary>
        /// <param name="column">A valid column name.</param>
        /// <param name="value">A null-able Integer to insert/update.</param>
        public SQLColumn(string column, int? value)
            : this(column)
        {
            if (value == null)
            {
                this.Value = "NULL";
            }
            else
            {
                this.Value = value.ToString();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLColumn"/> struct for use in inserting/updating in the SQL Database.
        /// </summary>
        /// <param name="column">A valid column name.</param>
        /// <param name="value">A valid DateTime? to insert/update (no earlier than Jan 1, 2000).</param>
        public SQLColumn(string column, DateTime? value)
            : this(column)
        {
            // Check if the DateTime is null or invalid, in which case we want to send a null string to the database.
            if (value == null || value.Value < new DateTime(2000, 1, 1))
            {
                this.Value = "NULL";
            }
            else
            {
                this.Value = "'" + value.Value.ToString() + "'";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLColumn"/> struct for use in inserting/updating in the SQL Database.
        /// </summary>
        /// <param name="column">A valid column name.</param>
        /// <param name="value">A Double to insert/update.</param>
        public SQLColumn(string column, double value)
            : this(column)
        {
            if (value == 0)
            {
                this.Value = "NULL";
            }
            else
            {
                this.Value = value.ToString();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLColumn"/> struct for use in inserting/updating in the SQL Database.
        /// </summary>
        /// <param name="column">A valid column name.</param>
        /// <param name="value">A Decimal to insert/update.</param>
        public SQLColumn(string column, decimal value)
            : this(column)
        {
            if (value == 0)
            {
                this.Value = "NULL";
            }
            else
            {
                this.Value = value.ToString();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLColumn"/> struct. 
        /// Private constructor that simply error checks that we have a column name.
        /// </summary>
        /// <param name="column">A valid column name.</param>
        private SQLColumn(string column)
            : this()
        {
            if (column == null)
            {
                throw new ArgumentNullException("SQLColumn", "Column name cannot be null");
            }

            this.Column = column;
        }

        /// <summary>Gets or sets the Column Name. </summary>
        /// <value>The Column Name.</value>
        public string Column { get; set; }

        /// <summary>Gets or sets the Clean Value. </summary>
        /// <value>A sanitized string for use in the SQL Server.</value>
        public string Value { get; set; }
    }
}