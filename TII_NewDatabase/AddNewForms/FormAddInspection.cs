﻿// <summary> Creates a form able to insert inspection information into the database. </summary>

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
        /// <param name="inspectionDate">The date of the inspection to load inspetion data from.</param>
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
            string query = string.Format("SELECT * FROM Inspection " +
                                         "WHERE Elevator_ID IN " +
                                         "    ( " +
                                         "    SELECT Elevator_ID " +
                                         "    FROM Building " +
                                         "    JOIN Elevator ON Elevator.Building_ID = Building.Building_ID " +
                                         "    WHERE Address = '{0}' " +
                                         "    ) " +
                                         "AND Date = '{1}' " +
                                         "AND IType_ID = " +
                                         "    ( " +
                                         "    SELECT IType_ID" +
                                         "    FROM InspectionTypes" +
                                         "    WHERE Name = '{2}'" +
                                         "    )", building_address, inspectionDate, inspectionType);

            List<Inspection> inspectionList = new List<Inspection>();

            foreach (DataRow row in SQL.Query.Select(query).Rows)
            {
                inspectionList.Add(new Inspection(row));
            }

            // Set the inspector's name.
            // We can just use the first result in the inspection list, since we only have a single inspector working on any site on a day
            this.cbo_Inspector.Text = inspectionList[0].InspectorName;

            // Same goes for the report file
            this.lbx_ReportFileList.Items.Add(inspectionList[0].ReportFile);

            // Set the Inspection Status for each elevator
            foreach (DataGridViewRow i in this.dgv_ElevatorList.Rows)
            {
                i.Cells["Status"].Value = inspectionList.Where(x => x.ElevatorNumber == i.Cells["Elevator Number"].Value.ToString()).Single().Status;
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

            // Populate the elevator list with the number and nickname of each unit
            foreach (Elevator elev in this.selectedBuilding.ElevatorList)
            {
                this.dgv_ElevatorList.Rows.Add(elev.ElevatorNumber, elev.Nickname, elev.ElevatorType, string.Empty);
                this.dict_ElevatorIDs.Add(elev.ElevatorNumber, elev.ID.Value);
            }      
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
                        // This is the form initializing itself.
                        this.ValidateInformation(this.dtp_InspectionDate, EventArgs.Empty);
                        this.ValidateInformation(this.dgv_ElevatorList, EventArgs.Empty);
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
                            this.error_provider.SetError(this.dtp_InspectionDate, string.Empty);
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

                case "cbo_InspectionType":
                    {
                        break;
                    }

                case "cbo_Inspector":
                    {
                        break;
                    }

                case "lbx_ReportFileList":
                    {
                        break;
                    }

                default: throw new Exception("How did you get here?");
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

            string formattedReportFile = string.Empty;

            try
            {
                // If the setting for moving a saving reports has been enabled
                if (Properties.Settings.Default.MoveAndSaveReports && this.lbx_ReportFileList.Items.Count != 0)
                {
                    formattedReportFile = Properties.Settings.Default.ReportLocation + this.FormatReportFilename();

                    // If there are multiple files in the list box, combine them in the order in which they appear
                    if (this.lbx_ReportFileList.Items.Count > 1)
                    {
                        string[] files = new string[this.lbx_ReportFileList.Items.Count];
                        for (int i = 0; i < files.Length; i++)
                        {
                            files[i] = this.lbx_ReportFileList.Items[i].ToString();
                        }

                        ////if (!File.Exists(FormattedReportFile))
                        ////{

                        ////}
                        PDF.CombineFiles(files, formattedReportFile);
                    }
                    else
                    {
                        if (!File.Exists(formattedReportFile))
                        {
                            // Check to be sure that the folder for this inspector exists.
                            if (!Directory.Exists(Path.GetDirectoryName(formattedReportFile)))
                            {
                                if (MessageBox.Show(string.Format("Folder for {0} does not exist. Create?", this.cbo_Inspector.Text), "Create Missing Folder?", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.OK)
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(formattedReportFile));
                                }
                            }
                            
                            File.Copy(this.lbx_ReportFileList.Items[0].ToString(), formattedReportFile);
                        }
                        else
                        {
                            MessageBox.Show("A report already exists in " + formattedReportFile, "Duplicate Report", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                }
                               
                //// If enabled and we have a valid file in the textbox, use the Format provided to rename the report file 
                //// with the information entered and make a copy in the report folder. 
                ////if (Properties.Settings.Default.MoveAndSaveReports && File.Exists(this.txt_ReportFile.Text))
                ////{
                ////    string filename = FormatReportFilename();

                ////    File.Copy(this.txt_ReportFile.Text, Properties.Settings.Default.ReportLocation + filename);
                ////    this.txt_ReportFile.Text = filename;
                ////}
                
                foreach (DataGridViewRow elev in this.dgv_ElevatorList.Rows)
                {
                    // If the elevator is marked as Not Inspected, there is no reason to make a note of it for the database.
                    if (elev.Cells["Status"].Value.ToString() != "Not Inspected")
                    {
                        Inspection newInspection = new Inspection();
                        newInspection.ElevatorID = this.dict_ElevatorIDs[elev.Cells["Elevator Number"].Value.ToString()];
                        newInspection.Date = this.dtp_InspectionDate.Value;
                        newInspection.InspectionType = this.cbo_InspectionType.Text;
                        newInspection.Status = elev.Cells["Status"].Value.ToString();
                        newInspection.InspectorName = this.cbo_Inspector.Text;
                        newInspection.ReportFile = formattedReportFile;

                        success = success && newInspection.CommitToDatabase();
                    }
                }

                if (success)
                {
                    MessageBox.Show("Inspection Added Successfully", "Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
            catch (Exception ex)
            {
                throw ex;
            }
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
            if (File.Exists("TempCert.pdf"))
            {
                File.Delete("TempCert.pdf");
            }
            
            PDF_Document newCert = new PDF_Document("TempCert.pdf");
            newCert.AddImage(@"C:\Users\Jon\Documents\TPIRs\DC\CleanCert_blank.jpg");

            // Third Party Agency
            newCert.AddText("Technical Inspection of D.C. Inc.", 180, 713);

            // Date
            newCert.AddText(DateTime.Now.ToString("MMMM dd, yyyy"), 470, 713);

            // Elevator Professional In Charge
            newCert.AddText("Anthony Vattimo, Jr.", 200, 690);

            // QEI#
            newCert.AddText("S-171", 500, 690);

            // Name of Inspector
            newCert.AddText(this.cbo_Inspector.Text, 135, 667);

            // QEI#
            newCert.AddText("C3690", 500, 667);

            // Unit #(s)
            List<string> unitnos = new List<string>();
            foreach (DataGridViewRow elev in this.dgv_ElevatorList.Rows)
            {
                unitnos.Add(elev.Cells["Nickname"].Value.ToString());
            }

            newCert.AddText("#" + unitnos.ToFormattedList(), 90, 644);

            // DCRA Certificate(s)
            List<string> certnos = new List<string>();
            foreach (DataGridViewRow elev in this.dgv_ElevatorList.Rows)
            {
                certnos.Add(elev.Cells["Elevator Number"].Value.ToString());
            }

            // The first seven certificate numbers can fit on the first line. Any number after than but less than 16 can go on the second.
            if (certnos.Count <= 7)
            {
                newCert.AddText(certnos.ToFormattedList(), 145, 621);
            }
            else if (certnos.Count >= 8 && certnos.Count <= 16)
            {
                newCert.AddText(certnos.GetRange(0, 7).ToFormattedList(false), 145, 621);
                newCert.AddText(certnos.GetRange(7, certnos.Count - 7).ToFormattedList(), 45, 598);
            }
            else
            {
                MessageBox.Show("Cannot create a cert with more than 16 units", "Too Many Units", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                newCert.ClosePDF();
                return;
            }
            
            // Project Address
            newCert.AddText("11 Dupont Circle, NW, Washington, DC 20036", 125, 575);

            // Project Name
            newCert.AddText("11 Dupont Circle, NW", 160, 552);

            // Inspection Discipline
            newCert.AddText("X", 195, 529);

            // Inspection Type
            // Periodic - will always be selected
            newCert.AddText("X", 195, 505);

            // Category 1
            if (this.cbo_InspectionType.Text.Contains("Category 1"))
            {
                newCert.AddText("X", 375, 505);
            }

            // Category 5
            if (this.cbo_InspectionType.Text.Contains("Category 5"))
            {
                newCert.AddText("X", 375, 493);
            }

            // FES
            if (this.selectedBuilding.FireEmergencyService)
            {
                newCert.AddText("X", 195, 493);
            }

            // Heat devices
            if (this.selectedBuilding.HeatDetectors)
            {
                newCert.AddText("X", 195, 482);
            }
            
            // Emergency Power
            if (this.selectedBuilding.EmergencyPower)
            {
                newCert.AddText("X", 375, 482);
            }

            // Elevator Professional In Charge
            newCert.AddText("Anthony Vattimo, Jr.", 100, 410);

            // Third Party Company Name
            newCert.AddText("Technical Inspection of D.C. Inc.", 90, 371);

            // Code year
            newCert.AddText("2013", 105, 269);

            newCert.ClosePDF();
        }
    }
}
