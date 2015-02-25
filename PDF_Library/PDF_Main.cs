﻿// <summary> Provides implementations of the iTestSharp libarary in a simpler way for use in the main project. </summary>

namespace PDF_Library
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using iTextSharp.text;
    using iTextSharp.text.pdf;

    /// <summary>
    /// Provides a simple interface for the iTextSharp functions.
    /// </summary>
    public class PDF_Document
    {
        /// <summary> The Generic Document variable for the PDF. </summary>
        private Document doc;

        /// <summary> PDF Writer to handle adding content to the document. </summary>
        private PdfWriter writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PDF_Document"/> class.
        /// </summary>
        /// <param name="filename">The Filename of the PDF to create. Must not already exist.</param>
        public PDF_Document(string filename)
        {
            if (File.Exists(filename))
            {
                throw new AccessViolationException("Cannot create PDF: " + filename + " already exists.");
            }
            
            // Create a letter sized PDF File
            this.doc = new Document(new iTextSharp.text.Rectangle(PageSize.LETTER));

            // Initialize the PDF Writer
            this.writer = PdfWriter.GetInstance(this.doc, new FileStream(filename, FileMode.Create));

            this.doc.Open();

            // Create a blank first page so it doesn't complain if we don't end up adding anything.
            PdfContentByte cb = this.writer.DirectContent;
            cb.MoveTo(this.doc.PageSize.Width / 2, this.doc.PageSize.Height / 2);
            cb.LineTo(this.doc.PageSize.Width / 2, this.doc.PageSize.Height / 2);
            cb.Stroke();
        }

        /// <summary>
        /// Create a Font for use by a iTextSharp method.
        /// </summary>
        /// <param name="basefont">The base Font family to use.</param>
        /// <param name="size">The size in pixels of the text.</param>
        /// <param name="style">The style of the text. (e.g. BaseFont.BOLD).</param>
        /// <param name="color">The color of the text.</param>
        /// <returns>A Font based on the given parameters.</returns>
        public static iTextSharp.text.Font CreateFont(string basefont, int size, int style, BaseColor color)
        {
            BaseFont bf = BaseFont.CreateFont(basefont, BaseFont.CP1252, false);
            return new iTextSharp.text.Font(bf, (float)size, style, color);
        }

        /// <summary>
        /// Create a Font for use by a iTextSharp method.
        /// </summary>
        /// <param name="basefont">The base Font family to use.</param>
        /// <param name="size">The size in pixels of the text.</param>
        /// <param name="style">The style of the text. (e.g. BaseFont.BOLD).</param>
        /// /// <returns>A Font based on the given parameters.</returns>
        public static iTextSharp.text.Font CreateFont(string basefont, int size, int style)
        {
            return CreateFont(basefont, size, style, BaseColor.BLACK);
        }

        /// <summary>
        /// Create a Font for use by a iTextSharp method.
        /// </summary>
        /// <param name="basefont">The base Font family to use.</param>
        /// <param name="size">The size in pixels of the text.</param>
        /// /// <returns>A Font based on the given parameters.</returns>
        public static iTextSharp.text.Font CreateFont(string basefont, int size)
        {
            return CreateFont(basefont, size, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        }

        /// <summary>
        /// Adds a text string to a designated location on the current PDF.
        /// </summary>
        /// <param name="text">The test string to add.</param>
        /// <param name="location">The location coordinates for the upper left point of text.</param>
        /// <param name="textbox">The size of the textbox the text should be written in.</param>
        /// <param name="rowheight">If it is to take up multiple lines, how far apart the lines should be.</param>
        /// <param name="textFont">The font to be used for the string text.</param>
        public void AddText(string text, Point location, Size textbox, int rowheight, iTextSharp.text.Font textFont)
        {
            PdfContentByte cb = this.writer.DirectContent;
            ColumnText coltext = new ColumnText(cb);
            Phrase newText = new Phrase(text, textFont);
            coltext.SetSimpleColumn(newText, location.X, location.Y, textbox.Height, textbox.Width, rowheight, Element.ALIGN_LEFT);
            coltext.Go();
        }

        /// <summary>
        /// Adds a text string to a designated location on the current PDF.
        /// </summary>
        /// <param name="text">The test string to add.</param>
        /// <param name="location">The location coordinates for the upper left point of text.</param>
        /// <param name="textbox">The size of the textbox the text should be written in.</param>
        /// <param name="rowheight">If it is to take up multiple lines, how far apart the lines should be.</param>
        public void AddText(string text, Point location, Size textbox, int rowheight)
        {
            this.AddText(text, location, textbox, rowheight, CreateFont(BaseFont.TIMES_ROMAN, 12));
        }

        /// <summary>
        /// Add an image to the document, scaled to fit the entire page.
        /// </summary>
        /// <param name="imagePath">The Filepath for the image.</param>
        public void AddImage(string imagePath)
        {
            var image = iTextSharp.text.Image.GetInstance(imagePath);
            image.ScaleToFit(this.doc.PageSize.Width, this.doc.PageSize.Height);
            image.SetAbsolutePosition(1, 1);
            doc.Add(image);
        }

        /// <summary>
        /// Close the PDF and save it. Super Important.
        /// </summary>
        public void ClosePDF()
        {
            this.doc.Close();
        }
    }
}