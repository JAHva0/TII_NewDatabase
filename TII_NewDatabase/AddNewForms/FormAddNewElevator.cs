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
    }
}
