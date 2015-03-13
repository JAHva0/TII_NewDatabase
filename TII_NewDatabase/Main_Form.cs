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
    using AddNewForms;
    using Database;
    using HelperClasses;
    using SQL;

    /// <summary>
    /// Containing class for Form Functions.
    /// </summary>
    public partial class Main_Form : Form
    {
        /// <summary>
        /// List that contains Company Name, Location (DC, MD or BOTH), and Active Status for Sorting/Reference.
        /// </summary>
        private static DatabaseList companyList;

        /// <summary>
        /// List that contains Building Address, Location (DC or MD), and Active status for Sorting/Reference.
        /// </summary>
        private static DatabaseList buildingList;

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

        /// <summary> Gets a reference to the stored company list. </summary>
        /// <value>The stored company list.</value>
        public static DatabaseList CompanyList
        {
            get
            {
                return companyList;
            }
        }

        /// <summary> Gets a reference to the stored building list. </summary>
        /// <value>The stored building list.</value>
        public static DatabaseList BuildingList
        {
            get
            {
                return buildingList;
            }
        }

        /// <summary>
        /// Occurs just before the form is displayed. 
        /// Check to make sure the credentials are valid, otherwise this whole database bit is pointless.
        /// </summary>
        /// <param name="sender">Will always be the form.</param>
        /// <param name="e">Will always be empty.</param>
        private void Main_Form_Load(object sender, EventArgs e)
        {
            #region Check Connection
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
            #endregion

            // Initialize the checkbox filters to whatever the saved settings are
            this.cbx_ShowMD.Checked = Properties.Settings.Default.MDFilterOn;
            this.cbx_ShowDC.Checked = Properties.Settings.Default.DCFilterOn;
            this.cbx_ShowInactive.Checked = Properties.Settings.Default.InactveFilterOn;

            // Initalize the contractor checkbox
            this.cbo_Contractor.Items.AddRange(Building.ContractorList.ToArray());

            // Populate the Lists with the Company and Building Information
            string companyQuery = "SELECT DISTINCT " +
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
                                  "LEFT JOIN Building ON Building.Company_ID = Company.Company_ID";
            companyList = new DatabaseList(companyQuery);

            string buildingQuery = "SELECT Building_ID, Address, state, Active FROM Building";
            buildingList = new DatabaseList(buildingQuery);

            this.PopulateListboxes();

            // Select the first item in the Company List to trigger the fields to fill with data.
            if (this.lbx_CompanyList.Items.Count > 0)
            {
                this.lbx_CompanyList.SelectedIndex = 0;
            }

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

            // Check to see which tab is active, so we don't waste time refreshing data that isn't visible
            if (this.tab_BuildingCompanySelector.SelectedTab == this.tab_ByCompany)
            {
                this.lbx_CompanyList.Items.Clear();
                this.lbx_CompanyList.Items.AddRange(companyList.GetFilteredList(
                                                                                dc_md_both,
                                                                                this.txt_FilterCompany.Text,
                                                                                !this.cbx_ShowInactive.Checked));
            }
            else
            {
                this.lbx_BuildingList.Items.Clear();
                this.lbx_BuildingList.Items.AddRange(buildingList.GetFilteredList(
                                                                                  dc_md_both,
                                                                                  this.txt_FilterAddress.Text,
                                                                                  !this.cbx_ShowInactive.Checked));
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

            // Quit immediately if the user didn't actually select something.
            if (currentLbx.SelectedIndex == -1)
            {
                return;
            }

            // Check which listbox was selected
            if (currentLbx.Name == "lbx_CompanyList")
            {
                selected_id = companyList.GetItemID(currentLbx.SelectedItem.ToString());
                this.currentlySelectedCompany = new Company(selected_id);
                this.PopulateFields(this.currentlySelectedCompany);
            }

            if (currentLbx.Name == "lbx_BuildingList" || currentLbx.Name == "lbx_OtherCompanyBuildings")
            {
                selected_id = buildingList.GetItemID(currentLbx.SelectedItem.ToString());
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

            if (this.lbx_OtherCompanyBuildings.Items.Count > 0)
            {
                this.lbx_OtherCompanyBuildings.SelectedIndex = 0;
            }
            else
            {
                this.PopulateFields(new Building());
            }
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

            this.cbo_Contractor.Text = selected_building.Contractor;
            this.cbx_BuildingActive.Checked = selected_building.Active;

            // Populate the elevator list box.
            this.lbx_ElevatorList.Items.Clear();
            foreach (Elevator elev in selected_building.ElevatorList)
            {
                this.lbx_ElevatorList.Items.Add(elev.ElevatorNumber);
            }
            
            // If there are no elevators, disable the add inspection button
            if (selected_building.ElevatorList.Count == 0)
            {
                this.btn_EnterNewInspection.Enabled = false;
            }
            else
            {
                this.btn_EnterNewInspection.Enabled = true;
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

                if (historyitem.Status != "No Inspection")
                {
                    this.lvw_InspectionList.Items.Add(i);
                }
            }

            // Populate the Building Contact List
            this.lbx_BuildingContacts.Items.Clear();
            foreach (Contact c in selected_building.ContactList)
            {
                this.lbx_BuildingContacts.Items.Add(c.Name);
            }

            // Populate the Company fields, now that we know the building and, by extension, it's owner
            // Assuming that the selected company isn't already the correct one.
            if (selected_building.Owner.Name != this.txt_CompanyName.Text && selected_building.Street != null)
            {
                this.PopulateFields(selected_building.Owner);
            }

            if (selected_building.Street != null)
            {
                this.lbx_OtherCompanyBuildings.SelectedItem = selected_building.Street;
            }
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
                reportFileMenu = new ContextMenu(new MenuItem[] 
                { 
                    new MenuItem("No Report Associated"),
                    new MenuItem("Locate A Report", this.ReportFileMenu_LocateReport)
                });
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
                try
                {
                    if (!row["Report"].ToString().Contains(@"C:") && Directory.Exists(Properties.Settings.Default.ReportLocation))
                    {
                        Process.Start(Properties.Settings.Default.ReportLocation + row["Report"].ToString());
                    }
                    else
                    {
                        Process.Start(row["Report"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private void ReportFileMenu_LocateReport(object sender, EventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Title = string.Format("Locate Report for {0} on {1}", this.lvw_InspectionList.SelectedItems[0].SubItems[1].Text, this.lvw_InspectionList.SelectedItems[0].Text);
            OFD.DefaultExt = ".pdf";
            OFD.InitialDirectory = Properties.Settings.Default.ReportLocation;
            if (OFD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string query = string.Format("SELECT Inspection.Inspection_ID, Elevator.Elevator_ID, Date, IType_ID, Status, Inspector, Report FROM Inspection " +
                                             "JOIN Elevator ON Elevator.Elevator_ID = Inspection.Elevator_ID " +
                                             "JOIN Building ON Building.Building_ID = Elevator.Building_ID " +
                                             "WHERE Date = '{0}' " +
                                             "AND IType_ID = (SELECT IType_ID FROM InspectionTypes WHERE Name = '{1}') " +
                                             "AND Building.Building_ID = {2}",
                                             this.lvw_InspectionList.SelectedItems[0].Text, // Date
                                             this.lvw_InspectionList.SelectedItems[0].SubItems[1].Text, // Type
                                             this.currentlySelectedBuilding.ID);
                // Update every inspection entry with the same date, type, and building ID
                foreach (DataRow inspectionToUpdate in SQL.Query.Select(query).Rows)
                {
                    Inspection toUpdate = new Inspection(inspectionToUpdate);
                    toUpdate.ReportFile = OFD.FileName;
                    toUpdate.CommitToDatabase();
                }
                // Fire the populate method to re-draw all of the information, including that we now have a report file associated.
                this.PopulateFields(this.currentlySelectedBuilding);
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
            this.txt_OfficePhone.Text = this.currentltSelectedContact.OfficePhone.ToString();
            this.txt_CellPhone.Text = this.currentltSelectedContact.CellPhone.ToString();
            this.txt_Fax.Text = this.currentltSelectedContact.Fax.ToString();
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
            Control editButton = (from b in buttonParent.Controls.Cast<Control>()
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
                        this.currentlySelectedBuilding.Contractor = this.cbo_Contractor.Text;

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
                editContact.MenuItems.Add("Modify Contact");
                editContact.MenuItems[2].Click += this.AddContact;
                editContact.MenuItems.Add("Remove Contact");
                editContact.MenuItems[3].Click += this.RemoveContact;

                // If the sender doesn't actually have anything selected, we can't remove/edit it, so gray that option out.
                if (((ListBox)sender).SelectedItem == null)
                {
                    editContact.MenuItems[2].Enabled = false;
                    editContact.MenuItems[3].Enabled = false;
                }

                ((ListBox)sender).ContextMenu = editContact;
            }
        }

        /// <summary>
        /// Spawns an "Add New Contact" form.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Any Event Args.</param>
        private void AddContact(object sender, EventArgs e)
        {
            FormAddNewContact addContact = new FormAddNewContact();

            switch (((MenuItem)sender).Text)
            {
                case "Add New Contact":
                    {
                        addContact = new FormAddNewContact(this.currentlySelectedCompany, this.currentlySelectedBuilding);
                        break;
                    }

                case "Add Existing Contact":
                    {
                        throw new NotImplementedException("Add Existing Contact Not Supported");
                    }

                case "Modify Contact":
                    {
                        addContact = new FormAddNewContact(this.currentltSelectedContact);
                        break;
                    }
            }

            addContact.ShowDialog();
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
        /// Opens a child form based on which button was pressed.
        /// </summary>
        /// <param name="sender"> The button which was clicked.</param>
        /// <param name="e">Any Event Args.</param>
        private void OpenChildForm(object sender, EventArgs e)
        {
            string sender_name = string.Empty;
            if (sender is Control)
            {
                sender_name = ((Control)sender).Name;
            }
            else if (sender is ToolStripItem)
            {
                sender_name = ((ToolStripItem)sender).Name;
            }

            switch (sender_name)
            {
                case "btn_EnterNewInspection":
                    {
                        FormAddInspection newInspection = new FormAddInspection(this.txt_BuildingAddress.Text);
                        newInspection.ShowDialog();
                        this.PopulateFields(this.currentlySelectedBuilding);
                        break;
                    }

                case "btn_AddNewCompany":
                    {
                        FormAddNewCompany newCompany = new FormAddNewCompany();
                        try
                        {
                            if (newCompany.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                // If the Dialog result is "OK", then the user saved a new company and we should refresh anything to do with it.
                                companyList.Regenerate();
                                this.PopulateListboxes();

                                // Also select this new company in the company list box
                                this.tab_BuildingCompanySelector.SelectedTab = this.tab_ByCompany;
                                this.txt_FilterCompany.Text = newCompany.NewCompanyName;
                                this.lbx_CompanyList.SelectedItem = this.txt_FilterCompany.Text;
                            }
                        }
                        catch (SQLDuplicateEntryException ex)
                        {
                            // If the user attempted to enter in a company which the SQL Server rejected as a duplicate, catch the exception here.
                            // Alert the user of the error
                            MessageBox.Show(((Company)ex.FailedObject).Name + " already exists in the database", "Duplicate Company Entry", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                            // Make sure tha company tab is active and select the duplicate value in the list
                            this.tab_BuildingCompanySelector.SelectedTab = this.tab_ByCompany;
                            this.lbx_CompanyList.SelectedItem = ((Company)ex.FailedObject).Name;
                        }
                        catch (Exception)
                        {
                            throw;
                        }

                        break;
                    }

                case "btn_NewBuilding":
                    {
                        FormAddNewBuilding newBuilding = new FormAddNewBuilding(this.txt_CompanyName.Text);
                        try
                        {
                            if (newBuilding.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                // If the Dialog result is "OK", then the user saved a new building and we should refresh anything to do with it.
                                buildingList.Regenerate();
                                this.PopulateListboxes();

                                // If the active tab is the By Company tab, we should select the newly added building in the "Other Buildings" listbox
                                if (this.tab_BuildingCompanySelector.SelectedTab == this.tab_ByCompany)
                                {
                                    this.lbx_OtherCompanyBuildings.SelectedItem = newBuilding.NewBuildingAddress;
                                }
                                else if (this.tab_BuildingCompanySelector.SelectedTab == this.tab_ByBuilding)
                                {
                                    this.lbx_BuildingList.SelectedItem = newBuilding.NewBuildingAddress;
                                }
                            }
                        }
                        catch (SQLDuplicateEntryException ex)
                        {
                            // If the user attempted to enter in a building address which the SQL Server rejected as a duplicate, catch the exception here
                            MessageBox.Show(((Building)ex.FailedObject).Street + " already exists in the database", "Duplicate Building Entry", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                            // Make sure the building tab is active and select the duplicate value from the list
                            this.tab_BuildingCompanySelector.SelectedTab = this.tab_ByBuilding;
                            this.lbx_BuildingList.SelectedItem = ((Building)ex.FailedObject).Street;
                        }
                        catch (Exception)
                        {
                            throw;
                        }

                        break;
                    }

                case "btn_NewElevator":
                    {
                        try
                        {
                            FormAddNewElevator newElevator = new FormAddNewElevator(this.currentlySelectedBuilding);
                            newElevator.ShowDialog();
                            this.PopulateFields(this.currentlySelectedBuilding);
                        }
                        catch (SQLDuplicateEntryException ex)
                        {
                            // If we get a duplicate entry error, alert the user
                            MessageBox.Show(string.Format("An Elevator with the number {0} already exists in the database.", ((Elevator)ex.FailedObject).ElevatorNumber), "Duplicate Elevator Number", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                            // Possibly we should either tell the user what building has the elevator number they are using? or if they would like to navigate to that building?
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }

                        break;
                    }

                case "mnu_Preferances":
                    {
                        FormPreferances preferances = new FormPreferances();
                        preferances.ShowDialog();
                        break;
                    }

                default: throw new NotImplementedException(sender_name + " is not provided for in the OpenChildForm() method.");
            }
        }

        /// <summary>
        /// Occurs when the main control tab is changed. Used to load form data when desired, rather than at load time.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Not used by this method.</param>
        private void MainTabChanged(object sender, EventArgs e)
        {
            if (this.tab_MainTabControl.SelectedTab == this.tab_UpcomingAndOverdue)
            {
                #region Overdue Inspection Query
                string overdue_query =
                    "SELECT DISTINCT Address, DATEDIFF(DAY, Inspection.Date, GetDate()) as DaysPast, InspectionTypes.Name, Inspection.Status " +
                    "FROM Inspection " +
                    "JOIN Elevator ON Elevator.Elevator_ID = Inspection.Elevator_ID " +
                    "JOIN Building ON Elevator.Building_ID = Building.Building_ID " +
                    "JOIN InspectionTypes ON Inspection.IType_ID = InspectionTypes.IType_ID " +
                    "WHERE Inspection_ID IN " +
                    "    (SELECT TOP 1 Inspection_ID " +
                    "    FROM Inspection  " +
                    "    JOIN Elevator AS Dupe ON Elevator.Elevator_ID = Inspection.Elevator_ID " +
                    "    WHERE Dupe.Elevator_ID = Elevator.Elevator_ID " +
                    "    ORDER BY Date DESC) " +
                    "AND Status <> 'Clean'  " +
                    "AND Status <> 'No Inspection' " +
                    "AND DATEDIFF(DAY, Inspection.Date, GetDate()) > 30 " +
                    "AND Active <> 0 " + 
                    "ORDER BY DaysPast DESC";
                #endregion

                this.lvw_OverdueInspections.Items.Clear();
                foreach (DataRow row in SQL.Query.Select(overdue_query).Rows)
                {
                    ListViewItem item = new ListViewItem(row["Address"].ToString());
                    item.SubItems.Add(row["DaysPast"].ToString());
                    item.SubItems.Add(row["Name"].ToString());
                    item.SubItems.Add(row["Status"].ToString());

                    this.lvw_OverdueInspections.Items.Add(item);
                }
            }
        }
    }
}
