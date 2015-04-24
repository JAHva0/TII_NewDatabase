// <summary> Contains Classes and Funtions needed to access and manipulate Google Calendar's Calendars. </summary>

namespace Google_Library
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Google.Apis.Calendar.v3;
    using Google.Apis.Calendar.v3.Data;
    
    /// <summary>
    /// Class for holding Data returned by Google's calendar API.
    /// </summary>
    public class Calendar
    {
        /// <summary> The supplied ID for this calendar. </summary>
        private string id;

        /// <summary> The name given to this calendar. From the Summary field. </summary>
        private string name;

        /// <summary> The Color ID assigned to this calendar. </summary>
        private string colorID;

        /// <summary> The Service required for accessing this calendar. </summary>
        private CalendarService service;

        private List<Entry> events = new List<Entry>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Calendar"/> class. 
        /// </summary>
        /// <param name="service">The Calendar Service needed to access.</param>
        /// <param name="name">The Name of the calendar.</param>
        public Calendar(string name)
        {
            // Get a list of all the calendars on this account, and pull out the one with a matching name.
            Calendar entry = GetList().Where(x => x.name == name).SingleOrDefault();
            this.id = entry.id;
            this.name = entry.name;
            this.colorID = entry.colorID;
            this.service = Authorization.calendarService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Calendar"/> class. Private constructor used by <see cref="GetList"/>.
        /// </summary>
        /// <param name="service">The Calendar Service needed to access.</param>
        /// <param name="entry">A <see cref="CalendarListEntry"/> loaded by a list request. </param>
        private Calendar(CalendarListEntry entry)
        {
            this.id = entry.Id;
            this.name = entry.Summary;
            this.colorID = entry.ColorId;
            this.service = Authorization.calendarService;
        }

        /// <summary> Gets the name of this calendar. </summary>
        /// <value> A string of the calendar's Name. </value>
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public List<Entry> AllEvents
        {
            get
            {
                return this.events;
            }
        }

        public List<Entry> UpcomingEvents
        {
            get
            {
                return this.events.Where(x => x.Start.Date >= DateTime.Now.Date).ToList();
            }
        }

        public void LoadEvents()
        {
            this.events = Entry.GetEntries(this.service, this.id);
        }
        
        /// <summary>
        /// Gets a list of all calendars on this account.
        /// </summary>
        /// <param name="service">The Service used to access the calendar.</param>
        /// <param name="options">Any options that might be requested.</param>
        /// <returns>A list of <see cref="Calendar"/>.</returns>
        public static List<Calendar> GetList(Options options = null)
        {
            CalendarListResource.ListRequest request = Authorization.calendarService.CalendarList.List();

            if (options == null)
            {
                request.MaxResults = 100;
            }
            else
            {
                request.MaxResults = options.MaxResults;
                request.ShowDeleted = options.ShowDeleted;
                request.ShowHidden = options.ShowHidden;
                request.MinAccessRole = options.MinAccessRole;
            }

            CalendarList calList = ProcessResults(request);

            List<Calendar> finalList = new List<Calendar>();
            foreach (CalendarListEntry entry in calList.Items)
            {
                finalList.Add(new Calendar(entry));
            }

            return finalList;
        }

        /// <summary>
        /// Gets a list of Events present in this calendar.
        /// </summary>
        /// <returns> A List of <see cref="Entry"/> classes. </returns>
        public List<Entry> Events()
        {
            return Entry.GetEntries(this.service, this.id);
        }

        /// <summary>
        /// Gets a list of Events present in this calendar.
        /// </summary>
        /// <param name="start"> The date of the earliest event that should be returned. </param>
        /// <param name="end"> The date of the latest event that should be returned. </param>
        /// <returns> A List of <see cref="Entry"/> classes. </returns>
        public List<Entry> Events(DateTime start, DateTime end)
        {
            return Entry.GetEntries(this.service, this.id, start, end);
        }

        /// <summary>
        /// Private method to parse through the results of a Calendar List Request.
        /// </summary>
        /// <param name="request">The request to parse.</param>
        /// <returns> A <see cref="CalendarList"/> of the results. </returns>
        private static CalendarList ProcessResults(CalendarListResource.ListRequest request)
        {
            try
            {
                CalendarList result = request.Execute();
                List<CalendarListEntry> calendarRows = new List<CalendarListEntry>();

                // Loop through the results until we get an empty one
                while (result.Items != null)
                {
                    // Add the rows to the list
                    calendarRows.AddRange(result.Items);

                    // We will know if we are on the last page when the next token is null
                    if (result.NextPageToken == null)
                    {
                        break;
                    }

                    // Create the next page of results
                    request.PageToken = result.NextPageToken;

                    result = request.Execute();
                }

                CalendarList allData = result;
                allData.Items = (List<CalendarListEntry>)calendarRows;
                return allData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Optional query parameters for requesting data from Google.
        /// </summary>
        public class Options
        {
            /// <summary> Gets or sets a value indicating whether deleted calendars be displayed. </summary>
            private bool showDeleted;

            /// <summary> Gets or sets a value indicating whether Hidden calendars be displayed. </summary>
            private bool showHidden;

            /// <summary> Gets or sets a value indicating the maximum number of entries to return. </summary>
            private int maxResults;

            /// <summary> Gets or sets a value indicating the minimum access for the user in the returned entries. </summary>
            private CalendarListResource.ListRequest.MinAccessRoleEnum? minAccessRole = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="Options"/> class.
            /// </summary>
            public Options()
            {
                this.maxResults = 100;
                this.showDeleted = false;
                this.showHidden = false;
                this.minAccessRole = null;
            }

            /// <summary> Gets or sets a value indicating whether deleted calendars be displayed. </summary>
            /// <value> False by default. </value>
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

            /// <summary> Gets or sets a value indicating whether Hidden calendars be displayed. </summary>
            /// <value> False by default. </value>
            public bool ShowHidden
            {
                get
                {
                    return this.showHidden;
                }

                set
                {
                    this.showHidden = value;
                }
            }

            /// <summary> Gets or sets the maximum number of entries to return. </summary>
            /// <value> 100 by default. </value>
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

            /// <summary>
            /// Gets or sets the minimum access for the user in the returned entries.
            /// Possible Values:
            /// owner - the user can read and modify events and access lists.
            /// reader - the user can read events that are not private.
            /// writer - the user can read and modify events.
            /// </summary>
            /// <value> No restriction by default. </value>
            public CalendarListResource.ListRequest.MinAccessRoleEnum? MinAccessRole
            {
                get
                {
                    return this.minAccessRole;
                }

                set
                {
                    this.minAccessRole = value;
                }
            }
        }
    }
}
