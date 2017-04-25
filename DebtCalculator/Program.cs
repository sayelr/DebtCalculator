// Sayel Rammaha    
// 4/10/17
// "Debt Calculator" Program Main() class

using System;
using System.Windows.Forms;

namespace DebtCalculator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
