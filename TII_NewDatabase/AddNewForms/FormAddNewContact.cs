// <summary> Form for adding a new contact entry to the database. Handles error checking prior to committing the entry. </summary>

namespace TII_NewDatabase.AddNewForms
{
    using System;
    using System.Windows.Forms;
    using Database;
    
    /// <summary> Creates a new "Add Contact" Form. </summary>
    public partial class FormAddNewContact : Form
    {
        /// <summary>
        /// Contact class to hold the form's information. Will also parse and error check prior to committing.
        /// </summary>
        private Contact newContact = new Contact();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FormAddNewContact"/> class.
        /// </summary>
        public FormAddNewContact()
        {
            this.InitializeComponent();
        }

        private void SaveEntry_Click(object sender, EventArgs e)
        {
            if (this.txt_Name.Text == string.Empty)
            {
                this.ShowInfoBox("Must at the least have a contact name in order to add an entry", "No Contact Name");
                return;
            }

            try
            {
                this.newContact.Name = this.txt_Name.Text;
                this.newContact.OfficePhone = new TelephoneNumber(this.txt_OfficePhone.Text, this.txt_Extension.Text);
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Private method to simplify showing a simple popup box.
        /// </summary>
        /// <param name="text">The text to display in the box.</param>
        /// <param name="caption">The title of the box.</param>
        private void ShowInfoBox(string text, string caption)
        {
            MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }
}
