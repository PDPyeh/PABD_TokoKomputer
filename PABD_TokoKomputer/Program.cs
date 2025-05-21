using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PABD_TokoKomputer
{
    static class Program
    {
        public static bool IsLoggedIn = false;
        public static string Username = "";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new FormLogin()); // Mulai dari login
            if (IsLoggedIn)
            {
                Application.Run(new MainForm()); // Kalau berhasil login, baru ke MainForm
            }
        }
    }

}
