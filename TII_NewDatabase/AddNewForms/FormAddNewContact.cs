﻿// <summary> Form for adding a new contact entry to the database. Handles error checking prior to committing the entry. </summary>

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
            this.FillRelationLists();
            this.FilterChanged(new object(), EventArgs.Empty);
        }

        /// <summary>
        /// Queries the database for a list of every company and building.
        /// </summary>
        private void FillRelationLists()
        {
            foreach (DataRow c in SQL.Query.Select("Name", "Company", "1=1").Rows)
            {
                this.companyList.Add(c["Name"].ToString());
            }

            foreach (DataRow b in SQL.Query.Select("Address", "Building", "1=1").Rows)
            {
                this.buildingList.Add(b["Address"].ToString());
            }     
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
                IEnumerable<string> filtered = this.companyList.Where(s => s.Contains(this.txt_CompanyFilter.Text) && !this.lbx_AssociatedCompanies.Items.Contains(s));
                this.lbx_CompanyList.Items.Clear();
                this.lbx_CompanyList.Items.AddRange(filtered.ToArray());
            }

            if (this.tabctrl_Associate.SelectedTab == this.tab_Building)
            {
                IEnumerable<string> filtered = this.buildingList.Where(s => s.Contains(this.txt_BuildingFilter.Text) && !this.lbx_AssociatedBuildings.Items.Contains(s));
                this.lbx_BuildingList.Items.Clear();
                this.lbx_BuildingList.Items.AddRange(filtered.ToArray());
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
                this.newContact.Name = this.cbo_Honorific.Text + " " + this.txt_Name.Text;
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
    }
}
