// <summary> Contains Classes and Funtions needed to access Google Calendar. </summary>

namespace Google_Library
{
    using System;
    using System.IO;
    using System.Threading;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Calendar.v3;
    using Google.Apis.Services;
    using Google.Apis.Util.Store;
    using Newtonsoft.Json;

    /// <summary>
    /// Static Methods for providing Services from authorization files or information.
    /// </summary>
    public class Authorization
    {
        /// <summary>
        /// The google calendar service.
        /// </summary>
        internal static CalendarService CalendarService;
        
        /// <summary>
        /// Authenticate application using OAuth2.
        /// </summary>
        /// <param name="clientID">The ID from the Client Secrets File.</param>
        /// <param name="clientSecret">The Secret from the Client Secrets File.</param>
        /// <param name="userName">A string used to identify the user.</param>
        public static void CalAuthenticateOAuth2(string clientID, string clientSecret, string userName)
        {
            // We are requesting to Read and Manage the user's Calendars
            string[] scopes = new string[] { CalendarService.Scope.Calendar, CalendarService.Scope.CalendarReadonly };

            try
            {
                // REquest the the user give permission, or load the stored Token that was saved in the app folder
                //// The token response will be saved in C:/Users/%Name%/AppData/Roaming/TIIDatabase
                UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets { ClientId = clientID, ClientSecret = clientSecret },
                    scopes,
                    userName,
                    CancellationToken.None,
                    new FileDataStore("TIIDatabase")).Result;

                CalendarService = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "TII Database"
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Authenticate application using OAuth2.
        /// </summary>
        /// <param name="clientSecretFile"> The Json file containing the client secrets. </param>
        /// <param name="userName">A string used to identify the user.</param>
        public static void CalAuthenticateOAuth2(string clientSecretFile, string userName)
        {
            string secret = string.Empty;
            string id = string.Empty;

            using (StreamReader stream = new StreamReader(clientSecretFile))
            {
                using (JsonTextReader jreader = new JsonTextReader(stream))
                {
                    while (jreader.Read())
                    {
                        switch (jreader.Path.ToString())
                        {
                            case "installed.client_secret":
                                {
                                    secret = jreader.Value.ToString();
                                    break;
                                }

                            case "installed.client_id":
                                {
                                    id = jreader.Value.ToString();
                                    break;
                                }
                        }
                    }
                }
            }

            CalAuthenticateOAuth2(id, secret, userName);
        }
    }
}