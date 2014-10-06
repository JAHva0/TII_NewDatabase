//-----------------------------------------------------------------------
// <summary>Entry Point for the Application</summary>
//-----------------------------------------------------------------------

namespace TII_NewDatabase
{
    using System;
    using System.Windows.Forms;
    
    /// <summary>
    /// Application Program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main_Form());
        }
    }
}