// <summary> Methods and Functions for the Calendar Tab of the main form. </summary>

namespace TII_NewDatabase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using Database;
    using Google_Library;

    /// <summary>
    /// Containing class for Form Functions.
    /// </summary>
    public partial class Main_Form : Form
    {
        private List<Calendar> calendars = new List<Calendar>();
        
        private void InitializeSchedulingTab()
        {
            this.cbo_InspectorToSchedule.Items.Clear();
            Authorization.CalAuthenticateOAuth2("client_secret.json", SQL.Connection.GetUser);

            foreach (Calendar c in Calendar.GetList())
            {
                if (Inspection.GetInspectors(true).Contains(c.Name))
                {
                    this.calendars.Add(c);
                    this.cbo_InspectorToSchedule.Items.Add(c.Name);
                }
            }
        }
        
        private void SchedulingDataChanged(object sender, EventArgs e)
        {

        }
    }
}