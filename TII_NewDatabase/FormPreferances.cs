// <summary> Form to let the user set preferances that affet the performance of the form. </summary>

namespace TII_NewDatabase
{
    using System;
    using System.IO;
    using System.Windows.Forms;

    using Database;
    
    /// <summary>
    /// Class to display and modify user preferences.
    /// </summary>
    public partial class FormPreferances : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormPreferances"/> class.
        /// </summary>
        public FormPreferances()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initialize the form's contents after loading.
        /// </summary>
        /// <param name="sender">This form.</param>
        /// <param name="e">Event On Load.</param>
        private void OnLoad(object sender, EventArgs e)
        {
            this.txt_ReportFileLocation.Text = Properties.Settings.Default.ReportLocation;
            this.cbx_MoveAndNameReports.Checked = Properties.Settings.Default.MoveAndSaveReports;
        }

        /// <summary>
        /// Method to let the user browse for and select a folder which contains the Reports.
        /// </summary>
        /// <param name="sender">The Browse button.</param>
        /// <param name="e">On Click Event.</param>
        private void OnClick_BrowseForReportFolder(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.txt_ReportFileLocation.Text = fbd.SelectedPath;
            }

            if (!Directory.Exists(fbd.SelectedPath + @"\MD") ||
                !Directory.Exists(fbd.SelectedPath + @"\DC") ||
                !Directory.Exists(fbd.SelectedPath + @"\Misc Documents"))
            {
                if (MessageBox.Show(
                    "Are you sure this is the correct folder? \r" +
                    "The Report folder should have the following layout:\r\r" +
                    " Misc Documents\r MD\r    Inspector1\r    Inspector2\r DC\r    Inspector1\r    Inspector2\r",
                    "Are you sure?",
                    MessageBoxButtons.RetryCancel,
                    MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Retry)
                {
                    this.OnClick_BrowseForReportFolder(new object(), EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Method to save the user's preferences and exit the form.
        /// </summary>
        /// <param name="sender">The Save button.</param>
        /// <param name="e">On Click Event.</param>
        private void OnClick_SaveAndExit(object sender, EventArgs e)
        {
            Properties.Settings.Default.ReportLocation = this.txt_ReportFileLocation.Text;
            Properties.Settings.Default.MoveAndSaveReports = this.cbx_MoveAndNameReports.Checked;
            Properties.Settings.Default.Save();
            this.Close();
        }
    }
}
