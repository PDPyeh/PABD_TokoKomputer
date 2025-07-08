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
using System.Runtime.Caching;
using PABD_TokoKomputer;

namespace UCP1PABD
{
    public partial class FormPembayaran : Form
    {

        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly string _cacheKey = "PelangganData";
        private readonly CacheItemPolicy _cachePolicy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5) // cache selama 5 menit
        };
        
        int selectedID = 0;
        koneksi kn = new koneksi();
        public FormPembayaran()
        {
            InitializeComponent();

            cbStatusBayar.Items.Add("Sukses");
            cbStatusBayar.Items.Add("Gagal");

            
            LoadComboBox();
            LoadData();
        }




        private void LoadComboBox()
        {
            using (SqlConnection conn = new SqlConnection(kn.connectionString()))
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter("SELECT Pemesanan.PemesananID, Pelanggan.Nama_Pelanggan FROM Pemesanan JOIN Pelanggan ON Pemesanan.PelangganID = Pelanggan.PelangganID", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                conn.Close();

                cbPemesanan.DataSource = dt;
                cbPemesanan.DisplayMember = "Nama_Pelanggan";
                cbPemesanan.ValueMember = "PemesananID";
            }
        }

        private void EnsureIndexes()
        {
            using (SqlConnection conn = new SqlConnection(kn.connectionString()))
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

        private void LoadData()
        {
            using (SqlConnection conn = new SqlConnection(kn.connectionString()))
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Pembayaran", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridView1.DataSource = dt;
                conn.Close();
                dtpTanggal.MinDate = DateTime.Today;
                dtpTanggal.MaxDate = new DateTime(DateTime.Today.Year, 12, 31);
            }
        }

        private void FormPembayaran_Load(object sender, EventArgs e)
        {

            cbStatusBayar.Items.Clear(); // bersihin dulu
            cbStatusBayar.Items.Add("Sukses");
            cbStatusBayar.Items.Add("Gagal");

            dataGridView1.Dock = DockStyle.Fill;
            EnsureIndexes();



        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(kn.connectionString()))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    decimal nominalDecimal;
                    string nominalStr = LblJumlah.Text.Replace("Rp", "").Replace(" ", "").Replace(",", "");
                    if (!decimal.TryParse(nominalStr, out nominalDecimal))
                    {
                        MessageBox.Show("Format jumlah pembayaran tidak valid!");
                        return;
                    }


                    SqlCommand cmd = new SqlCommand("sp_InsertPembayaran", conn, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PemesananID", cbPemesanan.SelectedValue);
                    cmd.Parameters.AddWithValue("@TanggalPembayaran", dtpTanggal.Value.Date);
                    cmd.Parameters.AddWithValue("@JumlahPembayaran", nominalDecimal);
                    cmd.Parameters.AddWithValue("@StatusPembayaran", cbStatusBayar.Text);

                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                    _cache.Remove(_cacheKey);
                    MessageBox.Show("Data pembayaran berhasil ditambahkan.");
                }
                catch (Exception ex)
                {
                    try { transaction.Rollback(); } catch { }
                    MessageBox.Show("Gagal tambah data: " + ex.Message);
                }
                finally
                {
                    conn.Close();
                    LoadData();
                    ClearInput();
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (selectedID == 0) return;

            using (SqlConnection conn = new SqlConnection(kn.connectionString()))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    decimal nominalDecimal;
                    string nominalStr = LblJumlah.Text.Replace("Rp", "").Replace(" ", "").Replace(",", "");
                    if (!decimal.TryParse(nominalStr, out nominalDecimal))
                    {
                        MessageBox.Show("Format jumlah pembayaran tidak valid!");
                        return;
                    }



                    SqlCommand cmd = new SqlCommand("sp_UpdatePembayaran", conn, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PembayaranID", selectedID);
                    cmd.Parameters.AddWithValue("@PemesananID", cbPemesanan.SelectedValue);
                    cmd.Parameters.AddWithValue("@TanggalPembayaran", dtpTanggal.Value.Date);
                    cmd.Parameters.AddWithValue("@JumlahPembayaran", nominalDecimal);
                    cmd.Parameters.AddWithValue("@StatusPembayaran", cbStatusBayar.Text);

                    cmd.ExecuteNonQuery();
                    _cache.Remove(_cacheKey);
                    transaction.Commit();
                    MessageBox.Show("Data pembayaran berhasil diupdate.");
                }
                catch (Exception ex)
                {
                    try { transaction.Rollback(); } catch { }
                    MessageBox.Show("Gagal update data: " + ex.Message);
                }
                finally
                {
                    conn.Close();
                    LoadData();
                    ClearInput();
                }
            }
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (selectedID == 0) return;

            using (SqlConnection conn = new SqlConnection(kn.connectionString()))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {

                    SqlCommand cmd = new SqlCommand("sp_DeletePembayaran", conn, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PembayaranID", selectedID);

                    cmd.ExecuteNonQuery();
                    _cache.Remove(_cacheKey);
                    transaction.Commit();
                    MessageBox.Show("Data pembayaran berhasil dihapus.");
                }
                catch (Exception ex)
                {
                    try { transaction.Rollback(); } catch { }
                    MessageBox.Show("Gagal hapus data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    conn.Close();
                    LoadData();
                    ClearInput();
                }
            }
        }


        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                selectedID = Convert.ToInt32(row.Cells["PembayaranID"].Value);
                cbPemesanan.Text = row.Cells["PemesananID"].Value.ToString();
                dtpTanggal.Value = Convert.ToDateTime(row.Cells["TanggalPembayaran"].Value);
                LblJumlah.Text = row.Cells["JumlahPembayaran"].Value.ToString();
                cbStatusBayar.Text = row.Cells["StatusPembayaran"].Value.ToString();
            }

        }

        private void ClearInput()
        {
            cbPemesanan.SelectedIndex = -1;
            cbStatusBayar.SelectedIndex = -1;
            LblJumlah.Text = "Rp 0";
            dtpTanggal.Value = DateTime.Now;
            selectedID = 0;
        }

        private void cbPemesanan_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cbPemesanan.SelectedValue != null)
            {
                using (SqlConnection conn = new SqlConnection(kn.connectionString()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
            SELECT pr.Harga, pm.Jumlah
            FROM Pemesanan pm
            JOIN Produk pr ON pm.ProdukID = pr.ProdukID
            WHERE pm.PemesananID = @id", conn);

                    cmd.Parameters.AddWithValue("@id", cbPemesanan.SelectedValue);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        decimal harga = Convert.ToDecimal(reader["Harga"]);
                        int jumlah = Convert.ToInt32(reader["Jumlah"]);
                        decimal total = harga * jumlah;

                        LblJumlah.Text = "Rp " + total.ToString("N0"); // tampilkan dengan format

                    }

                    reader.Close();
                    conn.Close();
                }
            }
        }



        private void cbStatusBayar_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txtJumlah_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void FormPembayaran_Load_1(object sender, EventArgs e)
        {
            cbStatusBayar.Items.Clear(); // bersihin dulu
            cbStatusBayar.Items.Add("Sukses");
            cbStatusBayar.Items.Add("Gagal");
            dataGridView1.CellClick += dataGridView1_CellContentClick_1;
            LoadData();
        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {

                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                selectedID = Convert.ToInt32(row.Cells["PembayaranID"].Value);
                cbPemesanan.SelectedValue = Convert.ToInt32(row.Cells["PemesananID"].Value);
                DateTime tanggal;
                if (DateTime.TryParse(row.Cells["TanggalPembayaran"].Value.ToString(), out tanggal))
                {
                    if (tanggal >= dtpTanggal.MinDate && tanggal <= dtpTanggal.MaxDate)
                    {
                        dtpTanggal.Value = tanggal;
                    }
                    else
                    {
                        dtpTanggal.Value = DateTime.Now;
                    }
                }
                else
                {
                    dtpTanggal.Value = DateTime.Now;
                }
                LblJumlah.Text = row.Cells["JumlahPembayaran"].Value.ToString();
                cbStatusBayar.Text = row.Cells["StatusPembayaran"].Value.ToString();
                decimal jumlah = Convert.ToDecimal(row.Cells["JumlahPembayaran"].Value);
                LblJumlah.Text = "Rp " + jumlah.ToString("N2");
            }
        }

        private void cbPemesanan_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void dtpTanggal_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}