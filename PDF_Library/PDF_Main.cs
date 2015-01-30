﻿// <summary> Provides implementations of the iTestSharp libarary in a simpler way for use in the main project </summary>

namespace PDF_Library
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using iTextSharp.text;
    using iTextSharp.text.pdf;
    
    /// <summary> Provides static methods for PDF related functions. </summary>
    public static partial class PDF
    {
        
    }

    public class PDF_Document
    {
        private Document doc;

        public PDF_Document(string filename)
        {
            PdfReader reader = new PdfReader(filename);

            doc = new Document(reader.GetPageSizeWithRotation(1));
        }

        public void AddText(string text, Point location)
        {
            
        }
    }
}
