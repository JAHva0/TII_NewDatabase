// <summary> Exceptions for use specifically by the SQL Namespace. </summary>

namespace SQL
{
    using System;
    using System.Data;

    /// <summary>
    /// Exceptions intended to be thrown when the SQL server returns a duplicate entry, either in the primary key or as a result of a condition.
    /// </summary>
    public class SQLDuplicateEntryException : Exception
    {
        /// <summary> The object which failed in order to cause the error. </summary>
        private object failedObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLDuplicateEntryException"/> class. Create a SQL Duplicate Exception.
        /// </summary>
        public SQLDuplicateEntryException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLDuplicateEntryException"/> class. Create a SQL Duplicate Exception.
        /// </summary>
        /// <param name="message">The message to return with this exception.</param>
        public SQLDuplicateEntryException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLDuplicateEntryException"/> class. Create a SQL Duplicate Exception.
        /// </summary>
        /// <param name="message">The message to return with this exception.</param>
        /// <param name="innerException">Any base exception that is related to this. </param>
        public SQLDuplicateEntryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLDuplicateEntryException"/> class. Create a SQL Duplicate Exception.
        /// </summary>
        /// <param name="message">The message to return with this exception.</param>
        /// <param name="innerException">Any base exception that is related to this. </param>
        /// <param name="failedObject">The object responsible for the failure.</param>
        public SQLDuplicateEntryException(string message, Exception innerException, object failedObject)
            : base(message, innerException)
        {
            this.failedObject = failedObject;
        }

        /// <summary> Gets the value of the object with failed in order to create the error. </summary>
        /// <value>The object which is responsible for the error.</value>
        public object FailedObject
        {
            get
            {
                return this.failedObject;
            }
        }
    }
}
