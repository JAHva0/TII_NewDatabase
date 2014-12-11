// <summary>Form for adding a new company entry to the database. Handles error checking prior to committing the entry.</summary>

namespace TII_NewDatabase.AddNewForms
{
    using System;
    using System.Windows.Forms;
    using Database;
    
    /// <summary>
    /// Creates a "New Company" Form.
    /// </summary>
    public partial class FormAddNewCompany : Form
    {
        /// <summary> Company class to hold the form's information. Will also parse and error check prior to committing. </summary>
        private Company newCompany = new Company();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FormAddNewCompany"/> class.
        /// </summary>
        public FormAddNewCompany()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Fires upon clicking the save button. Checks to make sure vital information is present in the form.
        /// Also catches any exceptions thrown by attempting to assign invalid data to the Company class and alerts
        /// the user, rather than just crashing out.
        /// </summary>
        /// <param name="sender">The Save Entry button.</param>
        /// <param name="e">Any Event Args.</param>
        private void SaveEntry(object sender, EventArgs e)
        {
            if (this.txt_CompanyName.Text == string.Empty)
            {
                this.ShowInfoBox("Must at the least have a company name in order to add an entry", "No Company Name");
                return;
            }
            
            try
            {
                this.newCompany.Name = this.txt_CompanyName.Text;
                this.newCompany.Street = this.txt_CompanyStreet.Text;
                this.newCompany.City = this.txt_CompanyCity.Text;
                this.newCompany.State = this.txt_CompanyState.Text;
                this.newCompany.Zip = this.txt_CompanyZip.Text;
            }
            catch (ArgumentException ex)
            {
                switch (ex.Message)
                {
                    case "Address.State must be a valid Abbreviation":
                        {
                            this.ShowInfoBox("'State' must be a valid abbreviation.", "Invalid State");
                            break;
                        }

                    case "Address.Zip must be exactly 5 Chars in length.":
                        {
                            this.ShowInfoBox("'Zip' must be a valid 5-digit number.", "Invalid Zip Code");
                            break;
                        }

                    case "Address.Zip must be a numeric string":
                        {
                            this.ShowInfoBox("'Zip' must be a valid 5-digit number.", "Invalid Zip Code");
                            break;
                        }

                    default:
                        {
                            throw ex;
                        }
                }

                return;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (this.newCompany.SaveConfirmation())
            {
                try
                {
                    this.newCompany.CommitToDatabase();
                }
                catch (SQL.SQLDuplicateEntryException ex)
                {
                    throw new SQL.SQLDuplicateEntryException("Duplicate Company Entry", ex, this.newCompany);
                }
                catch (Exception)
                {
                    throw;
                }
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
