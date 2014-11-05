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
        /// <summary> Holds the building information from the selected building. </summary>
        private Building selectedBuilding;

        /// <summary> A string array with the possible elevator statuses. To make things easier if one needs to be added, edited, or removed. </summary>
        private string[] elev_STATUSES = { "Clean", "Outstanding Violations", "Paperwork Needed", "Not Inspected" };
        
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

        /// <summary>
        /// Method which occurs once the user has selected a building from the list box.
        /// </summary>
        /// <param name="sender">The Sender.</param>
        /// <param name="e">Any Event Args.</param>
        private void BuildingSelected(object sender, EventArgs e)
        {
            // If there is no selected item, bail out immediatly.
            if (this.lbx_BuildingList.SelectedIndex == 0)
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
                inspectionTypes = new string[]
                {
                    "Periodic",
                    "Periodic Reinspection",
                    "Category 1 / Periodic",
                    "Category 1 / Periodic Reinspection",
                    "Category 5 / Periodic",
                    "Category 5 / Periodic Reinspection"
                };  
            }
            else
            {
                inspectionTypes = new string[]
                {
                    "Annual",
                    "Reinspection",
                    "Category 5"
                };  
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
            col_ElevStatus.Items.AddRange(this.elev_STATUSES);
            this.dgv_ElevatorList.Columns.Add(col_ElevStatus);

            // Populate the elevator list with the number and nickname of each unit
            foreach (Elevator elev in this.selectedBuilding.ElevatorList)
            {
                this.dgv_ElevatorList.Rows.Add(elev.ElevatorNumber, elev.Nickname, elev.ElevatorType, string.Empty);
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
                row.Cells["Status"].Value = this.cbo_SetAllInspections.SelectedItem.ToString();
            }
        }

        /// <summary>
        /// After each piece of information is entered, check to make sure it is valid, and if all pieces are present and valid, enable the submit button.
        /// </summary>
        /// <param name="sender">The Control which was just left.</param>
        /// <param name="e">Any Event Args.</param>
        private void ValidateInformation(object sender, EventArgs e)
        {
            /* Things we need to know are complete before we enable the Submit button
             * 1 - Every elevator in the list has a status assigned
             * 2 - The date has been changed and is valid for the inspection
             * 3 - There is a valid Inspection Type assigned
             * 4 - There is a valid Inspector assigned
            */
                        
            switch (((Control)sender).Name)
            {
                case "dtp_InspectionDate":
                    {
                        if (dtp_InspectionDate.Value > DateTime.Now)
                        {
                            error_dtp_InspectionDate.SetError(this.dtp_InspectionDate, "Date must be no later than today");
                        }
                        else if (!dtp_InspectionDate.Checked)
                        {
                            error_dtp_InspectionDate.SetError(this.dtp_InspectionDate, "Please select a date for the inspection");
                        }
                        else
                        {
                            error_dtp_InspectionDate.Clear();
                        }

                        break;
                    }

                case "dgv_ElevatorList":
                    {
                        bool allStatusComplete = true;
                        
                        foreach (DataGridViewRow row in this.dgv_ElevatorList.Rows)
                        {
                            foreach (string status in this.elev_STATUSES)
                            {
                                // Check for valid status here.
                            }
                        }

                        if (!allStatusComplete)
                        {
                            error_ElevatorStatus.SetError(this.dgv_ElevatorList, "Must set a status for each elevator");
                        }
                        else
                        {
                            error_ElevatorStatus.Clear();
                        }

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
        }
    }
}
