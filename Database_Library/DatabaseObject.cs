// <summary> The Base Object for all items retrieved from the database.</summary>
namespace Database
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;
    using System.Windows.Forms;

    /// <summary>
    /// Event Handler for Database Objects. Intended to fire any time there is a change made to the class before any SQL Operations. 
    /// </summary>
    /// <param name="sender">The class which is sending the edit, from which we can get the table name.</param>
    /// <param name="column_name">The column the edit is occurring in.</param>
    /// <param name="old_value">The original value of the cell.</param>
    /// <param name="new_value">The new value of the cell.</param>
    public delegate void IsEditedEventHandler(object sender, string column_name, string old_value, string new_value);

    /// <summary> The base class for all objects pulled from the database.</summary>
    public class BaseObject
    {
        /// <summary> A list of edits added as required by the Edited event.</summary>
        private List<DBEdit> edits = new List<DBEdit>();
       
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseObject" /> class.
        /// Provides a jumping off point for all Database related classes, including events and variables common to all of them. 
        /// </summary>
        public BaseObject()
        {
            this.Edited += this.BaseObject_Edited;
        }

        /// <summary> 
        /// The Event for determining if an object has changed since it was loaded from the Database.
        /// Add and Remove keywords are not strictly required, but adding them cleanly suppresses a warning. 
        /// </summary>
        //// Taken from http://blogs.msdn.com/b/trevor/archive/2008/08/14/c-warning-cs0067-the-event-event-is-never-used.aspx
        public event IsEditedEventHandler Edited
        {
            add 
            { 
                // throw new NotSupportedException(); 
            }

            remove 
            { 
            }
        }

        /// <summary>
        /// Gets or sets the value of the id object. Since it is assigned by the Database, there is no reason to ever change it outside of the SQL database.
        /// The id can be set only within this class and any derived classes, and should never be modified outside of the initial SQL retrieve. 
        /// By extension, it's also possible to determine if the object is new or not by whether or not it has been assigned an id. "null" id's will
        /// be items which need to be inserted into the database, whereas those with will just be updates. 
        /// </summary>
        /// <value>The ID for this object, assigned by the SQL Server.</value>
        public int? ID
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets a value indicating whether or not the class has been edited.
        /// </summary>
        /// <value>True, if the edit list contains at least one value.</value>
        public bool IsEdited
        {
            get
            {
                return this.edits.Count > 0;
            }
        }

        /// <summary> Gets a list of strings as a way of exposing the edit list. </summary>
        /// <value> A list of strings in the format of "Name: Old Value -> New Value".</value>
        public List<string> Edits
        {
            get
            {
                List<string> editList = new List<string>();
                foreach (DBEdit e in this.edits)
                {
                    editList.Add(string.Format("{0}: {1} -> {2}", e.Column_Name, e.Old_Value, e.New_Value));
                }

                return editList;
            }
        }

        /// <summary>
        /// Retrieves the string description of an Enumerator. 
        /// </summary>
        /// <param name="value">The Enumerator to check.</param>
        /// <returns>The string description of the given enumerator.</returns>
        //// Shamelessly copied from http://blog.spontaneouspublicity.com/associating-strings-with-enums-in-c.
        public static string GetEnumDescription(Enum value)
        {
            System.Reflection.FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
        }

        /// <summary>
        /// Virtual method that is called after any updates or inserts to add a list of any changes that were logged to the DBEdits table.
        /// </summary>
        /// <returns>True, if all of the edits are committed successfully.</returns>
        public virtual bool CommitToDatabase()
        {
            // success will be true if every Insert query completes correctly.
            bool success = true;
            foreach (DBEdit edit in this.edits)
            {
                SQLColumn[] value_pairs = new SQLColumn[] 
                                                          { 
                                                              new SQLColumn("TableName", edit.Table_Name),
                                                              new SQLColumn("Item_ID", edit.Item_Id),
                                                              new SQLColumn("ColumnName", edit.Column_Name),
                                                              new SQLColumn("TimeStamp", DateTime.Now),
                                                              new SQLColumn("OldValue", edit.Old_Value), 
                                                              new SQLColumn("NewValue", edit.New_Value),
                                                              new SQLColumn("UserName", SQL.Connection.GetUser)
                                                          };

                //success = success && SQL.Query.Insert("DBEdits", value_pairs);
            }

            return success;
        }

        /// <summary>
        /// Shared private function to determine that the Server has only returned a single row, and will error if it does not.
        /// </summary>
        /// <param name="tbl"> A data table from the server to be checked.</param>
        /// <returns> The single row from the data table. </returns>
        protected static DataRow AffirmOneRow(DataTable tbl)
        {
            if (tbl.Rows.Count != 1)
            {
                throw new ArgumentException("Datatable must contain exactly one row");
            }

            return tbl.Rows[0];
        }

        /// <summary>
        /// Creates a Save Confirmation Dialog with the given title and edits listed in the body.
        /// </summary>
        /// <param name="title">The string to be displayed in the title bar after "Save the Following Changes to".</param>
        /// <returns>True if the user selected OK, false otherwise of if there were no edits in the list passed.</returns>
        protected virtual bool SaveConfirmation(string title)
        {
            // If no changes were made, don't bother confirming.
            if (this.Edits.Count == 0)
            {
                return false;
            }

            string editList = string.Empty;
            foreach (string e in this.Edits)
            {
                editList += e + Environment.NewLine;
            }

            if (MessageBox.Show(editList, string.Format("Save the Following Changes to {0}?", title), MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.OK)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Method that is used by <see cref="IsEditedEventHandler"/> to flag via <see cref="edited"/> that a particular item has been 
        /// edited since loading from the Database.
        /// </summary>
        /// <param name="sender">The class which is sending the edit, from which we can get the table name.</param>
        /// <param name="column_name">The column the edit is occurring in.</param>
        /// <param name="old_value">The original value of the cell.</param>
        /// <param name="new_value">The new value of the cell.</param>
        protected void BaseObject_Edited(object sender, string column_name, string old_value, string new_value)
        {
            // Check to make sure this particular value hasn't been changed previously
            if (this.edits.Exists(x => x.Column_Name == column_name))
            {
                // This column name appears in edits already, so we are just going to change the new value rather than make a new DBEdit entry
                DBEdit toChange = new DBEdit();
                foreach (DBEdit e in this.edits)
                {
                    if (e.Column_Name == column_name)
                    {
                        toChange = e;
                    }
                }

                // Remove the edit with that column name
                this.edits.Remove(toChange);

                // Modify the new value to reflect what we have just changed it to.
                toChange.New_Value = new_value;

                // Add the modified edit back into the list.
                this.edits.Add(toChange);
            }
            else
            {
                // Add a new edit to the edit array for use during a commit.
                this.edits.Add(new DBEdit(sender.GetType().Name, this.ID, column_name, old_value, new_value));
            }
        }

        /// <summary>
        /// Method that is used by <see cref="IsEditedEventHandler"/> to flag via <see cref="edited"/> that a particular item has been 
        /// edited since loading from the Database.
        /// </summary>
        /// <param name="sender">The class which is sending the edit, from which we can get the table name.</param>
        /// <param name="column_name">The column the edit is occurring in.</param>
        /// <param name="old_date">The original Date Time value of the cell.</param>
        /// <param name="new_date">The new Date Time value of the cell.</param>
        protected void BaseObject_Edited(object sender, string column_name, DateTime old_date, DateTime new_date)
        {
            string old_value;

            if (old_date < new DateTime(2000, 1, 1))
            {
                old_value = string.Empty;
            }
            else
            {
                old_value = old_date.ToString();
            }

            this.BaseObject_Edited(sender, column_name, old_value, new_date.ToString());
        }

        /// <summary>
        /// Method that is used by <see cref="IsEditedEventHandler"/> to flag via <see cref="edited"/> that a particular item has been 
        /// edited since loading from the Database.
        /// </summary>
        /// <param name="sender">The class which is sending the edit, from which we can get the table name.</param>
        /// <param name="column_name">The column the edit is occurring in.</param>
        /// <param name="old_int">The original integer value of the cell.</param>
        /// <param name="new_int">The new integer value of the cell.</param>
        protected void BaseObject_Edited(object sender, string column_name, int old_int, int new_int)
        {
            string old_value;

            if (old_int == 0)
            {
                old_value = string.Empty;
            }
            else
            {
                old_value = old_int.ToString();
            }

            this.BaseObject_Edited(sender, column_name, old_value, new_int.ToString());
        }

        /// <summary> Private non-inherited struct to log edits made to the database. </summary>
        private struct DBEdit
        {
            /// <summary> The name of the table being edited.</summary>
            public string Table_Name;

            /// <summary> The id of the item within the table. Provided within this class.</summary>
            public int? Item_Id;

            /// <summary> The name of the column the edit is occurring in.</summary>
            public string Column_Name;

            /// <summary> The old value.</summary>
            public string Old_Value;

            /// <summary> The new value.</summary>
            public string New_Value;

            /// <summary>
            /// Initializes a new instance of the <see cref="DBEdit"/> struct.
            /// </summary>
            /// <param name="table_name">The table the edit is occurring to.</param>
            /// <param name="item_id">The id of the item within the table. Provided within this class.</param>
            /// <param name="column_name">The column the edit is occurring in.</param>
            /// <param name="old_value">The original value of the cell.</param>
            /// <param name="new_value">The new value of the cell.</param>
            public DBEdit(string table_name, int? item_id, string column_name, string old_value, string new_value)
            {
                this.Table_Name = table_name;
                this.Item_Id = item_id;
                this.Column_Name = column_name;
                this.Old_Value = old_value;
                this.New_Value = new_value;
            }
        }
    }
}
