// <summary> Creates a form able to insert inspection information into the database. </summary>

namespace TII_NewDatabase.AddNewForms
{
    using System;
    using System.Data;
    using System.Windows.Forms;
    using Database;
    using SQL;

    /// <summary> Form for entering inspection information. </summary>
    public partial class FormAddInspection : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormAddInspection"/> class.
        /// </summary>
        public FormAddInspection()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Method that occurs once as the form loads as it is being displayed to the user.
        /// </summary>
        /// <param name="sender">The Sender.</param>
        /// <param name="e">Any Event Args.</param>
        private void OnLoad(object sender, EventArgs e)
        {
            this.lbx_BuildingList.Items.AddRange(Main_Form.BuildingList.GetFilteredList(string.Empty, string.Empty));
        }

        /// <summary>
        /// When text in the Building filter is changed, clear the list, and refill it from the Building List.
        /// </summary>
        /// <param name="sender">The Sender.</param>
        /// <param name="e">Any Event Args.</param>
        private void Filter_Changed(object sender, EventArgs e)
        {
            this.lbx_BuildingList.Items.Clear();
            this.lbx_BuildingList.Items.AddRange(Main_Form.BuildingList.GetFilteredList(string.Empty, this.txt_BuildingFilter.Text));
        }
    }
}
