// <summary> Form for adding Elevator entries into the database. Performs basic error checking and provides information to the user regarding proper data input. </summary>

namespace TII_NewDatabase.AddNewForms
{
    using System;
    using System.Windows.Forms;
    using Database;
    
    /// <summary>
    /// Creates a form for adding elevator entries.
    /// </summary>
    public partial class FormAddNewElevator : Form
    {
        /// <summary> The building to which this elevator is being added. </summary>
        private Building owner;
        
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
        }

        /// <summary>
        /// Save the currently entered data to the database.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Any Event Args.</param>
        private void SaveEntry(object sender, EventArgs e)
        {
            Elevator newElevator = new Elevator();
            newElevator.OwnerID = this.owner.ID.Value;
            newElevator.ElevatorNumber = this.txt_ElevatorNumber.Text;
            newElevator.ElevatorType = this.cbo_ElevatorType.Text;
            newElevator.Nickname = this.txt_ElevatorNick.Text;

            newElevator.CommitToDatabase();
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
                            this.error_provider.SetError(this.cbo_ElevatorType, string.Empty);
                        }
                        
                        break;
                    }

                case "txt_ElevatorNick":
                    {
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

            return true;
        }
    }
}
