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
    public partial class FormPemesanan : Form
    {

        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly string _cacheKey = "PelangganData";
        private readonly CacheItemPolicy _cachePolicy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5) // cache selama 5 menit
        };
        
        private int selectedID = 0;
        private int stokSaatIni = 0;
        koneksi kn = new koneksi();


        public FormPemesanan()
        {
            InitializeComponent();
            LoadComboBoxData();
            LoadData();
            cbStatus.Items.Add("Diproses");
            cbStatus.Items.Add("Dikirim");
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
        private void LoadComboBoxData()
        {
            using (SqlConnection conn = new SqlConnection(kn.connectionString()))
            {
                conn.Open();

                // Pelanggan
                SqlCommand cmd1 = new SqlCommand("SELECT PelangganID, Nama_Pelanggan FROM Pelanggan", conn);
                SqlDataReader rdr1 = cmd1.ExecuteReader();
                Dictionary<int, string> pelangganList = new Dictionary<int, string>();
                while (rdr1.Read())
                {
                    pelangganList.Add((int)rdr1["PelangganID"], rdr1["Nama_Pelanggan"].ToString());
                }
                rdr1.Close();
                cbPelanggan.DataSource = new BindingSource(pelangganList, null);
                cbPelanggan.DisplayMember = "Value";
                cbPelanggan.ValueMember = "Key";

                // Produk
                SqlCommand cmd2 = new SqlCommand("SELECT ProdukID, NamaProduk FROM Produk", conn);
                SqlDataReader rdr2 = cmd2.ExecuteReader();
                Dictionary<int, string> produkList = new Dictionary<int, string>();
                while (rdr2.Read())
                {
                    produkList.Add((int)rdr2["ProdukID"], rdr2["NamaProduk"].ToString());
                }
                rdr2.Close();
                cbProduk.DataSource = new BindingSource(produkList, null);
                cbProduk.DisplayMember = "Value";
                cbProduk.ValueMember = "Key";

                conn.Close();
            }
        }

        private void LoadData()
        {
            using (SqlConnection conn = new SqlConnection(kn.connectionString()))
            {
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Pemesanan", conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                    conn.Close();
                }
            }
        }
        private void dtpTanggal_ValueChanged(object sender, EventArgs e)
        {

        }

        private void cbPelanggan_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            int produkID = ((KeyValuePair<int, string>)cbProduk.SelectedItem).Key;
            using (SqlConnection conn = new SqlConnection(kn.connectionString()))
            {
                SqlCommand cmdStok = new SqlCommand("SELECT Stok FROM Produk WHERE ProdukID = @ProdukID", conn);
                cmdStok.Parameters.AddWithValue("@ProdukID", produkID);

            
                conn.Open();
                int stokSaatIni = (int)cmdStok.ExecuteScalar();
                conn.Close();

                if (!IsJumlahValid(txtJumlah.Text))
                {
                    MessageBox.Show("Jumlah pemesanan harus angka dan lebih dari 0!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int jumlahStok = int.Parse(txtJumlah.Text);
                if (jumlahStok > stokSaatIni)
                {
                    MessageBox.Show("Jumlah pemesanan melebihi stok yang tersedia!", "Stok Tidak Cukup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (jumlahStok == stokSaatIni)
                {
                    var result = MessageBox.Show("Stok akan habis setelah pemesanan ini. Lanjutkan?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.No)
                        return;
                }


                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    SqlCommand cmd = new SqlCommand("TambahPemesanan", conn, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PelangganID", ((KeyValuePair<int, string>)cbPelanggan.SelectedItem).Key);
                    cmd.Parameters.AddWithValue("@ProdukID", ((KeyValuePair<int, string>)cbProduk.SelectedItem).Key);
                    cmd.Parameters.AddWithValue("@Jumlah", jumlahStok);
                    cmd.Parameters.AddWithValue("@Status_Pesanan", cbStatus.Text);

                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                    _cache.Remove(_cacheKey);

                    MessageBox.Show("Pemesanan berhasil ditambah!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                    ClearInput();
                }
                catch (SqlException ex)
                {
                    try { transaction.Rollback(); } catch { }
                    MessageBox.Show("Gagal tambah pemesanan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    conn.Close();
                }
            }

        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (selectedID == 0)
            {
                MessageBox.Show("Pilih data pemesanan yang ingin diedit!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!IsJumlahValid(txtJumlah.Text))
            {
                MessageBox.Show("Jumlah pemesanan harus angka dan lebih dari 0!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int jumlahStok = int.Parse(txtJumlah.Text);
            if (jumlahStok > stokSaatIni)
            {
                MessageBox.Show("Jumlah pemesanan melebihi stok yang tersedia!", "Stok Tidak Cukup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Ambil data lama dari DataGridView
            DataGridViewRow selectedRow = dataGridView1.CurrentRow;
            int oldPelangganID = Convert.ToInt32(selectedRow.Cells["PelangganID"].Value);
            int oldProdukID = Convert.ToInt32(selectedRow.Cells["ProdukID"].Value);
            int oldJumlah = Convert.ToInt32(selectedRow.Cells["Jumlah"].Value);
            string oldStatus = selectedRow.Cells["Status_Pesanan"].Value.ToString();
            DateTime oldTanggal = Convert.ToDateTime(selectedRow.Cells["TanggalPemesanan"].Value);

            int newPelangganID = ((KeyValuePair<int, string>)cbPelanggan.SelectedItem).Key;
            int newProdukID = ((KeyValuePair<int, string>)cbProduk.SelectedItem).Key;
            string newStatus = cbStatus.Text;
            DateTime newTanggal = dtpTanggal.Value;

            if (
                oldPelangganID == newPelangganID &&
                oldProdukID == newProdukID &&
                oldJumlah == jumlahStok &&
                oldStatus == newStatus &&
                oldTanggal.Date == newTanggal.Date
            )
            {
                MessageBox.Show("Silakan ubah data terlebih dahulu sebelum menyimpan.", "Tidak Ada Perubahan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(kn.connectionString()))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    SqlCommand cmd = new SqlCommand("UpdatePemesanan", conn, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@PemesananID", selectedID);
                    cmd.Parameters.AddWithValue("@PelangganID", newPelangganID);
                    cmd.Parameters.AddWithValue("@ProdukID", newProdukID);
                    cmd.Parameters.AddWithValue("@Jumlah", jumlahStok);
                    cmd.Parameters.AddWithValue("@Status_Pesanan", newStatus);

                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                    _cache.Remove(_cacheKey);

                    MessageBox.Show("Pemesanan berhasil diupdate!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                    ClearInput();
                }
                catch (SqlException ex)
                {
                    try { transaction.Rollback(); } catch { }
                    MessageBox.Show("Gagal update pemesanan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    conn.Close();
                }
            }
        }


        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (selectedID == 0)
            {
                MessageBox.Show("Pilih data pemesanan yang ingin dihapus!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var konfirmasi = MessageBox.Show("Yakin ingin menghapus data ini?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (konfirmasi != DialogResult.Yes)
                return;

            using (SqlConnection conn = new SqlConnection(kn.connectionString()))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    SqlCommand cmd = new SqlCommand("HapusPemesanan", conn, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PemesananID", selectedID);

                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                    _cache.Remove(_cacheKey);

                    MessageBox.Show("Pemesanan berhasil dihapus!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                    ClearInput();
                }
                catch (SqlException ex)
                {
                    try { transaction.Rollback(); } catch { }
                    MessageBox.Show("Gagal hapus pemesanan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void FormPemesanan_Load(object sender, EventArgs e)
        {
            LoadData();

            // Hubungkan event CellClick ke fungsi handler
            dataGridView1.CellClick += dataGridView1_CellClick;
            dtpTanggal.MinDate = DateTime.Today;
            dtpTanggal.MaxDate = new DateTime(DateTime.Today.Year, 12, 31);
            EnsureIndexes();
        }

        private void cbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ClearInput()
        {
            cbPelanggan.SelectedIndex = -1;
            cbProduk.SelectedIndex = -1;
            cbStatus.SelectedIndex = -1;
            txtJumlah.Text = "";
            dtpTanggal.Value = DateTime.Now;
            selectedID = 0;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                selectedID = Convert.ToInt32(row.Cells["PemesananID"].Value);
                cbPelanggan.SelectedValue = Convert.ToInt32(row.Cells["PelangganID"].Value);
                cbProduk.SelectedValue = Convert.ToInt32(row.Cells["ProdukID"].Value);
                DateTime tanggal;
                if (DateTime.TryParse(row.Cells["TanggalPemesanan"].Value.ToString(), out tanggal))
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
                cbStatus.Text = row.Cells["Status_Pesanan"].Value.ToString();
                txtJumlah.Text = row.Cells["Jumlah"].Value.ToString();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void dtpTanggal_ValueChanged_1(object sender, EventArgs e)
        {

        }

        private bool IsJumlahValid(string input)
        {
            if (int.TryParse(input, out int jumlah))
            {
                return jumlah > 0;
            }
            return false;
        }

        private void cbProduk_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbProduk.SelectedItem != null)
            {
                int produkID = ((KeyValuePair<int, string>)cbProduk.SelectedItem).Key;

                using (SqlConnection tempConn = new SqlConnection(kn.connectionString()))
                {

                    tempConn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT Stok FROM Produk WHERE ProdukID = @id", tempConn))
                    {
                        cmd.Parameters.AddWithValue("@id", produkID);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            stokSaatIni = Convert.ToInt32(result);
                            LblStok.Text = "Stok: " + stokSaatIni.ToString();
                        }
                    }
                    tempConn.Close();
                }
            }
        }




    }
}
