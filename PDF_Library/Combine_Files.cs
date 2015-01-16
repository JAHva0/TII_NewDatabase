// <summary> Provides implementations of the iTestSharp libarary in a simpler way for use in the main project </summary>

namespace PDF_Library
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using iTextSharp.text;
    using iTextSharp.text.pdf;

    /// <summary> Provides static methods for PDF related functions. </summary>
    public static partial class PDF
    {
        /// <summary>
        /// Combines two or more PDF files into one in the order in which they are provided.
        /// </summary>
        /// <param name="filelist">Two or more files to combine into one.</param>
        /// <param name="outputfile">The file which will contain the combined files. Must not currently exist.</param>
        public static void CombineFiles(string[] filelist, string outputfile)
        {
            if (filelist.Length < 2)
            {
                throw new ArgumentException("At least two files are needed to combine. 'filelist' contains " + filelist.Length.ToString());
            }

            // Make sure there isn't already a file with the same name as the output file
            if (File.Exists(outputfile))
            {
                throw new AccessViolationException("Output File already exists: " + outputfile);
            }

            try
            {
                int fileIndex = 0;

                // Create a reader for the first document to combine
                PdfReader reader = new PdfReader(filelist[fileIndex]);

                // Create a Document object
                Document doc = new Document(reader.GetPageSizeWithRotation(1));

                // Create a writer that listens to the document
                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(outputfile, FileMode.Create));

                // Open the document
                doc.Open();
                PdfContentByte cb = writer.DirectContent;
                PdfImportedPage page;
                int rotation;

                // Add content to the writer based on the filelist.
                foreach (string file in filelist)
                {
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        // Set the page in the new document to be the same size and rotation as the one we're copying in. 
                        doc.SetPageSize(reader.GetPageSizeWithRotation(i));

                        // Start a new page based on the current page in the reader. 
                        doc.NewPage();
                        page = writer.GetImportedPage(reader, i);
                        rotation = reader.GetPageRotation(i);
                        if (rotation == 90 || rotation == 270)
                        {
                            cb.AddTemplate(page, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                        }
                        else
                        {
                            cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                        }
                    }

                    // As long as we aren't already on the last file in the list, increment the file index by one.
                    if (fileIndex < filelist.Length - 1)
                    {
                        fileIndex++;
                        reader = new PdfReader(filelist[fileIndex]);
                    }
                }

                doc.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
