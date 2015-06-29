// <summary> Creates a form able to insert inspection information into the database. </summary>

namespace TII_NewDatabase.AddNewForms
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;

    using Database;
    using PDF_Library;
    using SQL;

    /// <summary> Form for entering inspection information. </summary>
    public partial class FormAddInspection : Form
    {
        /// <summary> The Report Format to use in the report folder. </summary>
        private const string REPORTFILE_FORMAT = @"\{ST}\{Inspector Name}\{YYMMDD} - {InspType} - {Address}.pdf";
        
        /// <summary> Holds the building information from the selected building. </summary>
        private Building selectedBuilding;

        /// <summary>
        /// Dictionary that holds the relations between elevator numbers and their database assigned IDs. 
        /// Prevents us from having to re-query for this information for each insertion when it is available when we load them for the first time.
        /// </summary>
        private Dictionary<string, int> dict_ElevatorIDs;

        /// <summary> A list of the inspections to be added to the database. </summary>
        private List<Inspection> inspectionList = new List<Inspection>();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FormAddInspection"/> class.
        /// </summary>
        public FormAddInspection()
        {
            this.InitializeComponent();

            this.ValidateInformation(this, EventArgs.Empty);
            this.cbo_SetAllInspections.Items.AddRange(Inspection.Statuses);

            // Set the Inspector Combo box to display all active inspectors in alphabetical order
            this.cbo_Inspector.Items.AddRange(Inspection.GetInspectors(true));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormAddInspection"/> class.
        /// </summary>
        /// <param name="building_address">The building address to initialize the form with. </param>
        public FormAddInspection(string building_address)
            : this()
        {
            this.txt_BuildingFilter.Text = building_address;
            if (this.lbx_BuildingList.Items.Count > 0)
            {
                this.lbx_BuildingList.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormAddInspection"/> class.
        /// </summary>
        /// <param name="building_address">The building address to initialize the form with. </param>
        /// <param name="inspectionDate">The date of the inspection to load inspection data from.</param>
        /// <param name="inspectionType">The type of the inspection to load inspection data from.</param>
        public FormAddInspection(string building_address, DateTime inspectionDate, string inspectionType)
            : this(building_address)
        {
            // Set the date, since that was passed in
            this.dtp_InspectionDate.Checked = true;
            this.dtp_InspectionDate.Value = inspectionDate;

            // Set the inspection type, since that was passed in as well
            this.cbo_InspectionType.Text = inspectionType;

            // Query to select all inspections from this building, on this date, of this type
            string query = string.Format(
                "SELECT Inspection.ID, Elevator_ID, Date, InspectionType.Name AS Type, Clean, Inspector.Name AS Inspector, Documents.FilePath AS Report " +
                "FROM Inspection " +
                "JOIN Inspector ON Inspector_ID = Inspector.ID " +
                "LEFT JOIN InspectionType ON InspectionType_ID = InspectionType.ID " +
                "LEFT JOIN Documents ON Report_ID = Documents.ID " +
                "WHERE Elevator_ID IN " +
                "( " +
                "    SELECT Elevator_ID " +
                "    FROM Building " +
                "    JOIN Elevator ON Elevator.Building_ID = Building.ID " +
                "    JOIN Address ON Building.Address_ID = Address.ID " +
                "    WHERE Street = '{0}' " +
                ") " +
                "AND Date = '{1}' " +
                "AND InspectionType.Name = '{2}'",
                building_address,
                inspectionDate,
                inspectionType);

            // Zero out the Inspection list. (Selecting a building in the FormAddInspection(Building) Constructor was populating the inspection list with blank inspections.)
            this.inspectionList = new List<Inspection>();
            foreach (DataRow row in SQL.Query.Select(query).Rows)
            {
                this.inspectionList.Add(new Inspection(row));
            }

            // Set the inspector's name.
            // We can just use the first result in the inspection list, since we only have a single inspector working on any site on a day
            this.cbo_Inspector.Text = this.inspectionList[0].InspectorName;

            // Same goes for the report file
            if (this.inspectionList[0].ReportFile != string.Empty)
            {
                this.lbx_ReportFileList.Items.Add(this.inspectionList[0].ReportFile);
            }

            // Set the Inspection Status for each elevator
            foreach (DataGridViewRow i in this.dgv_ElevatorList.Rows)
            {
                i.Cells["Status"].Value = this.inspectionList.Where(x => x.ElevatorNumber == i.Cells["Elevator Number"].Value.ToString()).Single().Status;
            }

            // Disable the Address filter box and list
            this.txt_BuildingFilter.Enabled = false;
            this.lbx_BuildingList.Enabled = false;

            // Change the Submit button so it's clear that we're in edit mode
            this.btn_SubmitInspection.Text = "Submit Revised Inspection";
        }

        /// <summary>
        /// Method that occurs once as the form loads as it is being displayed to the user.
        /// </summary>
        /// <param name="sender">The Sender.</param>
        /// <param name="e">Any Event Args.</param>
        private void OnLoad(object sender, EventArgs e)
        {
            // Fire the Filter changed Method to load the form, either based on the address passed in or empty, to load a full list.
            this.Filter_Changed(new object(), EventArgs.Empty);
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

        /// <summary>
        /// Method which occurs once the user has selected a building from the list box.
        /// </summary>
        /// <param name="sender">The Sender.</param>
        /// <param name="e">Any Event Args.</param>
        private void BuildingSelected(object sender, EventArgs e)
        {
            // If there is no selected item, bail out immediatly.
            if (this.lbx_BuildingList.SelectedIndex == -1)
            {
                return;
            }

            this.selectedBuilding = new Building(Main_Form.BuildingList.GetItemID(this.lbx_BuildingList.SelectedItem.ToString()));
            this.dgv_ElevatorList.Columns.Clear();

            // Set up the Inspection Type Combo box with the correct inspection type information
            this.cbo_InspectionType.Items.Clear();
            string[] inspectionTypes;
            if (this.selectedBuilding.County == "Washington D.C.")
            {
                inspectionTypes = Inspection.GetInspectionTypes("DC");
                this.btn_CreateDCCert.Visible = true;
            }
            else
            {
                inspectionTypes = Inspection.GetInspectionTypes("MD");
                this.btn_CreateDCCert.Visible = false;
            }

            this.cbo_InspectionType.Items.AddRange(inspectionTypes);
                
            // Add the Elevator Number Column
            DataGridViewColumn col_ElevNum = new DataGridViewTextBoxColumn();
            col_ElevNum.Name = "Elevator Number";
            col_ElevNum.Width = 80;
            this.dgv_ElevatorList.Columns.Add(col_ElevNum);

            // Add a Nickname Column
            DataGridViewColumn col_ElevNick = new DataGridViewTextBoxColumn();
            col_ElevNick.Name = "Nickname";
            col_ElevNick.Width = 60;
            this.dgv_ElevatorList.Columns.Add(col_ElevNick);

            // Add a Nickname Column
            DataGridViewColumn col_ElevType = new DataGridViewTextBoxColumn();
            col_ElevType.Name = "Type";
            col_ElevType.Width = 60;
            this.dgv_ElevatorList.Columns.Add(col_ElevType);

            // Add a status dropdown column
            DataGridViewComboBoxColumn col_ElevStatus = new DataGridViewComboBoxColumn();
            col_ElevStatus.Name = "Status";
            col_ElevStatus.Items.AddRange(Inspection.Statuses);
            this.dgv_ElevatorList.Columns.Add(col_ElevStatus);

            // Reset the Elevator ID Dictionary
            this.dict_ElevatorIDs = new Dictionary<string, int>();

            // Create a new set of Inspections
            this.inspectionList = new List<Inspection>();

            // Populate the elevator list with the number and nickname of each unit
            foreach (Elevator elev in this.selectedBuilding.ElevatorList)
            {
                this.dgv_ElevatorList.Rows.Add(elev.ElevatorNumber, elev.Nickname, elev.ElevatorType, string.Empty);
                this.dict_ElevatorIDs.Add(elev.ElevatorNumber, elev.ID.Value);
                
                // Create a new inspection for each elevator, to be updated as the form is filled out
                Inspection i = new Inspection();
                i.ElevatorID = elev.ID.Value;
                this.inspectionList.Add(i);
            }

            // Validate the Data Grid View
            this.ValidateInformation(this.dgv_ElevatorList, EventArgs.Empty);
        }

        /// <summary>
        /// Upon selecting an item in the Set All Inspections combo box, the selected text will be populated in the status column for every elevator.
        /// </summary>
        /// <param name="sender">The Sender.</param>
        /// <param name="e">Any Event Args.</param>
        private void SetAllInspections(object sender, EventArgs e)
        {            
            foreach (DataGridViewRow row in this.dgv_ElevatorList.Rows)
            {
                row.Cells["Status"].Value = this.cbo_SetAllInspections.Text;
            }
        }

        /// <summary>
        /// After each piece of information is entered, check to make sure it is valid, and if all pieces are present and valid, enable the submit button.
        /// </summary>
        /// <param name="sender">The Control which was just left.</param>
        /// <param name="e">Any Event Args.</param>
        private void ValidateInformation(object sender, EventArgs e)
        {
            /* Things we need to know are complete before we enable the Submit button:
             * 1 - Every elevator in the list has a status assigned.
             * 2 - The date has been changed and is valid for the inspection.
             * 3 - There is a valid Inspection Type assigned.
             * 4 - There is a valid Inspector assigned.
            */        

            switch (((Control)sender).Name)
            {
                case "FormAddInspection":
                    {
                        // This is the form initializing itself. Validate all of our controls so errors appear which must be cleared
                        this.ValidateInformation(this.dtp_InspectionDate, EventArgs.Empty);
                        this.ValidateInformation(this.cbo_InspectionType, EventArgs.Empty);
                        this.ValidateInformation(this.cbo_Inspector, EventArgs.Empty);
                        break;
                    }
                
                case "dtp_InspectionDate":
                    {
                        if (this.dtp_InspectionDate.Value > DateTime.Now)
                        {
                            this.error_provider.SetError(this.dtp_InspectionDate, "Date must be no later than today");
                        }
                        else if (!this.dtp_InspectionDate.Checked)
                        {
                            this.error_provider.SetError(this.dtp_InspectionDate, "Please select a date for the inspection");
                        }
                        else
                        {
                            // If no error was found, remove the error symbol from the control
                            this.error_provider.SetError(this.dtp_InspectionDate, string.Empty);

                            // Update all of the inspections in the list to have the new date
                            foreach (Inspection i in this.inspectionList)
                            {
                                i.Date = this.dtp_InspectionDate.Value;
                            }
                        }

                        break;
                    }

                case "dgv_ElevatorList":
                    {
                        bool allStatusComplete = true;
                        
                        foreach (DataGridViewRow row in this.dgv_ElevatorList.Rows)
                        {
                            if (!Inspection.Statuses.Contains(row.Cells["Status"].Value.ToString()))
                            {
                                allStatusComplete = false;
                            }

                            // As long as we're rolling through each row, update each inspection with the value that's currently set
                            // Select from the inspection List where the ID is equal to the ID we get from the dictionary for this elevator number
                            // Set the status of the one inspection we get from the query (there should only be one) to whatever is in the data grid view row
                            int elevatorID = this.dict_ElevatorIDs[row.Cells["Elevator Number"].Value.ToString()];
                            this.inspectionList.Where(x => x.ElevatorID == elevatorID).Single().Status = row.Cells["Status"].Value.ToString();
                        }

                        if (!allStatusComplete)
                        {
                            this.error_provider.SetError(this.dgv_ElevatorList, "Must set a status for each elevator");
                        }
                        else
                        {
                            this.error_provider.SetError(this.dgv_ElevatorList, string.Empty);
                        }

                        break;
                    }

                case "dgv_InspectionList_Cell":
                    {
                        this.ValidateInformation(this.dgv_ElevatorList, EventArgs.Empty);
                        break;
                    }

                case "cbo_InspectionType":
                    {
                        // Make sure the inspection type is not empty
                        if (this.cbo_InspectionType.Text == string.Empty)
                        {
                            this.error_provider.SetError(this.cbo_InspectionType, "Must enter an inspection type");
                            break;
                        }
                        
                        // Make sure the inspection type that is in the box is one that is included in the type list
                        if (!this.cbo_InspectionType.Items.Contains(this.cbo_InspectionType.Text))
                        {
                            this.error_provider.SetError(this.cbo_InspectionType, "Must be a valid inspection type from the list");
                            break;
                        }

                        // Update all of our inspections and remove the error if the value meets all of our criteria

                        // Set the inspection type for all inspections in our list to be equal to what we've set in the combo box
                        foreach (Inspection i in this.inspectionList)
                        {
                            i.InspectionType = this.cbo_InspectionType.Text;
                        }

                        // Remove the error
                        this.error_provider.SetError(this.cbo_InspectionType, string.Empty);
                        break;
                    }

                case "cbo_Inspector":
                    {
                        // Make sure there is an inspector set
                        if (this.cbo_Inspector.Text == string.Empty)
                        {
                            this.error_provider.SetError(this.cbo_Inspector, "Must enter an inspector's name");
                            break;
                        }

                        // Make sure the inspector is a valid name from the inspector list
                        if (!Inspection.GetInspectors().Contains(this.cbo_Inspector.Text))
                        {
                            this.error_provider.SetError(this.cbo_Inspector, "Must be an inspector who appears in the database");
                            break;
                        }
                        
                        // Update all of our inspections and remove the error if the value meets all of our criteria

                        // Set the inspector for all inspections in our list to be what we've set in the combo box
                        foreach (Inspection i in this.inspectionList)
                        {
                            i.InspectorName = this.cbo_Inspector.Text;
                        }

                        // Remove the error
                        this.error_provider.SetError(this.cbo_Inspector, string.Empty);
                        break;
                    }

                case "cbo_SetAllInspections":
                    {
                        this.ValidateInformation(this.dgv_ElevatorList, EventArgs.Empty);
                        break;
                    }

                default: throw new Exception("How did you get here?");
            }

            // Check and see if, assuming this is a DC job, we have all of the neseccary information to make a cert. 
            if (this.selectedBuilding != null)
            {
                bool allClean = true;
                foreach (DataGridViewRow elevRow in this.dgv_ElevatorList.Rows)
                {
                    if (elevRow.Cells["Status"].Value.ToString() != "Clean")
                    {
                        allClean = false;
                    }
                }

                // If everything is here, enable the cert button. Otherwise, disable it.
                if (allClean && this.cbo_Inspector.Text != string.Empty && this.dtp_InspectionDate.Checked && this.cbo_InspectionType.Text != string.Empty)
                {
                    this.btn_CreateDCCert.Enabled = true;
                }
                else
                {
                    this.btn_CreateDCCert.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Collect the data on the form and create a new inspection class for each elevator, then commit the data to the database.
        /// </summary>
        /// <param name="sender">The Submit Button.</param>
        /// <param name="e">Any Event Args.</param>
        private void SubmitInspection(object sender, EventArgs e)
        {
            bool success = true;

            try
            {
                // If the setting for moving a saving reports has been enabled and the user has added at least one report
                if (Properties.Settings.Default.MoveAndSaveReports && this.lbx_ReportFileList.Items.Count != 0)
                {
                    this.MoveAndSaveReport();
                }

                foreach (Inspection i in this.inspectionList)
                {
                    // If there is at least one item in the reports, set the report to whatever is in the listbox at the moment.
                    if (this.lbx_ReportFileList.Items.Count > 0)
                    {
                        i.ReportFile = this.lbx_ReportFileList.Items[0].ToString();
                    }

                    // If the elevator wasn't inspected, there's no reason to make a note of it for the database.
                    if (i.Status != "No Inspection")
                    {
                        success = success && i.CommitToDatabase();
                    }
                }

                if (success)
                {
                    string message = string.Empty;
                    if (this.btn_SubmitInspection.Text == "Submit Inspection")
                    {
                        message = "Inspection Added Successfully";
                    }
                    else
                    {
                        message = "Inspection Updated Successfully";
                    }

                    MessageBox.Show(message, "Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Clear all the fields to reset the form.
                    this.ResetForm();
                }
                else
                {
                    MessageBox.Show("Something has gone wrong...", "SQL Failure", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
            catch (SQLDuplicateEntryException)
            {
                MessageBox.Show(string.Format("{0} inspection already exists for this building on {1}", this.cbo_InspectionType.Text, this.dtp_InspectionDate), "Duplicate Inspection", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                this.ResetForm();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Moves the report added in the list box to the location stored in settings and designated by the Report Format.
        /// If there are more than one file in the list box, combines them into a single file before moving them.
        /// </summary>
        private void MoveAndSaveReport()
        {
            string formattedReportFile = Properties.Settings.Default.ReportLocation + this.FormatReportFilename();

            // Check to see if this file already exists
            if (File.Exists(formattedReportFile))
            {
                if (MessageBox.Show("A report with this filename already exists. Overwrite?", "Report already exists", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.OK)
                {
                    File.Delete(formattedReportFile);
                }
                else
                {
                    return;
                }
            }
            
            // If there are multiple files in the list box, combine them in the order in which they appear
            if (this.lbx_ReportFileList.Items.Count > 1)
            {
                string[] files = new string[this.lbx_ReportFileList.Items.Count];
                for (int i = 0; i < files.Length; i++)
                {
                    files[i] = this.lbx_ReportFileList.Items[i].ToString();
                }

                PDF.CombineFiles(files, formattedReportFile);
            }
            else
            {
                // Check to be sure that the folder for this inspector exists.
                if (!Directory.Exists(Path.GetDirectoryName(formattedReportFile)))
                {
                    if (MessageBox.Show(string.Format("Folder for {0} does not exist. Create?", this.cbo_Inspector.Text), "Create Missing Folder?", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.OK)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(formattedReportFile));
                    }
                    else
                    {
                        MessageBox.Show("No folder exists for this file - it will not be moved.", "Folder Does Not Exist");
                        return;
                    }
                }

                File.Copy(this.lbx_ReportFileList.Items[0].ToString(), formattedReportFile);
            }

            // Clear the Report Listbox and replace it with the new filename
            this.lbx_ReportFileList.Items.Clear();
            this.lbx_ReportFileList.Items.Add(formattedReportFile);
        }

        /// <summary>
        /// Creates a string from the data in the form.
        /// </summary>
        /// <returns>A form containing data in the provided Report File Format.</returns>
        private string FormatReportFilename()
        {
            string filename = REPORTFILE_FORMAT;
            filename = filename.Replace("{ST}", this.selectedBuilding.State);
            filename = filename.Replace("{Inspector Name}", this.cbo_Inspector.Text);
            filename = filename.Replace("{YYMMDD}", this.dtp_InspectionDate.Value.ToYYMMDDString());
            filename = filename.Replace("{InspType}", Inspection.GetInspectionTypeAbbv(this.cbo_InspectionType.Text));
            filename = filename.Replace("{Address}", this.selectedBuilding.Street);
            return filename;
        }

        /// <summary>
        /// Method for resetting the form back to it's initial state.
        /// </summary>
        private void ResetForm()
        {
            this.dtp_InspectionDate.Value = DateTime.Today;
            this.dtp_InspectionDate.Checked = false;
            this.cbo_InspectionType.Text = string.Empty;
            this.cbo_Inspector.Text = string.Empty;
            this.lbx_ReportFileList.Items.Clear();
            this.cbo_SetAllInspections.Text = string.Empty;
            this.SetAllInspections(new object(), EventArgs.Empty);
        }

        /// <summary>
        /// Method that changes the appearance of the Mouse when the user drags a file over the Report File textbox.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Any Event Args.</param>
        private void ReportFile_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        /// <summary>
        /// Method for grabbing the filename out of the drag drop over the Report File Textbox and putting it into the Textbox.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Any Event Args.</param>
        private void ReportFile_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filenames = (string[])e.Data.GetData(DataFormats.FileDrop);

                this.lbx_ReportFileList.Items.AddRange(filenames);
            }
        }

        /// <summary>
        /// Method to allow the user to browse for the report file, which is then added to the Report File Textbox.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Any Event Args.</param>
        private void AddReport_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.lbx_ReportFileList.Items.Add(System.IO.Path.GetFileName(ofd.FileName));
            }
        }

        /// <summary>
        /// Creates a DC Cert based on the building and inspection information provided.
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void CreateDCCert(object sender, EventArgs e)
        {           
            CreateCert.CleanCert("TempCert.pdf", this.cbo_Inspector.Text, this.selectedBuilding, this.cbo_InspectionType.Text);

            // If we haven't already made a cert, add it to the first position in the list box. 
            if (!this.lbx_ReportFileList.Items.Contains("TempCert.pdf"))
            {
                this.lbx_ReportFileList.Items.Insert(0, "TempCert.pdf");
            }
        }

        /// <summary>
        /// When the elevator list combo box is opened in order to modify the values, we assign the Validate event to that combo box so we can check it.
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void ElevatorList_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            ComboBox cb = e.Control as ComboBox;
            if (cb != null)
            {
                cb.Name = "dgv_InspectionList_Cell";
                cb.SelectedIndexChanged -= this.ValidateInformation; // Otherwise it just keeps adding handlers to the event,
                cb.SelectedIndexChanged += this.ValidateInformation;
            }
        }
    }
}
