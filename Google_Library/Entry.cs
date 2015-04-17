// <summary> Contains Classes and Funtions needed to access and manipulate Google Calendar's Events. </summary>

namespace Google_Library
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Google.Apis.Calendar.v3;
    using Google.Apis.Calendar.v3.Data;

    /// <summary>
    /// Provides a simple way to interface with a Calendar Entry. 
    /// </summary>
    public class Entry
    {
        /// <summary> The Google Provided ID for this event. </summary>
        private string id;

        /// <summary> The creation time of this event. </summary>
        private DateTime created;

        /// <summary> The start time of this event. </summary>
        private DateTime start;

        /// <summary> The end time of this event. </summary>
        private DateTime end;

        /// <summary> The status of this event. (e.g. "Tentative", "Canceled", "Confirmed"). </summary>
        private string status;

        /// <summary> The Summary Field of this event. </summary>
        private string summary;

        /// <summary>
        /// Initializes a new instance of the <see cref="Entry"/> class.
        /// </summary>
        /// <param name="eventinfo">The Event provided by Google's API.</param>
        public Entry(Event eventinfo)
        {
            this.id = eventinfo.Id;
            this.created = eventinfo.Created.Value;

            // If the Start and End Date Time are both null, it is because this is an all day event.
            if (eventinfo.Start.DateTime == null && eventinfo.End.DateTime == null)
            {
                DateTime.TryParse(eventinfo.Start.Date, out this.start);
                DateTime.TryParse(eventinfo.End.Date, out this.end);
                this.end.AddDays(1); // This is an all day event, so the next free time is going to be the next day.
            }
            else
            {
                this.start = eventinfo.Start.DateTime.Value;
                this.end = eventinfo.End.DateTime.Value;
            }

            this.status = eventinfo.Status;
            this.summary = eventinfo.Summary;
        }

        /// <summary> Gets the Creation Date of the Event. </summary>
        /// <value>A Date and Time of the Event Creation. </value>
        public DateTime Created
        {
            get
            {
                return this.created;
            }
        }

        /// <summary> Gets the Start Date of the Event. </summary>
        /// <value>A Date and Time of the Event Start. </value>
        public DateTime Start
        {
            get
            {
                return this.start;
            }
        }

        /// <summary> Gets the End Date of the Event. </summary>
        /// <value>A Date and Time of the Event End. </value>
        public DateTime End
        {
            get
            {
                return this.end;
            }
        }

        /// <summary>
        /// Gets a list of Events from the provided calendar.
        /// </summary>
        /// <param name="service"> The Calendar service to use. </param>
        /// <param name="calendarID"> The Calendar ID to get events from. </param>
        /// <returns> A list of <see cref="Entry"/> classes. </returns>
        public static List<Entry> GetEntries(CalendarService service, string calendarID)
        {
            List<Entry> entryList = new List<Entry>();
            foreach (Event e in EventHelper.List(service, calendarID).Items)
            {
                entryList.Add(new Entry(e));
            }

            return entryList;
        }

        /// <summary>
        /// Gets a list of Events from the provided calendar.
        /// </summary>
        /// <param name="service"> The Calendar service to use. </param>
        /// <param name="calendarID"> The Calendar ID to get events from. </param>
        /// <param name="start"> The date of the earliest event that should be returned. </param>
        /// <param name="end"> The date of the latest event that should be returned. </param>
        /// <returns> A list of <see cref="Entry"/> classes. </returns>
        public static List<Entry> GetEntries(CalendarService service, string calendarID, DateTime start, DateTime end)
        {
            List<Entry> entryList = new List<Entry>();
            foreach (Event e in EventHelper.List(service, calendarID).Items)
            {
                Entry newEntry = new Entry(e);
                if (newEntry.start > start && newEntry.end < end)
                {
                    entryList.Add(newEntry);
                }
            }

            return entryList;
        }

        /// <summary>
        /// Prints a formatted version of the relevant data for this class.
        /// </summary>
        /// <returns>A formatted string.</returns>
        public override string ToString()
        {
            return string.Format(
                "{0} - {1} to {2} - {3} - {4}",
                this.status,
                this.start.ToString(),
                this.end.ToString(),
                this.summary,
                this.created.ToString());
        }

        /// <summary>
        /// Contains Static methods to pull events from google.
        /// </summary>
        private class EventHelper
        {
            /// <summary>
            /// Returns a specific event.
            /// </summary>
            /// <param name="service">The authenticated Calendar Service.</param>
            /// <param name="id">The Calendar ID.</param>
            /// <param name="eventID">The Event ID.</param>
            /// <returns>The Specified Event.</returns>
            public static Event Get(CalendarService service, string id, string eventID)
            {
                try
                {
                    return service.Events.Get(id, eventID).Execute();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            /// <summary>
            /// Adds an entry to a specified calendar.
            /// </summary>
            /// <param name="service">The authenticated Calendar Service.</param>
            /// <param name="id">The Calendar ID.</param>
            /// <param name="body">The Event to insert.</param>
            /// <returns>A copy of the Event that was inserted.</returns>
            public static Event Insert(CalendarService service, string id, Event body)
            {
                try
                {
                    return service.Events.Insert(body, id).Execute();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            /// <summary>
            /// Moves an event from one calendar to another.
            /// </summary>
            /// <param name="service">The authenticated Calendar Service.</param>
            /// <param name="id">The Calendar ID to move from.</param>
            /// <param name="eventID">The Event ID.</param>
            /// <param name="destinationID">The Calendar ID to move to.</param>
            /// <returns>A copy of the event that was moved.</returns>
            public static Event Move(CalendarService service, string id, string eventID, string destinationID)
            {
                try
                {
                    return service.Events.Move(id, eventID, destinationID).Execute();
                }
                catch (Exception)
                {
                    throw;
                }
            }

            /// <summary>
            /// Updates an event.
            /// </summary>
            /// <param name="service">The authenticated Calendar Service.</param>
            /// <param name="id">The Calendar ID to move from.</param>
            /// <param name="eventID">The Event ID.</param>
            /// <param name="body">The new body text for the event.</param>
            /// <returns>A copy of the event that was updated.</returns>
            public static Event Update(CalendarService service, string id, string eventID, Event body)
            {
                try
                {
                    return service.Events.Update(body, id, eventID).Execute();
                }
                catch (Exception)
                {
                    throw;
                }
            }

            /// <summary>
            /// Gets a list of events off of a calendar.
            /// </summary>
            /// <param name="service">The authenticated Calendar Service.</param>
            /// <param name="id">The Calendar ID. </param>
            /// <param name="options"> Any options to be applied to this list. </param>
            /// <returns>A List of events from the provided calendar.</returns>
            public static Events List(CalendarService service, string id, Options options = null)
            {
                EventsResource.ListRequest request = service.Events.List(id);

                if (options == null)
                {
                    request.MaxResults = 100;
                }
                else
                {
                    request.MaxResults = options.MaxResults;
                    request.ShowDeleted = options.ShowDeleted;
                }

                return ProcessResults(request);
            }

            /// <summary>
            /// Private Method for processing results from a List Request.
            /// </summary>
            /// <param name="request">The List Request for these events. </param>
            /// <returns> An <see cref="Events"/> class. </returns>
            private static Events ProcessResults(EventsResource.ListRequest request)
            {
                try
                {
                    Events result = request.Execute();
                    List<Event> allRows = new List<Event>();

                    // Loop through until we arrive at an empty page.
                    while (result.Items != null)
                    {
                        // add the rows to the final list
                        allRows.AddRange(result.Items);

                        // If this is the last page, drop out right away
                        if (result.NextPageToken == null)
                        {
                            break;
                        }

                        // Prepare thenext page of results
                        request.PageToken = result.NextPageToken;

                        // Execute and process the next page request
                        result = request.Execute();
                    }

                    Events allData = result;
                    allData.Items = (List<Event>)allRows;
                    return allData;
                }
                catch (Exception)
                {
                    throw;
                }
            }

            /// <summary>
            /// Options for modifying Event Requests.
            /// </summary>
            public class Options
            {
                /// <summary> Gets or sets a value indicating whether to show deleted events. </summary>
                private bool showDeleted;

                /// <summary> Gets or sets a value indicating the maximum number of results to return. </summary>
                private int maxResults;

                /// <summary>
                /// Initializes a new instance of the <see cref="Options"/> class.
                /// </summary>
                public Options()
                {
                    this.maxResults = 100;
                    this.showDeleted = false;
                }

                /// <summary> Gets or sets a value indicating whether to show deleted events. </summary>
                /// <value> A boolean indicating if deleted events should be shown. </value>
                public bool ShowDeleted
                {
                    get
                    {
                        return this.showDeleted;
                    }

                    set
                    {
                        this.showDeleted = value;
                    }
                }

                /// <summary> Gets or sets a value indicating the maximum number of results to return. </summary>
                /// <value> An Integer representing the maximum number of events to return. </value>
                public int MaxResults
                {
                    get
                    {
                        return this.maxResults;
                    }

                    set
                    {
                        this.maxResults = value;
                    }
                }
            }
        }
    }
}