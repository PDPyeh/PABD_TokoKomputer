using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace PABD_TokoKomputer
{
    class koneksi
    {
        public string connectionString() // untuk membangun dan mengembalikan string koneksi ke database
        {
            string connectStr = "";
            try
            {
                string localIP = GetLocalIPAddress(); // opsional, bisa dihapus kalau gak dipakai
                connectStr = "Server=10.200.60.152,1433;Initial Catalog=SistemTokoComputerPABD_1;User ID=adminToko;Password=1234;";
                return connectStr;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }


        public static string GetLocalIPAddress() // untuk mengambil IP Address pada PC yang menjalankan aplikasi
        {
            // mengambil informasi tentang local host
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork) // Mengambil IPv4
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Tidak ada alamat IP yang ditemukan.");
        }
    }
}
