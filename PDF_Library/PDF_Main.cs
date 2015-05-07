// <summary> Provides implementations of the iTestSharp libarary in a simpler way for use in the main project. </summary>

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

        private PdfContentByte canvas;

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
            this.canvas = writer.DirectContent;
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

        public void AddText(string text, Point location)
        {
            this.canvas.BeginText();
            this.canvas.MoveText(location.X, location.Y);
            this.canvas.SetFontAndSize(BaseFont.CreateFont(), 10);
            this.canvas.ShowText(text);
            this.canvas.EndText();
        }

        public void AddText(string text, int x_location, int y_location)
        {
            AddText(text, new Point(x_location, y_location));
        }

        /// <summary>
        /// Add an image to the document, scaled to fit the entire page.
        /// </summary>
        /// <param name="imagePath">The Filepath for the image.</param>
        public void AddImage(string imagePath)
        {
            this.AddImage(imagePath, 1.0f, new Point(1, 1));
        }

        /// <summary>
        /// Add an image to the document with a given scale and location
        /// </summary>
        /// <param name="imagePath">The Filepath for the image.</param>
        /// <param name="scale">The amount to scale the image.</param>
        /// <param name="location">The coordinates for the image to appear on the document.</param>
        public void AddImage(string imagePath, float scale, Point location)
        {
            var image = iTextSharp.text.Image.GetInstance(imagePath);
            image.ScaleToFit(image.Width * scale, image.Height * scale);
            image.SetAbsolutePosition(location.X, location.Y);
            doc.Add(image);
        }

        public void AddImage(string imagePath, float desiredWidth, float desiredHeight)
        {
            var image = iTextSharp.text.Image.GetInstance(imagePath);
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
