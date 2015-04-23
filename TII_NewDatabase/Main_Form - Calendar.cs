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

            cbo_AddressToSchedule.Items.AddRange(buildingList.GetFilteredList(string.Empty, string.Empty));
        }
        
        /// <summary>
        /// Occurs whenever the user changes a value in the Scheduling Groupbox. 
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

            Calendar selectedCal = this.calendars.Where(x => x.Name == cbo_InspectorToSchedule.SelectedItem.ToString()).SingleOrDefault();

            if (this.cbo_AddressToSchedule.SelectedItem.ToString() != null && this.cbo_InspectorToSchedule.SelectedItem.ToString() != null)
            {
                this.GetAvailableDates(selectedCal);
            }
        }

        private void GetAvailableDates(Calendar calendar)
        {
            List<DateTime> availableTimes = new List<DateTime>();
            DateTime potentialSpot = this.dtp_StartDate.Value.Date + this.dtp_NoEarlierThan.Value.TimeOfDay;
            TimeSpan potentialLength = new TimeSpan((int)this.nym_HoursNeeded.Value, 0, 0);

            while (availableTimes.Count < 20)
            {
                //System.Diagnostics.Debug.WriteLine("Checking Events for {0}", potentialSpot.Date);

                // Continue to check our potential spot until the end time of the potential spot we're looking for is later than the maximum we're allowed
                while ((potentialSpot.TimeOfDay.Add(potentialLength) <= this.dtp_NoLaterThan.Value.TimeOfDay))
                {
                    //System.Diagnostics.Debug.WriteLine("Checking for a potential spot between {0} and {1}", potentialSpot.TimeOfDay, potentialSpot.TimeOfDay.Add(potentialLength));

                    bool wontWork = false;
                    // Get all of the events on the calendar that occur on the same day as our potential day.
                    foreach (Entry daysEvents in calendar.UpcomingEvents.Where(x => x.Start.Date == potentialSpot.Date))
                    {
                        //System.Diagnostics.Debug.WriteLine("Comparing to an event that goes from {0} to {1}", daysEvents.Start.TimeOfDay, daysEvents.End.TimeOfDay);
                        // If the event start is equal to or within our potential event, it will not work.
                        if (potentialSpot.TimeOfDay >= daysEvents.Start.TimeOfDay && potentialSpot.TimeOfDay <= daysEvents.End.TimeOfDay)
                        {
                            //System.Diagnostics.Debug.WriteLine("The start time of our potential spot ({0}) interferes with this event ({1}-{2})", potentialSpot.TimeOfDay.ToString(), daysEvents.Start.TimeOfDay, daysEvents.End.TimeOfDay);
                            wontWork = true;
                        }

                        if (potentialSpot.TimeOfDay.Add(potentialLength) >= daysEvents.Start.TimeOfDay && potentialSpot.TimeOfDay.Add(potentialLength) <= daysEvents.End.TimeOfDay)
                        {
                            //System.Diagnostics.Debug.WriteLine("The end time of our potential spot ({0}) interferes with this event ({1}-{2})", potentialSpot.TimeOfDay.Add(potentialLength).ToString(), daysEvents.Start.TimeOfDay, daysEvents.End.TimeOfDay);
                            wontWork = true;
                        }

                        if (wontWork)
                        {
                            break;
                        }

                        
                    }

                    if (!wontWork)
                    {
                        // If we've gotten this far and we haven't tripped our boolean, the potential spot must work. Add it to our available times
                        availableTimes.Add(potentialSpot);
                    }

                    // Add 15 minutes to the potential spot before we check again.
                    potentialSpot = potentialSpot.AddMinutes(15);
                }

                // Add a day and set the start time back to our earliest time.
                potentialSpot = potentialSpot.Date.AddDays(1).Add(this.dtp_NoEarlierThan.Value.TimeOfDay);

                if (potentialSpot.DayOfWeek == DayOfWeek.Saturday)
                {
                    potentialSpot = potentialSpot.AddDays(2);
                }

                if (potentialSpot.DayOfWeek == DayOfWeek.Sunday)
                {
                    potentialSpot = potentialSpot.AddDays(1);
                }
            }

            this.lbx_PossibleAppointments.Items.Clear();
            foreach (DateTime slot in availableTimes)
            {
                this.lbx_PossibleAppointments.Items.Add(string.Format("{0} @ {1}", slot.Date, slot.TimeOfDay));
            }

            List<DateTime> dates = new List<DateTime>();

            DateTime t = new DateTime(2015, 2, 1);
            while (t < DateTime.Now)
            {
                if (t.DayOfWeek != DayOfWeek.Saturday && t.DayOfWeek != DayOfWeek.Sunday)
                {
                    Random r = new Random();
                    if (r.Next(0, 10) != 1)
                    {
                        dates.Add(t);
                    }
                }
                t = t.AddDays(1);
            }

            List<DateTime> datesToKeep = new List<DateTime>();
        }
    }
}