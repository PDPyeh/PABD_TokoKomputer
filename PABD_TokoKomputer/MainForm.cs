using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UCP1PABD;
using OfficeOpenXml;
using System.IO;
using System.Data.SqlClient;



namespace PABD_TokoKomputer
{
    public partial class MainForm : Form
    {

        private SqlConnection conn = new SqlConnection("Data Source=LAPTOP-Q7EVPB6K\\PRADIPAYOGANANDA;Initial Catalog=SistemTokoComputerPABD_1;Integrated Security=True");
        public MainForm()
        {
            InitializeComponent();
        }

        // Fungsi reusable buat nampilin form ke dalam panelMain
        private void LoadFormInPanel(Form frm)
        {
            panelMain.Controls.Clear();
            frm.TopLevel = false;
            frm.FormBorderStyle = FormBorderStyle.None;
            frm.Dock = DockStyle.Fill;
            panelMain.Controls.Add(frm);
            frm.Show();
           
        }

        private void btnPelanggan_Click(object sender, EventArgs e)
        {
            LoadFormInPanel(new FormPelanggan());
        }

        private void btnProduk_Click(object sender, EventArgs e)
        {
            LoadFormInPanel(new FormProduk());
        }

        private void btnPemesanan_Click(object sender, EventArgs e)
        {
            LoadFormInPanel(new FormPemesanan());
        }

        private void btnPembayaran_Click(object sender, EventArgs e)
        {
            LoadFormInPanel(new FormPembayaran());
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Yakin mau keluar?", "Konfirmasi Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.Hide(); // Sembunyikan form saat ini
                FormLogin login = new FormLogin(); // Ganti dengan nama form login lo
                login.Show(); // Tampilkan form login
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            MessageBox.Show("Selamat datang, " + Program.Username + "!", "Welcome", MessageBoxButtons.OK, MessageBoxIcon.Information);
            EnsureIndexes();
        }

        private void btn_pemasukan_Click(object sender, EventArgs e)
        {
            LoadFormInPanel(new reportpemasukan());
        }

        private void EnsureIndexes()
        {
            conn.Open();

                string indexScript = @"
                    -- Pelanggan
                    IF OBJECT_ID('dbo.Pelanggan', 'U') IS NOT NULL
                    BEGIN
                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Pelanggan_Nama' AND object_id = OBJECT_ID('dbo.Pelanggan'))
                            CREATE NONCLUSTERED INDEX idx_Pelanggan_Nama ON dbo.Pelanggan(Nama_Pelanggan);

                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Pelanggan_NoTelepon' AND object_id = OBJECT_ID('dbo.Pelanggan'))
                            CREATE NONCLUSTERED INDEX idx_Pelanggan_NoTelepon ON dbo.Pelanggan(NoTelepon);
                    END

                    -- Produk
                    IF OBJECT_ID('dbo.Produk', 'U') IS NOT NULL
                    BEGIN
                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Produk_NamaProduk' AND object_id = OBJECT_ID('dbo.Produk'))
                            CREATE NONCLUSTERED INDEX idx_Produk_NamaProduk ON dbo.Produk(NamaProduk);

                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Produk_Kategori' AND object_id = OBJECT_ID('dbo.Produk'))
                            CREATE NONCLUSTERED INDEX idx_Produk_Kategori ON dbo.Produk(KategoriProduk);
                    END

                    -- Pemesanan
                    IF OBJECT_ID('dbo.Pemesanan', 'U') IS NOT NULL
                    BEGIN
                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Pemesanan_Tanggal' AND object_id = OBJECT_ID('dbo.Pemesanan'))
                            CREATE NONCLUSTERED INDEX idx_Pemesanan_Tanggal ON dbo.Pemesanan(TanggalPemesanan);

                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Pemesanan_Status' AND object_id = OBJECT_ID('dbo.Pemesanan'))
                            CREATE NONCLUSTERED INDEX idx_Pemesanan_Status ON dbo.Pemesanan(Status_Pesanan);
                    END

                    -- Pembayaran
                    IF OBJECT_ID('dbo.Pembayaran', 'U') IS NOT NULL
                    BEGIN
                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Pembayaran_Tanggal' AND object_id = OBJECT_ID('dbo.Pembayaran'))
                            CREATE NONCLUSTERED INDEX idx_Pembayaran_Tanggal ON dbo.Pembayaran(TanggalPembayaran);

                        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Pembayaran_Status' AND object_id = OBJECT_ID('dbo.Pembayaran'))
                            CREATE NONCLUSTERED INDEX idx_Pembayaran_Status ON dbo.Pembayaran(StatusPembayaran);
                    END
                    ";

                using (var cmd = new SqlCommand(indexScript, conn))
                {
                    cmd.ExecuteNonQuery();
                }

            conn.Close();

        }


    }

}
