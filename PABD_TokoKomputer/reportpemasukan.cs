using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace PABD_TokoKomputer
{
    public partial class reportpemasukan: Form
    {
        public reportpemasukan()
        {
            InitializeComponent();
        }

        private void reportpemasukan_Load(object sender, EventArgs e)
        {

            this.reportViewer1.RefreshReport();
            SetupReportViewer();
        }

        private void SetupReportViewer()
        {
            // Connection string to your database
            string connectionString = "Data Source=LAPTOP-Q7EVPB6K\\PRADIPAYOGANANDA;" + "Initial Catalog=SistemTokoComputerPABD_1;Integrated Security=True";

            // SQL query to retrieve the required data from the database
            string query = @"
                SELECT        
                         Pembayaran.PembayaranID, Pelanggan.Nama_Pelanggan, Produk.NamaProduk, Pemesanan.TanggalPemesanan, Pembayaran.TanggalPembayaran, Pemesanan.Jumlah, Pembayaran.JumlahPembayaran, 
                         Pembayaran.StatusPembayaran
                FROM     
                         Pembayaran 
                INNER JOIN
                         Pemesanan ON Pembayaran.PemesananID = Pemesanan.PemesananID INNER JOIN
                         Produk ON Pemesanan.ProdukID = Produk.ProdukID INNER JOIN
                         Pelanggan ON Pemesanan.PelangganID = Pelanggan.PelangganID";

            // Create a DataTable to store the data
            DataTable dt = new DataTable();

            // Use SqlDataAdapter to fill the DataTable with data from the database
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);

                da.Fill(dt);
            }
            // Create a ReportDataSource
            ReportDataSource rds = new ReportDataSource("DataSetPemasukan", dt); // Make sure "DataSet1" matches your RDLC dataset name

            // Clear any existing data sources and add the new data source
            reportViewer1.LocalReport.DataSources.Clear();
            reportViewer1.LocalReport.DataSources.Add(rds);

            // Set the path to the report (.rdlc file)
            // Change this to the actual path of your RDLC file
            reportViewer1.LocalReport.ReportPath = @"D:\PABD_RDLCREPORT\reportpemasukan.rdlc";
            // Refresh the ReportViewer to show the updated report
            reportViewer1.RefreshReport();
        }

        private void reportViewer1_Load(object sender, EventArgs e)
        {

        }
    }
}
