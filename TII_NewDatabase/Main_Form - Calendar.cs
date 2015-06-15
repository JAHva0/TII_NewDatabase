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
        /// <summary>
        /// A list to hold the calendars needed for this form. 
        /// </summary>
        private List<Calendar> calendars = new List<Calendar>();
        
        /// <summary>
        /// Initializes the Scheduling tab by authorizing with Google and populating the Inspector's combo box.
        /// </summary>
        private void InitializeSchedulingTab()
        {
            this.cbo_InspectorToSchedule.Items.Clear();
            Authorization.CalAuthenticateOAuth2("client_secret.json", SQL.Connection.GetUser);

            // Get a list of inspectors who have a calendar
            foreach (Calendar c in Calendar.GetList())
            {
                if (Inspection.GetInspectors(true).Contains(c.Name))
                {
                    c.LoadEvents();
                    this.calendars.Add(c);
                    this.cbo_InspectorToSchedule.Items.Add(c.Name);
                }
            }
        }
        
        /// <summary>
        /// Occurs whenever the user changes a value in the Scheduling <see cref="GroupBox"/>. 
        /// Checks to make sure all the data entered is valid.
        /// </summary>
        /// <param name="sender">The control which triggered this event.</param>
        /// <param name="e">This parameter is not used.</param>
        private void SchedulingDataChanged(object sender, EventArgs e)
        {
            Control senderControl = (Control)sender;

            // Make sure we can't set the Late time any earlier than the start time.
            if (senderControl.Name == "dtp_NoEarlierThan")
            {
                this.dtp_NoLaterThan.MinDate = this.dtp_NoEarlierThan.Value;
            }

            if (this.cbo_InspectorToSchedule.SelectedItem.ToString() == string.Empty)
            {
                return;
            }

            Calendar selectedCal = this.calendars.Where(x => x.Name == this.cbo_InspectorToSchedule.SelectedItem.ToString()).SingleOrDefault();

            if (this.cbo_InspectorToSchedule.SelectedItem.ToString() != null)
            {
                // Remove all dates that we've bolded before this point.
                this.cal_CalendarDisplay.RemoveAllBoldedDates();

                // Bold all dates that have something scheduled on them already
                foreach (DateTime date in selectedCal.AllEvents.Select(x => x.Start).Distinct())
                {
                    this.cal_CalendarDisplay.AddBoldedDate(date);
                }

                // Refresh the calendar to show our new bolded dates.
                this.cal_CalendarDisplay.UpdateBoldedDates();
            }
        }

        /// <summary>
        /// Updates the Current Schedule <see cref="ListBox"/> to display all events that occur on the date selected on the calendar control.
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void CalendarDisplay_DateChanged(object sender, DateRangeEventArgs e)
        {
            if (this.cbo_InspectorToSchedule.SelectedItem == null)
            {
                return;
            }
            
            this.lbx_CurrentSchedule.Items.Clear();
            Calendar selectedCal = this.calendars.Where(x => x.Name == cbo_InspectorToSchedule.SelectedItem.ToString()).SingleOrDefault();

            foreach (Entry calEntry in selectedCal.Events(cal_CalendarDisplay.SelectionStart))
            {
                this.lbx_CurrentSchedule.Items.Add(string.Format("{0}-{1}: {2}", calEntry.Start.ToShortTimeString(), calEntry.End.ToShortTimeString(), calEntry.Summary));
            }
        }
    }
}