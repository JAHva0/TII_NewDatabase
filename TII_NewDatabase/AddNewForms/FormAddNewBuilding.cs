// <summary>Form for adding a new building entry to the database. Handles error checking prior to committing the entry.</summary>

namespace TII_NewDatabase.AddNewForms
{
    using System;
    using System.Data;
    using System.Windows.Forms;
    using Database;
    using SQL;
    
    /// <summary>
    /// Creates a "New Building" Form.
    /// </summary>
    public partial class FormAddNewBuilding : Form
    {
        /// <summary> Building class to hold the form's information. Will also parse and error check prior to committing. </summary>
        private Building newBuilding = new Building();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FormAddNewBuilding"/> class.
        /// </summary>
        public FormAddNewBuilding()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormAddNewBuilding"/> class.
        /// </summary>
        /// <param name="company">The company with which to preemptively fill part of the form.</param>
        public FormAddNewBuilding(string company)
        {
            this.InitializeComponent();
            this.cbo_Owner.Text = company;
        }

        /// <summary> Gets the Address of the building added to the database by this form.</summary>
        /// <value> The address of the building added by this form. </value>
        /// <exception cref="ArgumentNullException"> This Property can only be called if the building class contained within is not empty.</exception>
        public string NewBuildingAddress
        {
            get
            {
                if (this.newBuilding.Street != string.Empty)
                {
                    return this.newBuilding.Street;
                }
                else
                {
                    throw new ArgumentNullException("You cannot retreive the address of a building that has not been committed");
                }
            }
        }

        /// <summary>
        /// Fires once upon opening to populate the Company and Contractor list boxes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Any Event Args.</param>
        private void OnLoad(object sender, EventArgs e)
        {
           foreach (DataRow r in Query.Select("Name", "Company", "1=1 ORDER BY Name").Rows)
           {
               this.cbo_Owner.Items.Add(r["Name"].ToString());
           }

           foreach (DataRow c in Query.Select("DISTINCT Contractor", "Building", "Contractor IS NOT NULL").Rows)
           {
               this.cbo_Contractor.Items.Add(c["Contractor"].ToString());
           }
        }

        /// <summary>
        /// Fires once the user tries to leave the combo box. Makes sure that they have selected an item from the list, 
        /// and not provided something that might break the company object.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Any Event Args.</param>
        private void EnsureValidListboxInput(object sender, EventArgs e)
        {
            ComboBox lbx_sender = (ComboBox)sender;

            if (!lbx_sender.Items.Contains(lbx_sender.Text) && lbx_sender.Text != string.Empty)
            {
                MessageBox.Show("Please select an item from the list");
                lbx_sender.Focus();
            }
        }

        /// <summary>
        /// Performs basic error checking and commits the object to the database via the <see cref="CommitToDatabase"/> method.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Any Event Args.</param>
        private void SaveEntry(object sender, EventArgs e)
        {
            if (this.txt_BuildingStreet.Text == string.Empty)
            {
                this.ShowInfoBox("Must have at least a building address in order to add an entry", "No Building Address");
                return;
            }

            try
            {
                this.newBuilding.Owner = new Company(this.cbo_Owner.Text);
                this.newBuilding.Name = this.txt_BuildingName.Text;
                this.newBuilding.Street = this.txt_BuildingStreet.Text;
                this.newBuilding.City = this.txt_BuildingCity.Text;
                this.newBuilding.State = this.txt_BuildingState.Text;
                this.newBuilding.Zip = this.txt_BuildingZip.Text;
                this.newBuilding.County = this.cbo_BuildingCounty.Text;
                this.newBuilding.FirmFee = new Money(this.txt_FirmFee.Text);
                this.newBuilding.HourlyFee = new Money(this.txt_HourlyFee.Text);
                this.newBuilding.Anniversary = this.cbo_BuildingAnniversary.Text;
                this.newBuilding.Contractor = this.cbo_Contractor.Text;

                if (this.newBuilding.SaveConfirmation())
                {
                    this.newBuilding.CommitToDatabase();
                }
            }
            catch (SQL.SQLDuplicateEntryException ex)
            {
                throw new SQLDuplicateEntryException("Duplicate Building Entry", ex, this.newBuilding);
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

        /// <summary>
        /// To make things simpler, if the State textbox is Validated and the text is "DC", we go ahead and select that as the county.
        /// </summary>
        /// <param name="sender">The State Text box.</param>
        /// <param name="e">On Validation.</param>
        private void ValidateState(object sender, EventArgs e)
        {
            if (((Control)sender).Text == "DC")
            {
                this.cbo_BuildingCounty.Text = "Washington D.C.";
                this.cbo_BuildingCounty.Enabled = false;
            }
            else
            {
                this.cbo_BuildingCounty.Enabled = true;
            }
        }
    }
}
