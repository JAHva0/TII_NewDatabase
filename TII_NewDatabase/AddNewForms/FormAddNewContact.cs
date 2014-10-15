// <summary> Form for adding a new contact entry to the database. Handles error checking prior to committing the entry. </summary>

namespace TII_NewDatabase
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Windows.Forms;
    using Database;
    
    /// <summary> Creates a new "Add Contact" Form. </summary>
    public partial class FormAddNewContact : Form
    {
        /// <summary>
        /// Contact class to hold the form's information. Will also parse and error check prior to committing.
        /// </summary>
        private Contact newContact = new Contact();

        /// <summary> Stores a temporary list of every company name in the database. </summary>
        private List<string> companyList = new List<string>();

        /// <summary> Stores a temporary list of every building address in the database. </summary>
        private List<string> buildingList = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FormAddNewContact"/> class.
        /// </summary>
        public FormAddNewContact()
        {
            this.InitializeComponent();
            this.FilterChanged(new object(), EventArgs.Empty);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormAddNewContact"/> class.
        /// </summary>
        /// <param name="assigned_company"> A company this contact should start assigned to.</param>
        /// <param name="assigned_building"> A Building this contact should start assigned to.</param>
        public FormAddNewContact(Company assigned_company, Building assigned_building)
            : this()
        {
            // Remove the building from the left list and add it to the list on the right.
            this.lbx_BuildingList.Items.Remove(assigned_building.Street);
            this.lbx_AssociatedBuildings.Items.Add(assigned_building.Street);

            // Remove the company from the left list and add it to the list on the right.
            this.lbx_CompanyList.Items.Remove(assigned_company.Name);
            this.lbx_AssociatedCompanies.Items.Add(assigned_company.Name);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormAddNewContact"/> class.
        /// </summary>
        /// <param name="contactToEdit"> Contact with which to pre-populate the form. </param>
        public FormAddNewContact(Contact contactToEdit)
            : this()
        {
            this.txt_Name.Text = contactToEdit.Name;
            this.txt_OfficePhone.Text = contactToEdit.OfficePhone.Number;
            this.txt_Extension.Text = contactToEdit.OfficePhone.Ext;
            this.txt_CellPhone.Text = contactToEdit.CellPhone.Number;
            this.txt_Fax.Text = contactToEdit.Fax.Number;
            this.txt_Email.Text = contactToEdit.Email;

            foreach (string cpny in contactToEdit.CompanyList)
            {
                this.lbx_CompanyList.Items.Remove(cpny);
                this.lbx_AssociatedCompanies.Items.Add(cpny);
            }

            foreach (string bldg in contactToEdit.BuildingList)
            {
                this.lbx_BuildingList.Items.Remove(bldg);
                this.lbx_AssociatedBuildings.Items.Add(bldg);
            }

            this.newContact = contactToEdit;
        }

        /// <summary>
        /// Called any time the form needs to refresh the list of buildings or companies displayed in the list boxes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Any Event Args.</param>
        private void FilterChanged(object sender, EventArgs e)
        {
            if (this.tabctrl_Associate.SelectedTab == this.tab_Company)
            {
                this.lbx_CompanyList.Items.Clear();
                this.lbx_CompanyList.Items.AddRange(Main_Form.CompanyList.GetFilteredList(
                                                                                          string.Empty,
                                                                                          this.txt_CompanyFilter.Text, 
                                                                                          this.lbx_AssociatedCompanies.Items.Cast<string>().ToArray()));
            }

            if (this.tabctrl_Associate.SelectedTab == this.tab_Building)
            {
                this.lbx_BuildingList.Items.Clear();
                this.lbx_BuildingList.Items.AddRange(Main_Form.BuildingList.GetFilteredList(
                                                                                            string.Empty,
                                                                                            this.txt_BuildingFilter.Text,
                                                                                            this.lbx_AssociatedBuildings.Items.Cast<string>().ToArray()));
            }
        }

        /// <summary>
        /// Checks the entry for errors and completeness, then parses all of the data into a private class. That class attempts to insert it's data into the database.
        /// </summary>
        /// <param name="sender">The Save Entry button.</param>
        /// <param name="e">Any event args.</param>
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
                this.newContact.CellPhone = new TelephoneNumber(this.txt_CellPhone.Text);
                this.newContact.Fax = new TelephoneNumber(this.txt_Fax.Text);
                this.newContact.Email = this.txt_Email.Text;
            }
            catch (ArgumentException ex)
            {
                switch (ex.Message)
                {
                    case "'phonenumber' must be a valid 10-digit phone number.":
                        {
                            this.ShowInfoBox("Invalid Phone Number", "Check Phone Number");
                            return;
                        }

                    case "Invalid E-mail format":
                        {
                            this.ShowInfoBox("Check that the e-mail provided is complete", "Incomplete Email");
                            return;
                        }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            this.newContact.CommitToDatabase();
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
        /// Based on the attributes of the button which called the method, move items from one list box to another.
        /// </summary>
        /// <param name="sender">The Button who called the method.</param>
        /// <param name="e">Any event args.</param>
        private void MoveListboxItem(object sender, EventArgs e)
        {
            Button btn_sender = (Button)sender;

            // The button is located on the Company tab page
            if (btn_sender.Parent == this.tab_Company)
            {
                if (btn_sender.Text == ">>" && this.lbx_CompanyList.SelectedItem != null)
                {
                    this.lbx_AssociatedCompanies.Items.Add(this.lbx_CompanyList.SelectedItem);
                    this.lbx_CompanyList.Items.Remove(this.lbx_CompanyList.SelectedItem);
                }

                if (btn_sender.Text == "<<" && this.lbx_AssociatedCompanies.SelectedItem != null)
                {
                    this.lbx_CompanyList.Items.Add(this.lbx_AssociatedCompanies.SelectedItem);
                    this.lbx_AssociatedCompanies.Items.Remove(this.lbx_AssociatedCompanies.SelectedItem);
                }
            }

            // The button is located on the Building tab page
            if (btn_sender.Parent == this.tab_Building)
            {
                if (btn_sender.Text == ">>" && this.lbx_BuildingList.SelectedItem != null)
                {
                    this.lbx_AssociatedBuildings.Items.Add(this.lbx_BuildingList.SelectedItem);
                    this.lbx_BuildingList.Items.Remove(this.lbx_BuildingList.SelectedItem);
                }

                if (btn_sender.Text == "<<" && this.lbx_AssociatedBuildings.SelectedItem != null)
                {
                    this.lbx_BuildingList.Items.Add(this.lbx_AssociatedBuildings.SelectedItem);
                    this.lbx_AssociatedBuildings.Items.Remove(this.lbx_AssociatedBuildings.SelectedItem);
                }
            }
        }
    }
}
