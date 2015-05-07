// <summary> Provides methods to create and save PDFs of Certificatiosn. </summary>

namespace TII_NewDatabase
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Database;
    using PDF_Library;
    
    /// <summary>
    /// Provides Static methods to create PDF copies of Certifications.
    /// </summary>
    public static class CreateCert
    {
        /// <summary>
        /// Creates a PDF file with the provided information on a DCRA Clean Cert Form.
        /// </summary>
        /// <param name="destinationFileName">The File to create.</param>
        /// <param name="inspectorName">The name of the inspector who performed this inspection.</param>
        /// <param name="building">The <see cref="Building"/> this inspection was performed at.</param>
        /// <param name="inspectionType"> The Type of inspection performed.</param>
        public static void CleanCert(string destinationFileName, string inspectorName, Building building, string inspectionType)
        {
            if (File.Exists("destinationFileName"))
            {
                File.Delete("destinationFileName");
            }

            PDF_Document newCert = new PDF_Document("TempCert.pdf");
            newCert.AddImage(@"C:\Users\Jon\Documents\TPIRs\DC\CleanCert_blank.jpg");

            // -----------------------Header Information-------------------------
            // Third Party Agency
            newCert.AddText("Technical Inspection of D.C. Inc.", 180, 713);

            // Date
            newCert.AddText(DateTime.Now.ToString("MMMM dd, yyyy"), 470, 713);

            // Elevator Professional In Charge
            newCert.AddText("Anthony Vattimo, Jr.", 200, 690);

            // QEI#
            newCert.AddText("S-171", 500, 690);

            // Name of Inspector
            newCert.AddText(inspectorName, 135, 667);

            // QEI#
            newCert.AddText("C3690", 500, 667);

            // Unit #(s)
            List<string> unitnos = new List<string>();

            // DCRA Certificate(s)
            List<string> certnos = new List<string>();
            foreach (Elevator elev in building.ElevatorList)
            {
                unitnos.Add(elev.Nickname);
                certnos.Add(elev.ElevatorNumber);
            }

            newCert.AddText("#" + unitnos.ToFormattedList(), 90, 644);

            // The first seven certificate numbers can fit on the first line. Any number higher than 7 that but less than 16 can go on the second.
            if (certnos.Count <= 7)
            {
                newCert.AddText(certnos.ToFormattedList(), 145, 621);
            }
            else if (certnos.Count >= 8 && certnos.Count <= 16)
            {
                newCert.AddText(certnos.GetRange(0, 7).ToFormattedList(false), 145, 621);
                newCert.AddText(certnos.GetRange(7, certnos.Count - 7).ToFormattedList(), 45, 598);
            }
            else
            {
                newCert.ClosePDF();
                throw new ArgumentOutOfRangeException("elevators", "Cannot create a properly formatted Cert with more than 16 units.");
            }

            // Project Address
            newCert.AddText(building.FormattedAddress, 125, 575);

            // Project Name
            newCert.AddText(building.Name, 160, 552);

            // Inspection Discipline
            newCert.AddText("X", 195, 529);

            // -------------------Inspection Type------------------------
            // Periodic - will always be selected
            newCert.AddText("X", 195, 505);

            // Category 1
            if (inspectionType.Contains("Category 1"))
            {
                newCert.AddText("X", 375, 505);
            }

            // Category 5
            if (inspectionType.Contains("Category 5"))
            {
                newCert.AddText("X", 375, 493);
            }

            // -------------------Certification Box------------------------
            // Elevator Professional In Charge
            newCert.AddText("Anthony Vattimo, Jr.", 100, 410);

            // Third Party Company Name
            newCert.AddText("Technical Inspection of D.C. Inc.", 90, 371);

            // Code year
            newCert.AddText("2013", 105, 269);

            // Add Signature
            newCert.AddImage(@"C:\Users\Jon\Documents\TPIRs\DC\TonySig.png", .45f, new System.Drawing.Point(105, 172));

            // Close the PDF
            newCert.ClosePDF();
        }
    }
}