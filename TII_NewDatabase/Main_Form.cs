//-----------------------------------------------------------------------
// <summary>Main Form Functions.</summary>
//-----------------------------------------------------------------------

namespace TII_NewDatabase
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using Database;
    using SQL;    

    /// <summary>
    /// Containing class for Form Functions.
    /// </summary>
    public partial class Main_Form : Form
    {
        /// <summary>
        /// Dictionary that contains Company Name, Location (DC, MD or BOTH), and Active Status for Sorting/Reference. Indexed by Company_ID.
        /// </summary>
        private Dictionary<int, CompanyListItem> companyList = new Dictionary<int, CompanyListItem>();

        /// <summary>
        /// Dictionary that contains Building Address, Location (DC or MD), and Active status for Sorting/Reference. Indexed by Building_ID.
        /// </summary>
        private Dictionary<int, BuildingListItem> buildingList = new Dictionary<int, BuildingListItem>();

        /// <summary> Boolean that is false until the form is done loading, to prevent unwanted tripping of events before the user sees the form.</summary>
        private bool form_loaded = false;

        /// <summary> The Company currently selected on the form. Allows us easy access to a contact list. </summary>
        private Company currentlySelectedCompany;

        /// <summary> The Building currently selected on the form. Allows us easy access to a contact list. </summary>
        private Building currentlySelectedBuilding;

        /// <summary> The Contact currently selected on the form. </summary>
        private Contact currentltSelectedContact;
        
        /// <summary>Initializes a new instance of the <see cref="Main_Form"/> class.</summary>
        public Main_Form()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Occurs just before the form is displayed. 
        /// Check to make sure the credentials are valid, otherwise this whole database bit is pointless.
        /// </summary>
        /// <param name="sender">Will always be the form.</param>
        /// <param name="e">Will always be empty.</param>
        private void Main_Form_Load(object sender, EventArgs e)
        {
            Connection.CreateConnection(
                                        Properties.Settings.Default.UserName,
                                        Properties.Settings.Default.Password,
                                        Properties.Settings.Default.ServerAddress);
            
            // Check if the stored connection criteria are valid
            if (!Connection.ConnectionUp)
            {
                while (!Connection.ConnectionUp)
                {
                    // See if there are any stored fields are blank
                    if (Properties.Settings.Default.UserName == string.Empty ||
                        Properties.Settings.Default.Password == string.Empty ||
                        Properties.Settings.Default.ServerAddress == string.Empty)
                    {
                        if (
                            MessageBox.Show(
                                            "A Valid Connection must be made prior to opening the Database - Please ensure all credentials are stored and valid.",
                                            "Missing Connection Credentials",
                                            MessageBoxButtons.OKCancel,
                                            MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Cancel)
                        {
                            this.Close(); // If the user cancels the Missing Credentials popup, just shut down everything.
                        }
                    }
                    else
                    {
                        if (
                            MessageBox.Show(
                                            "The Stored Credentials do not appear to be valid. Please check the UserName, Password, and Server Location.",
                                            "Invalid Connection Credentials",
                                            MessageBoxButtons.OKCancel,
                                            MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Cancel)
                        {
                            this.Close(); // If the user cancels the Invalid Credentials popup, just shut down everything.
                        }
                    }

                    // Create a new Connection that can be tested
                    this.OpenConnectionSettingsForm(new object(), EventArgs.Empty);
                    Connection.CreateConnection(
                                                Properties.Settings.Default.UserName,
                                                Properties.Settings.Default.Password,
                                                Properties.Settings.Default.ServerAddress);
                }
            }

            // Initialize the checkbox filters to whatever the saved settings are
            this.cbx_ShowMD.Checked = Properties.Settings.Default.MDFilterOn;
            this.cbx_ShowDC.Checked = Properties.Settings.Default.DCFilterOn;
            this.cbx_ShowInactive.Checked = Properties.Settings.Default.InactveFilterOn;

            // Populate the Dictionaries with the Company and Building Information
            this.companyList = new Dictionary<int, CompanyListItem>();
            foreach (DataRow companyRow in Query.Select("SELECT DISTINCT " +
                                                            "Company.Company_ID, " +
                                                            "Company.Name, " +
                                                            "CASE " + // If the Company_ID appears in both the DC and MD lists, mark it 'DCMD'.
                                                                "WHEN Company.Company_ID IN " +
                                                                "( " +
                                                                    "SELECT Company_ID  " +
                                                                    "FROM Company " +
                                                                    "WHERE Company_ID IN  " +
                                                                        "( " +
                                                                        "SELECT Company_ID  " +
                                                                        "FROM Building " +
                                                                        "WHERE State = 'MD' " +
                                                                        ") " +
                                                                    "AND Company_ID IN " +
                                                                        "( " +
                                                                        "SELECT Company_ID " +
                                                                        "FROM Building " +
                                                                        "WHERE State = 'DC' " +
                                                                        ") " +
                                                                ") THEN 'DCMD' " +
                                                                "ELSE Building.State " +
                                                            "END as Location, " +
                                                            "CASE " + // If the Company has any buildings in the Active List, mark it 'True'.
                                                                "WHEN Company.Company_ID IN " +
                                                                "( " +
                                                                    "SELECT Company_ID " +
                                                                    "FROM Building " +
                                                                    "WHERE Active = 'True' " +
                                                                ") THEN 'True' " +
                                                                "ELSE 'False' " +
                                                            "END as Active " +
                                                        "FROM Company " +
                                                        "JOIN Building ON Building.Company_ID = Company.Company_ID").Rows)
            {
                this.companyList.Add(
                                     Convert.ToInt32(companyRow["Company_ID"]), 
                                     new CompanyListItem(
                                                         companyRow["Name"].ToString(), 
                                                         companyRow["Location"].ToString(), 
                                                         Convert.ToBoolean(companyRow["Active"])));
            }

            this.buildingList = new Dictionary<int, BuildingListItem>();
            foreach (DataRow buildingRow in Query.Select("SELECT Building_ID, Address, state, Active FROM Building").Rows)
            {
                this.buildingList.Add(
                                      Convert.ToInt32(buildingRow["Building_ID"]),
                                      new BuildingListItem(
                                                           buildingRow["Address"].ToString(),
                                                           buildingRow["state"].ToString(),
                                                           Convert.ToBoolean(buildingRow["Active"])));
            }

            this.PopulateListboxes();

            // Select the first item in the Company List to trigger the fields to fill with data.
            this.lbx_CompanyList.SelectedIndex = 0;

            // We're done with all that, so if things want to start triggerign now (looking at you checkboxes) they can.
            this.form_loaded = true;
        }

        /// <summary>
        /// Ask the user if they intended to close the form, and if so, do so.
        /// </summary>
        /// <param name="sender">The name of the sender who called this function.</param>
        /// <param name="e">Any passed EventArgs.</param>
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to Exit?", "Exit Database?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
            {
                this.Close();
            }
        }

        /// <summary>
        /// Open the Connection Settings Dialog.
        /// </summary>
        /// <param name="sender">The name of the sender who called this function.</param>
        /// <param name="e">Any passed EventArgs.</param>
        private void OpenConnectionSettingsForm(object sender, EventArgs e)
        {
            using (FormConnectionSettings connectionSettings = new FormConnectionSettings())
            {
                connectionSettings.ShowDialog();
            }
        }

        /// <summary>
        /// Fires when the checkbox on any of the filters is changed. Saves the new setting and refreshes the lists.
        /// </summary>
        /// <param name="sender">The name of the sender who called this function.</param>
        /// <param name="e">Any passed EventArgs.</param>
        private void FilterChanged(object sender, EventArgs e)
        {
            // Quit out immediatly if the form isn't even loaded.
            if (!this.form_loaded)
            {
                return;
            }
            
            // Find out which checkbox was clicked
            switch (((CheckBox)sender).Name)
            {
                case "cbx_ShowMD":
                    {
                        Properties.Settings.Default.MDFilterOn = this.cbx_ShowMD.Checked;
                        break;
                    }

                case "cbx_ShowDC": 
                    {
                        Properties.Settings.Default.DCFilterOn = this.cbx_ShowDC.Checked; 
                        break;
                    }

                case "cbx_ShowInactive":
                    {
                        Properties.Settings.Default.InactveFilterOn = this.cbx_ShowInactive.Checked;
                        break;
                    }
            }

            Properties.Settings.Default.Save();

            // Refresh the Company and Building Lists to apply the new filters
            this.PopulateListboxes();
        }

        /// <summary>
        /// When called, populate the Company and Building Lists based on the contents of the Dictionaries, as well as the checkbox filters.
        /// </summary>
        private void PopulateListboxes()
        {            
            string dc_md_both = "NOTHING SELECTED"; // Default to something that nothing would have, in case neither checkbox is selected
            if (this.cbx_ShowDC.Checked && !this.cbx_ShowMD.Checked)
            {
                dc_md_both = "DC"; // Only Show Companies with DC Buildings
            }

            if (!this.cbx_ShowDC.Checked && this.cbx_ShowMD.Checked)
            {
                dc_md_both = "MD"; // Only Show Companies with MD Buildings
            }

            if (this.cbx_ShowDC.Checked && this.cbx_ShowMD.Checked)
            {
                dc_md_both = string.Empty; // Show All Companies
            }

            IEnumerable<CompanyListItem> filtered_companies;
            IEnumerable<BuildingListItem> filtered_buildings;

            // Check to see which tab is active, so we don't waste time refreshing data that isn't visible
            if (this.tab_BuildingCompanySelector.SelectedTab == this.tab_ByCompany)
            {
                // if Show Inactive is not checked, don't add companies if they don't have any active buildings
                if (!this.cbx_ShowInactive.Checked)
                {
                    filtered_companies =
                        from c in this.companyList.Values
                        where c.IsActive
                                && c.DCorMD.Contains(dc_md_both)
                                && c.Name.ToLower().Contains(this.txt_FilterCompany.Text.ToLower())
                        select c;
                }
                else
                {
                    filtered_companies =
                        from c in this.companyList.Values
                        where c.DCorMD.Contains(dc_md_both)
                                && c.Name.ToLower().Contains(this.txt_FilterCompany.Text.ToLower())
                        select c;
                }

                this.lbx_CompanyList.Items.Clear(); // Clear the Company List
                foreach (CompanyListItem i in filtered_companies)
                {
                    this.lbx_CompanyList.Items.Add(i.Name); // Add in Each Company name that wasn't filtered.
                }
            }
            else
            {
                if (!this.cbx_ShowInactive.Checked)
                {
                    filtered_buildings =
                        from b in this.buildingList.Values
                        where b.IsActive
                                && b.DCorMD.Contains(dc_md_both)
                                && b.Address.ToLower().Contains(this.txt_FilterAddress.Text.ToLower())
                        select b;
                }
                else
                {
                    filtered_buildings =
                        from b in this.buildingList.Values
                        where b.DCorMD.Contains(dc_md_both)
                                && b.Address.ToLower().Contains(this.txt_FilterAddress.Text.ToLower())
                        select b;
                }

                this.lbx_BuildingList.Items.Clear();
                foreach (BuildingListItem i in filtered_buildings)
                {
                    this.lbx_BuildingList.Items.Add(i.Address);
                }
            }
        }

        /// <summary>
        /// Fires every time either of the filter text boxes are changed. 
        /// Pulls out items from the list which do not contain the text.
        /// </summary>
        /// <param name="sender">Either txt_CompanyFilter or txt_BuildingFilter.</param>
        /// <param name="e">Will always be Empty.</param>
        private void Filter_TextChanged(object sender, EventArgs e)
        {
            this.PopulateListboxes();
        }

        /// <summary>
        /// Fires whenever the user selects an item from either list box.
        /// </summary>
        /// <param name="sender">The List box which called the event.</param>
        /// <param name="e">Any Associated EventArgs.</param>
        private void ListBox_ItemSelected(object sender, EventArgs e)
        {
            ListBox currentLbx = (ListBox)sender;
            int selected_id;

            // Check which listbox was selected
            if (currentLbx.Name == "lbx_CompanyList")
            {
                selected_id = (from c in this.companyList
                               where c.Value.Name == currentLbx.SelectedItem.ToString()
                               select c.Key).Single();
                this.currentlySelectedCompany = new Company(selected_id);
                this.PopulateFields(this.currentlySelectedCompany);
            }

            if (currentLbx.Name == "lbx_BuildingList" || currentLbx.Name == "lbx_OtherCompanyBuildings")
            {
                selected_id = (from b in this.buildingList
                               where b.Value.Address == currentLbx.SelectedItem.ToString()
                               select b.Key).Single();
                this.currentlySelectedBuilding = new Building(selected_id);
                this.PopulateFields(this.currentlySelectedBuilding);
            }

            // Roll through all of the controls in the Contact Information group box, and clear anything that is a Text Box.
            foreach (Control tbox in this.gbx_ContactInfo.Controls)
            {
                if (tbox is TextBox)
                {
                    ((TextBox)tbox).Clear();
                }
            }
        }

        /// <summary>
        /// Populates the browser fields once a company name has been selected from the list box.
        /// </summary>
        /// <param name="selected_company">The company that was selected.</param>
        private void PopulateFields(Company selected_company)
        {
            Debug.Assert(selected_company != null, "We should not be able to get a null Company passed to this method.");

            this.txt_CompanyName.Text = selected_company.Name;
            this.txt_CompanyStreet.Text = selected_company.Street;
            this.txt_CompanyCity.Text = selected_company.City;
            this.txt_CompanyState.Text = selected_company.State;
            this.txt_CompanyZip.Text = selected_company.Zip;

            // Fill the Associated Buildings Listbox with any addressed that also belong to this company.
            this.lbx_OtherCompanyBuildings.Items.Clear();
            foreach (string address in selected_company.AssociatedBuildings)
            {
                this.lbx_OtherCompanyBuildings.Items.Add(address);
            }

            this.lbx_CompanyContacts.Items.Clear();
            foreach (Contact c in selected_company.ContactList)
            {
                this.lbx_CompanyContacts.Items.Add(c.Name);
            }

            this.lbx_OtherCompanyBuildings.SelectedIndex = 0;
        }

        /// <summary>
        /// Populates the browser fields once a building address has been selected from the list box.
        /// </summary>
        /// <param name="selected_building">The Building that was selected.</param>
        private void PopulateFields(Building selected_building)
        {
            Debug.Assert(selected_building != null, "We should not be able to get a null Building passed to this method.");

            this.txt_BuildingName.Text = selected_building.Name;
            this.txt_BuildingAddress.Text = selected_building.Street;
            this.txt_BuildingCity.Text = selected_building.City;
            this.txt_BuildingState.Text = selected_building.State;
            this.txt_BuildingZip.Text = selected_building.Zip;
            this.cbo_BuildingCounty.Text = selected_building.County;
            this.txt_FirmFee.Text = selected_building.FirmFee.ToString();
            this.txt_HourlyFee.Text = selected_building.HourlyFee.ToString();

            if (selected_building.Anniversary != string.Empty)
            {
                this.cbo_BuildingAnniversary.Text = selected_building.Anniversary;
            }

            this.txt_Contractor.Text = selected_building.Contractor;
            this.cbx_BuildingActive.Checked = selected_building.Active;

            // Populate the elevator list box.
            this.lbx_ElevatorList.Items.Clear();
            foreach (string elev in selected_building.ElevatorList)
            {
                this.lbx_ElevatorList.Items.Add(elev);
            }

            // Populate the Inspection History list view
            this.lvw_InspectionList.Items.Clear();
            foreach (Building.InspectionHistory historyitem in selected_building.Inspection_History)
            {
                ListViewItem i = new ListViewItem(historyitem.Date.ToShortDateString());
                i.SubItems.Add(historyitem.Type);
                i.SubItems.Add(historyitem.Status);
                i.SubItems.Add(historyitem.Inspector);
                if (historyitem.Report)
                {
                    i.SubItems.Add("Yes");
                }
                else
                {
                    i.SubItems.Add("No");
                }

                this.lvw_InspectionList.Items.Add(i);
            }

            // Populate the Building Contact List
            this.lbx_BuildingContacts.Items.Clear();
            foreach (Contact c in selected_building.ContactList)
            {
                this.lbx_BuildingContacts.Items.Add(c.Name);
            }

            // Populate the Company fields, now that we know the building and, by extension, it's owner
            // Assuming that the selected company isn't already the correct one.
            if (selected_building.Owner.Name != this.txt_CompanyName.Text)
            {
                this.PopulateFields(selected_building.Owner);
            }

            this.lbx_OtherCompanyBuildings.SelectedItem = selected_building.Street;
        }

        /// <summary>
        /// Method called when the user clicks on the Inspection History List Box.
        /// Will check to be sure it is a right click, and if so will provide a context menu to open the report.
        /// Determines the report by Querying the inspections table for all reports for that building on that day.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The Mouse Event Arguments.</param>
        private void Inspection_RightClick(object sender, MouseEventArgs e)
        {
            // Not interested if the left mouse button was clicked. Exit out.
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                return;
            }
            
            ContextMenu reportFileMenu;

            // Check to see if the item the user clicked was noted to have a report when it loaded and provide the appropriate context menu.
            if (this.lvw_InspectionList.SelectedItems[0].SubItems[4].Text == "Yes")
            {
                reportFileMenu = new ContextMenu(new MenuItem[] { new MenuItem("Open Report File") });
                reportFileMenu.MenuItems[0].Click += this.ReportFileMenu_Click;
            }
            else
            {
                reportFileMenu = new ContextMenu(new MenuItem[] { new MenuItem("No Report Associated") });
                reportFileMenu.MenuItems[0].Enabled = false; // Don't enable the menu item, it's just for information
            }

            this.lvw_InspectionList.ContextMenu = reportFileMenu;
        }

        /// <summary>
        /// Method called by the Context menu.
        /// </summary>
        /// <param name="sender">The Context Menu that was the sender.</param>
        /// <param name="e">And Event Arguments.</param>
        private void ReportFileMenu_Click(object sender, EventArgs e)
        {
            DataRowCollection report_file = SQL.Query.Select(
                                                             string.Format(
                                                                           "SELECT DISTINCT Report " +
                                                                           "FROM Inspection " +
                                                                           "JOIN Elevator ON Inspection.Elevator_ID = Elevator.Elevator_ID " +
                                                                           "WHERE Elevator.Building_ID = " +
                                                                                                         "( " +
                                                                                                         "SELECT Building_ID " +
                                                                                                         "FROM Building " +
                                                                                                         "WHERE Address = '{0}' " +
                                                                                                         ") " +
                                                                           "AND Date = '{1}'",
                                                                           this.txt_BuildingAddress.Text,
                                                                           this.lvw_InspectionList.SelectedItems[0].Text)).Rows;
            
            // In case we end up with more than one report from that date, open them all.
            foreach (DataRow row in report_file)
            {
                if (File.Exists(row["Report"].ToString()))
                {
                    Process.Start(row["Report"].ToString());
                }
                else
                {
                    MessageBox.Show("Unable to Locate Report File.");
                }
            }
        }

        /// <summary>
        /// Event that fires whenever a new contact is selected.
        /// </summary>
        /// <param name="sender">The control that did the sending.</param>
        /// <param name="e">And Events.</param>
        private void Contact_Selected(object sender, EventArgs e)
        {
            // Bail out if we didn't even select anything.
            if (((ListBox)sender).SelectedItem == null)
            {
                return;
            }
            
            // See which list box was selected, and make sure the other one is unselected when that happens
            if (((ListBox)sender).Name == "lbx_CompanyContacts")
            {
                if (this.lbx_BuildingContacts.SelectedIndex != -1)
                {
                    this.lbx_BuildingContacts.SelectedIndex = -1;
                }
            }

            if (((ListBox)sender).Name == "lbx_BuildingContacts")
            {
                if (this.lbx_CompanyContacts.SelectedIndex != -1)
                {
                    this.lbx_CompanyContacts.SelectedIndex = -1;
                }
            }

            List<Contact> groupedContactList = new List<Contact>();
            groupedContactList.AddRange(this.currentlySelectedCompany.ContactList);
            groupedContactList.AddRange(this.currentlySelectedBuilding.ContactList);

            this.currentltSelectedContact = (from c in groupedContactList
                                             where c.Name == ((ListBox)sender).SelectedItem.ToString()
                                             select c).FirstOrDefault();

            this.txt_ContactName.Text = this.currentltSelectedContact.Name;
            this.txt_OfficePhone.Text = this.currentltSelectedContact.OfficePhone;
            this.txt_CellPhone.Text = this.currentltSelectedContact.CellPhone;
            this.txt_Fax.Text = this.currentltSelectedContact.Fax;
            this.txt_Email.Text = this.currentltSelectedContact.Email;
        }

        /// <summary>
        /// Searches the provided Group box and sets any text boxes found within to the provided state.
        /// </summary>
        /// <param name="box">The Group box to run through.</param>
        /// <param name="state">The Read Only State to set the Text Boxes to.</param>
        private void SetGroupBoxReadOnlyState(GroupBox box, bool state)
        {
            foreach (Control con in box.Controls)
            {
                if (con is TextBox)
                {
                    ((TextBox)con).ReadOnly = state;
                }

                if (con is CheckBox)
                {
                    ((CheckBox)con).Enabled = !state;
                }

                if (con is ComboBox)
                {
                    ((ComboBox)con).Enabled = !state;

                    // Set the background color to make it clear to the user what state the Combo Box is in.
                    if (state)
                    {
                        con.BackColor = SystemColors.Control;
                    }
                    else
                    {
                        con.BackColor = SystemColors.Window;
                    }
                }
            }
        }

        /// <summary>
        /// Enables the text boxes in either the company, building, or contact group boxes to be edited by the user.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">Any Event Args.</param>
        private void EnableEditing(object sender, EventArgs e)
        {
            this.tab_BuildingCompanySelector.Enabled = false;
            
            Button buttonSender = (Button)sender;
            GroupBox buttonParent = (GroupBox)buttonSender.Parent;
            this.SetGroupBoxReadOnlyState(buttonParent, false);
            buttonSender.Text = "Save";
            buttonSender.Click += this.SaveChanges;
            buttonSender.Click += this.DisableEditing;
            buttonSender.Click -= this.EnableEditing;

            // Create a cancel button and add it just below the clicked button.
            Button btn_Cancel = new Button();
            btn_Cancel.Text = "Cancel";
            btn_Cancel.Name = "btn_Cancel";
            btn_Cancel.Size = buttonSender.Size;
            btn_Cancel.Parent = buttonParent;
            btn_Cancel.Location = new System.Drawing.Point(buttonSender.Location.X, buttonSender.Location.Y + 25);
            btn_Cancel.Click += this.DisableEditing;
            buttonParent.Controls.Add(btn_Cancel);
        }

        /// <summary>
        /// Disables editing in the group box. Also removes the "Cancel" button and resets the edit button back to it's original state.
        /// </summary>
        /// <param name="sender">The button which triggered the event.</param>
        /// <param name="e">Any Event Args.</param>
        private void DisableEditing(object sender, EventArgs e)
        {  
            Button buttonSender = (Button)sender;
            GroupBox buttonParent = (GroupBox)buttonSender.Parent;
            this.SetGroupBoxReadOnlyState(buttonParent, true);
            buttonParent.Controls.RemoveByKey("btn_Cancel");

            // Find the button that should intialize editing, and reset the text and the target for the click event.
            Button editButton = (from b in buttonParent.Controls.Cast<Button>()
                              where b.Name.Contains("btn_Edit")
                              select b).First();
            editButton.Text = "Edit";
            editButton.Click += this.EnableEditing;
            editButton.Click -= this.SaveChanges;

            // Only re-enable the Tab control if there are no cancel buttons left on the form.
            // Prevents the control from re-enabling if the user is editing multiple entries at the same time
            if (this.Controls.Find("btn_Cancel", true).Count() == 0)
            {
                this.tab_BuildingCompanySelector.Enabled = true;
            }
        }

        /// <summary>
        /// Does nothing for the moment. Placeholder.
        /// </summary>
        /// <param name="sender">The button which triggered the event.</param>
        /// <param name="e">Any Event Args.</param>
        private void SaveChanges(object sender, EventArgs e)
        {            
            Button buttonSender = (Button)sender;
            GroupBox buttonParent = (GroupBox)buttonSender.Parent;

            switch (buttonParent.Text)
            {
                case "Company":
                    {
                        Debug.Assert(this.currentlySelectedCompany != null, "Company cannot be null at this point");
                        
                        this.currentlySelectedCompany.Name = this.txt_CompanyName.Text;
                        this.currentlySelectedCompany.Street = this.txt_CompanyStreet.Text;
                        this.currentlySelectedCompany.City = this.txt_CompanyCity.Text;
                        this.currentlySelectedCompany.State = this.txt_CompanyState.Text;
                        this.currentlySelectedCompany.Zip = this.txt_CompanyZip.Text;

                        if (this.currentlySelectedCompany.SaveConfirmation())
                        {
                            this.currentlySelectedCompany.CommitToDatabase();
                        }

                        break;
                    }

                case "Building":
                    {
                        Debug.Assert(this.currentlySelectedBuilding != null, "Building cannot be null at this point");

                        this.currentlySelectedBuilding.Name = this.txt_BuildingName.Text;
                        this.currentlySelectedBuilding.Street = this.txt_BuildingAddress.Text;
                        this.currentlySelectedBuilding.City = this.txt_BuildingCity.Text;
                        this.currentlySelectedBuilding.State = this.txt_BuildingState.Text;
                        this.currentlySelectedBuilding.Zip = this.txt_BuildingZip.Text;
                        this.currentlySelectedBuilding.Active = this.cbx_BuildingActive.Checked;
                        this.currentlySelectedBuilding.County = this.cbo_BuildingCounty.Text;
                        this.currentlySelectedBuilding.FirmFee = new Money(this.txt_FirmFee.Text);
                        this.currentlySelectedBuilding.HourlyFee = new Money(this.txt_HourlyFee.Text);
                        this.currentlySelectedBuilding.Anniversary = this.cbo_BuildingAnniversary.Text;
                        this.currentlySelectedBuilding.Contractor = this.txt_Contractor.Text;

                        if (this.currentlySelectedBuilding.SaveConfirmation())
                        {
                            this.currentlySelectedBuilding.CommitToDatabase();
                        }

                        break;
                    }

                case "Contact":
                    {
                        break;
                    }

                default: throw new NotImplementedException("How did you get here?");
            }
        }

        /// <summary>
        /// Opens the Add New Company form.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Any Event Args.</param>
        private void AddNewCompany(object sender, EventArgs e)
        {
            FormAddNewCompany newCompany = new FormAddNewCompany();
            newCompany.ShowDialog();
        }

        /// <summary>
        /// Opens the Add New Building Form.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Any Event Args.</param>
        private void AddNewBuilding(object sender, EventArgs e)
        {
            FormAddNewBuilding newBuilding = new FormAddNewBuilding(this.txt_CompanyName.Text);
            newBuilding.ShowDialog();
        }

        /// <summary>
        /// Spawns a Context Menu from right clicking on either of the contact list boxes.
        /// </summary>
        /// <param name="sender">The sender. Either Company Contacts or Building Contacts.</param>
        /// <param name="e">Any Event Args.</param>
        private void Contacts_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ContextMenu editContact = new ContextMenu();
                editContact.Name = ((Control)sender).Name + "_ctxMnu";
                editContact.MenuItems.Add("Add New Contact");
                editContact.MenuItems[0].Click += this.AddContact;
                editContact.MenuItems.Add("Add Existing Contact");
                editContact.MenuItems[1].Click += this.AddContact;
                editContact.MenuItems.Add("Remove Contact");
                editContact.MenuItems[2].Click += this.RemoveContact;

                // If the sender doesn't actually have anything selected, we can't remove it, so gray that option out.
                if (((ListBox)sender).SelectedItem == null)
                {
                    editContact.MenuItems[2].Enabled = false;
                }

                ((ListBox)sender).ContextMenu = editContact;
            }
        }

        private void AddContact(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Removes a contact from either a building or a company with which they are currently associated.
        /// </summary>
        /// <param name="sender">A context menu spawned over either of the contact list boxes.</param>
        /// <param name="e">Any Event Args.</param>
        private void RemoveContact(object sender, EventArgs e)
        {
            if (((MenuItem)sender).Parent.Name == "lbx_CompanyContacts_ctxMnu")
            {
                if (MessageBox.Show(string.Format("Remove {0} from {1}?", this.lbx_CompanyContacts.SelectedItem, this.txt_CompanyName.Text), "Remove Contact?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.OK)
                {
                    this.currentltSelectedContact.RemoveFromCompany(this.txt_CompanyName.Text);
                }
            }
            else if (((MenuItem)sender).Parent.Name == "lbx_BuildingContacts_ctxMnu")
            {
                if (MessageBox.Show(string.Format("Remove {0} from {1}?", this.lbx_BuildingContacts.SelectedItem, this.txt_BuildingAddress.Text), "Remove Contact?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.OK)
                {
                    this.currentltSelectedContact.RemoveFromBuilding(this.txt_BuildingAddress.Text);
                }
            }
        }

        /// <summary>
        /// Struct for holding Company Info used to sort/filter Companies in the ListBox.
        /// </summary>
        private struct CompanyListItem
        {
            /// <summary>Company Name.</summary>
            public string Name;

            /// <summary>Can be DC, MD or BOTH.</summary>
            public string DCorMD;

            /// <summary>Indicates if the Company has any active buildings associated or not.</summary>
            public bool IsActive;

            /// <summary>
            /// Initializes a new instance of the <see cref="CompanyListItem"/> struct.
            /// </summary>
            /// <param name="name">Company Name.</param>
            /// <param name="dc_or_md">Can be DC, MD or BOTH.</param>
            /// <param name="active">Indicates if the Company has any active buildings associated or not.</param>
            public CompanyListItem(string name, string dc_or_md, bool active)
            {
                this.Name = name;
                this.DCorMD = dc_or_md;
                this.IsActive = active;
            }
        }

        /// <summary>
        /// Struct for holding Building Info used to sort/filter Buildings in the ListBox.
        /// </summary>
        private struct BuildingListItem
        {
            /// <summary>Building Address.</summary>
            public string Address;

            /// <summary>Can be DC, MD or BOTH.</summary>
            public string DCorMD;

            /// <summary>Indicates if the Company has any active buildings associated or not.</summary>
            public bool IsActive;

            /// <summary>
            /// Initializes a new instance of the <see cref="BuildingListItem"/> struct.
            /// </summary>
            /// <param name="address">Building Address.</param>
            /// <param name="dc_or_md">Can be DC or MD.</param>
            /// <param name="active">Indicates if the building is listed as active.</param>
            public BuildingListItem(string address, string dc_or_md, bool active)
            {
                this.Address = address;
                this.DCorMD = dc_or_md;
                this.IsActive = active;
            }
        }
    }
}
