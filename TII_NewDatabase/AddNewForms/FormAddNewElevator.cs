// <summary> Form for adding Elevator entries into the database. Performs basic error checking and provides information to the user regarding proper data input. </summary>

namespace TII_NewDatabase.AddNewForms
{
    using System;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using Database;
    
    /// <summary>
    /// Creates a form for adding elevator entries.
    /// </summary>
    public partial class FormAddNewElevator : Form
    {
        /// <summary> The building to which this elevator is being added. </summary>
        private Building owner;

        /// <summary> Holds the elevator data we are editing. </summary>
        private Elevator elevData = new Elevator();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FormAddNewElevator"/> class.
        /// </summary>
        /// <param name="ownerBuilding"> 
        /// The Building to which this elevator is being added.
        /// Is a requirement, as we want to be certain of a valid building entry for every elevator. Also they don't move much so they can be tied down fairly confidently.
        /// </param>
        public FormAddNewElevator(Building ownerBuilding)
        {
            this.InitializeComponent();
            this.owner = ownerBuilding;
            this.Text = "Add Elevator To " + ownerBuilding.Street;
            this.cbo_ElevatorType.Items.AddRange(Elevator.Types);

            this.SetToolTips();

            this.elevData.OwnerID = ownerBuilding.ID.Value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormAddNewElevator"/> class.
        /// </summary>
        /// <param name="elevatorToEdit"> Initializing with a particular elevator changes this form to allow the editing of a particular entry. </param>
        public FormAddNewElevator(Elevator elevatorToEdit)
        {
            this.InitializeComponent();

            this.Text = "Edit Elevator";
            this.txt_ElevatorNumber.Text = elevatorToEdit.ElevatorNumber;
            this.txt_ElevatorNick.Text = elevatorToEdit.Nickname;
            this.cbo_ElevatorType.Text = elevatorToEdit.ElevatorType;

            this.cbo_ElevatorType.Items.AddRange(Elevator.Types);
            this.SetToolTips();

            this.elevData = elevatorToEdit;
        }

        /// <summary>
        /// Method to group together all of the tool tips for this form.
        /// </summary>
        private void SetToolTips()
        {
            ToolTip tip_ElevNumber = new ToolTip();
            tip_ElevNumber.SetToolTip(this.txt_ElevatorNumber, "The State Elevator Number or District Certificate Number. If Unknown, press the \"Generate...\" button to obtain a temporary Number from the server");

            ToolTip tip_ElevType = new ToolTip();
            tip_ElevType.SetToolTip(this.cbo_ElevatorType, "The Type of unit to be added. If the desired type does not appear here, it must be added to the list manually");

            ToolTip tip_ElevNick = new ToolTip();
            tip_ElevNick.SetToolTip(this.txt_ElevatorNick, "(Optional) If the unit is referred to by some easier to remember name (i.e. \"1\" or \"Lobby\"), it may be entered here");

            ToolTip tip_GenerateNumber = new ToolTip();
            tip_GenerateNumber.SetToolTip(this.btn_GenerateNumber, "Generates a number in the format \"TII000000\" for use in the database as a placeholder until the unit is assigned an offical number");
        }

        /// <summary>
        /// Save the currently entered data to the database.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Any Event Args.</param>
        private void SaveEntry(object sender, EventArgs e)
        {
            try
            {
                this.elevData.CommitToDatabase();
            }
            catch (SQL.SQLDuplicateEntryException ex)
            {
                throw new SQL.SQLDuplicateEntryException("Duplicate Elevator Entry", ex, this.elevData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Checks to make sure the data entered into the sender is valid, and if not, throws an error. 
        /// If there are no errors on the form, enables the Save button, and if there are, disables it.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Any Event Args.</param>
        private void ValidateData(object sender, EventArgs e)
        {
            switch (((Control)sender).Name)
            {
                case "txt_ElevatorNumber":
                    {
                        if (this.txt_ElevatorNumber.Text == string.Empty)
                        {
                            this.error_provider.SetError(this.txt_ElevatorNumber, "An elevator number must be provided. Generate a temporary one if necessary.");
                        }
                        else
                        {
                            this.elevData.ElevatorNumber = this.txt_ElevatorNumber.Text;
                            this.error_provider.SetError(this.txt_ElevatorNumber, string.Empty);
                        }
                        
                        break;
                    }

                case "cbo_ElevatorType":
                    {
                        if (!Elevator.Types.Contains(this.cbo_ElevatorType.Text))
                        {
                            this.error_provider.SetError(this.cbo_ElevatorType, "Must select an elevator type from the dropbox");
                        }
                        else
                        {
                            this.elevData.ElevatorType = this.cbo_ElevatorType.Text;
                            this.error_provider.SetError(this.cbo_ElevatorType, string.Empty);
                        }
                        
                        break;
                    }

                case "txt_ElevatorNick":
                    {
                        this.elevData.Nickname = this.txt_ElevatorNick.Text;
                        break;
                    }

                default: throw new NotImplementedException("How did you get here?");
            }

            this.btn_SaveEntry.Enabled = this.AllControlsContainValidData();
        }

        /// <summary>
        /// Private method to determine if there are any errors present on the form. If so, return false.
        /// </summary>
        /// <returns>True, if there are no errors on the form. </returns>
        private bool AllControlsContainValidData()
        {
            foreach (Control c in this.Controls)
            {
                if (this.error_provider.GetError(c) != string.Empty)
                {
                    return false;
                }
            }

            if (this.txt_ElevatorNumber.Text == string.Empty)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Generates a temporary elevator number in the format "TII00000" based on the highest number in that form present in the database at the moment.
        /// </summary>
        /// <param name="sender">The Generate button.</param>
        /// <param name="e">The Click Event.</param>
        private void GenerateNumber(object sender, EventArgs e)
        {
            // Get the topmost entry in the database that uses this format
            string number = SQL.Query.Select("TOP 1 Number", "Elevator", "Number LIKE 'TII%'", "ID DESC").Rows[0][0].ToString();
            
            // Strip out all the non-numeric characters from the string.
            number = Regex.Replace(number, "[^.0-9]", string.Empty);

            // Recreate the string with the number increased by one.
            number = "TII" + (Convert.ToInt32(number) + 1).ToString().PadLeft(5, '0');

            this.txt_ElevatorNumber.Text = number;
        }
    }
}
