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
            txt_UserName.Text = Properties.Settings.Default.UserName;
            txt_Password.Text = Properties.Settings.Default.Password;
            txt_ServerAddress.Text = Properties.Settings.Default.ServerAddress;
        }

        /// <summary>
        /// Test the current Connection and Credentials and inform the user if they worked.
        /// </summary>
        /// <param name="sender">Control for the Action.</param>
        /// <param name="e">Event Args.</param>
        private void Btn_TestConnection_Click(object sender, EventArgs e)
        {
            Connection.CreateConnection(txt_UserName.Text, txt_Password.Text, txt_ServerAddress.Text);
            if (Connection.ConnectionUp)
            {
                MessageBox.Show("Connection Successful");
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

        /// <summary>
        /// Saves the settings currently entered into the form.
        /// </summary>
        /// <param name="sender">Control for the Action.</param>
        /// <param name="e">Event Args.</param>
        private void Save_Settings(object sender, EventArgs e)
        {
            // Store each of the text boxes in it's cooresponding Settings Item and save it as the default
            Properties.Settings.Default.UserName = txt_UserName.Text;
            Properties.Settings.Default.Password = txt_Password.Text;
            Properties.Settings.Default.ServerAddress = txt_ServerAddress.Text;
            Properties.Settings.Default.Save();

            MessageBox.Show("Settings Saved");
        }
    }
}
