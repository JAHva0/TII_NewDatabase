// <summary> Quick helper class to locate files in the report folder and associate them with inspections. </summary>

namespace TII_NewDatabase.HelperClasses
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    
    /// <summary>
    /// Emergency class created after I deleted the Report row in the database.
    /// </summary>
    public static class LocateReportFiles
    {
        /// <summary>
        /// More or less a temporary function built as a result of my wiping out the entire Report row in the database.
        /// Rolls through every file in the report directory and attempts to associate them with an inspection already entered in the database.
        /// </summary>
        /// <param name="directory_path">The directory path to search in. </param>
        public static void Search(string directory_path)
        {
            // Quit immediatly if this isn't even a valid path.
            if (!Directory.Exists(directory_path))
            {
                return;
            }

            foreach (string location_directory in Directory.GetDirectories(directory_path))
            {
                foreach (string inspector_directory in Directory.GetDirectories(location_directory))
                {
                    foreach (string file in Directory.GetFiles(inspector_directory))
                    {
                        string[] filename_parts = Path.GetFileNameWithoutExtension(file).Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);

                        try
                        {
                            DateTime date = filename_parts[0].ToYYMMDD();
                            string type, address;

                            type = filename_parts[1];
                            address = filename_parts[2];

                            // Find the most likely address in the database
                            List<Similarity> similarities = new List<Similarity>();
                            foreach (string db_addy in Main_Form.BuildingList.GetFilteredList(Directory.GetParent(inspector_directory).Name, string.Empty))
                            {
                                similarities.Add(new Similarity(address.SimilarityFactor(db_addy), db_addy));
                            }

                            Similarity mostlikely = similarities.OrderByDescending(o => o.Similar).ToList().First();

                            DataTable matchingInspections = SQL.Query.Select(
                                string.Format(
                                    "SELECT Inspection_ID " +
                                    "FROM Inspection " +
                                    "JOIN Elevator ON Elevator.Elevator_ID = Inspection.Elevator_ID " +
                                    "WHERE Building_ID = {0} " +
                                    "AND Date = '{1}'", 
                                    Main_Form.BuildingList.GetItemID(mostlikely.Value), 
                                    date));

                            foreach (DataRow inspection in matchingInspections.Rows)
                            {
                                SQL.Query.Update(
                                    "Inspection",
                                    new Database.SQLColumn[] { new Database.SQLColumn("Report", file) },
                                    string.Format("Inspection_ID = '{0}'", inspection["Inspection_ID"]));
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message + "\n" + file + " is not a properly formatted report file");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Quick variable for holding value pairs of similarity and values.
        /// </summary>
        private struct Similarity
        {
            /// <summary>
            /// The percentage indicating how similar this object was.
            /// </summary>
            public double Similar;

            /// <summary>
            /// The string value of this object.
            /// </summary>
            public string Value;

            /// <summary>
            /// Initializes a new instance of the <see cref="Similarity"/> struct.
            /// </summary>
            /// <param name="similarity">How similar this object was.</param>
            /// <param name="value">The value of this object.</param>
            public Similarity(double similarity, string value)
            {
                this.Similar = similarity;
                this.Value = value;
            }
        }
    }
}
