//-------------------------------------------
// <summary>Form the user can change the SQL Server connection settings</summary>
//------------------------------------------

namespace TII_NewDatabase
{
    using System;
    using System.Windows.Forms;
    using SQL;

    /// <summary>
    /// Connection Settings Form.
    /// </summary>
    public partial class FormConnectionSettings : Form
    {       
        /// <summary>
        /// Initializes a new instance of the <see cref="FormConnectionSettings"/> class.
        /// </summary>
        public FormConnectionSettings()
        {
            this.InitializeComponent();

            // Populate the Text Boxes with any stored information
            this.txt_UserName.Text = Properties.Settings.Default.UserName;
            this.txt_Password.Text = Properties.Settings.Default.Password;
            this.txt_ServerAddress.Text = Properties.Settings.Default.ServerAddress;
        }

        /// <summary>
        /// Gets a value indicating whether or not the currently supplied connection credentials and server address make a valid connection.
        /// </summary>
        /// <value> True, if the database connection can be made. </value>
        public bool ConnectionWorking
        {
            get
            {
                Connection.CreateConnection(txt_UserName.Text, txt_Password.Text, txt_ServerAddress.Text);
                if (Connection.ConnectionUp)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Test the current Connection and Credentials and inform the user if they worked.
        /// </summary>
        /// <param name="sender">Control for the Action.</param>
        /// <param name="e">Event Args.</param>
        private void Btn_TestConnection_Click(object sender, EventArgs e)
        {
            if (this.ConnectionWorking)
            {
                // Store each of the text boxes in it's cooresponding Settings Item and save it as the default
                Properties.Settings.Default.UserName = txt_UserName.Text;
                Properties.Settings.Default.Password = txt_Password.Text;
                Properties.Settings.Default.ServerAddress = txt_ServerAddress.Text;
                Properties.Settings.Default.Save();

                if (MessageBox.Show("Settings Saved - close this window?", "Connection Successful", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Check that the User/Pass and server location are correct", "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Close the Form regardless of any changes. 
        /// </summary>
        /// <param name="sender">Control for the Action. In this case, the Close button.</param>
        /// <param name="e">Event Args.</param>
        private void Close_Form(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
