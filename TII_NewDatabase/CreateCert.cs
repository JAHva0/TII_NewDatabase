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
            if (File.Exists(destinationFileName))
            {
                File.Delete(destinationFileName);
            }

            PDF_Document newCert = new PDF_Document(destinationFileName);
            newCert.AddImage(Properties.Settings.Default.CertBackgroundImage, 610f, 789.4f);

            // -----------------------Header Information-------------------------
            // Third Party Agency
            newCert.AddText("Technical Inspection of D.C. Inc.", 180, 713);

            // Date
            newCert.AddText(DateTime.Now.ToString("MMMM dd, yyyy"), 470, 713);

            // Elevator Professional In Charge
            newCert.AddText(Properties.Settings.Default.CertProfessionalInCharge, 200, 690);

            // QEI#
            newCert.AddText(Inspection.GetInspectorNAESAID(Properties.Settings.Default.CertProfessionalInCharge), 500, 690);

            // Name of Inspector
            newCert.AddText(inspectorName, 135, 667);

            // QEI#
            newCert.AddText(Inspection.GetInspectorNAESAID(inspectorName), 500, 667);

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
            if (certnos.ToFormattedList().Length < 80)
            {
                newCert.AddText(certnos.ToFormattedList(), 145, 621);
            }
            else if (certnos.ToFormattedList().Length < 180)
            {
                string line = string.Empty;
                int count = 4; // We can safely start at two, since we're not going to have four 40-char elevator numbers
                while (line.Length < 80)
                {
                    line = certnos.GetRange(0, count).ToFormattedList(false);
                    count++;
                }
                count--;

                newCert.AddText(certnos.GetRange(0, count).ToFormattedList(false), 145, 621);

                if (certnos.Count - count == 1)
                {
                    // If there is only one item on the second line, put an ampersand at the start of it for pretty.
                    newCert.AddText("& " + certnos[count], 45, 598);
                }
                else
                {
                    newCert.AddText(certnos.GetRange(count, certnos.Count - count).ToFormattedList(), 45, 598);
                }
            }
            else
            {
                newCert.ClosePDF();
                throw new ArgumentOutOfRangeException("Elevator Cert Numbers", "Cannot create a properly formatted Cert with this many units.");
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
            newCert.AddText(Properties.Settings.Default.CertProfessionalInCharge, 100, 410);

            // Third Party Company Name
            newCert.AddText("Technical Inspection of D.C. Inc.", 90, 371);

            // Code year
            newCert.AddText("2013", 105, 269);

            // Add Signature
            newCert.AddImage(Properties.Settings.Default.CertSignatureFile, .45f, new System.Drawing.Point(105, 172));

            // Close the PDF
            newCert.ClosePDF();
        }
    }
}