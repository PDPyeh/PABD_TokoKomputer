using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Data.SqlClient;

namespace PABD_TokoKomputer
{
    class koneksi
    {
        public string connectionString() // untuk membangun dan mengembalikan string koneksi ke database
        {
            string connectStr = "";
            try
            {
                 // mendeklarasikan ip address
                connectStr = "Server=tcp:barbershoppabd.database.windows.net,1433;Initial Catalog=SistemTokoComputerPABD_1;Persist Security Info=False;User ID=LordAAI;Password=omkegams;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30";
                return connectStr;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }
    }
}
