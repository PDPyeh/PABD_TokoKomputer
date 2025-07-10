using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.Caching;
using OfficeOpenXml;
using System.IO;
using PABD_TokoKomputer;
using System.Diagnostics;

namespace UCP1PABD
{
    public partial class FormPelanggan : Form
    {

        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly string _cacheKey = "PelangganData";
        private readonly CacheItemPolicy _cachePolicy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5) // cache selama 5 menit
        };

        koneksi kn = new koneksi();

        private int selectedPelangganId = -1;
        public FormPelanggan()
        {
            InitializeComponent();
            dataGridView1.CellClick += dataGridView1_CellClick;

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





        private void btnTambah_Click(object sender, EventArgs e)
        {
            // Validasi input harus lengkap
            if (txtNama.Text == "" || txtAlamat.Text == "" || txtNoTelepon.Text == "")
            {
                MessageBox.Show("Lengkapi semua input!");
                return;
            }

            // Validasi nama dan no telepon
            if (!IsNamaValid(txtNama.Text))
            {
                MessageBox.Show("Nama tidak boleh mengandung angka!");
                return;
            }

            if (!IsNoTeleponValid(txtNoTelepon.Text))
            {
                MessageBox.Show("No. Telepon hanya boleh angka, minimal 9 digit dan maximal 13 !");
                return;
            }

            if (IsDuplicatePelanggan(txtNama.Text, txtNoTelepon.Text))
            {
                MessageBox.Show("Data pelanggan dengan nama dan nomor telepon yang sama sudah ada!", "Duplikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(kn.connectionString()))
            {
                conn.Open();
                // Mulai transaction di sisi client
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    SqlCommand cmd = new SqlCommand("sp_InsertPelanggan", conn, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Set parameter sesuai stored procedure
                    cmd.Parameters.AddWithValue("@Nama_Pelanggan", txtNama.Text);
                    cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text);
                    cmd.Parameters.AddWithValue("@NoTelepon", txtNoTelepon.Text);

                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                    _cache.Remove(_cacheKey);

                    MessageBox.Show("Data pelanggan berhasil ditambahkan.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                    ClearInput();
                }
                catch (SqlException ex)
                {
                    try { transaction.Rollback(); } catch { }
                    MessageBox.Show("Gagal tambah pelanggan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(kn.connectionString()))
            {
                if (selectedPelangganId == -1)
                {
                    MessageBox.Show("Pilih data pelanggan yang akan diubah.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DataGridViewRow selectedRow = dataGridView1.CurrentRow;
                string oldNama = selectedRow.Cells["Nama_Pelanggan"].Value.ToString();
                string oldAlamat = selectedRow.Cells["Alamat"].Value.ToString();
                string oldTelepon = selectedRow.Cells["NoTelepon"].Value.ToString();

                // Cek apakah data berubah
                if (txtNama.Text == oldNama && txtAlamat.Text == oldAlamat && txtNoTelepon.Text == oldTelepon)
                {
                    MessageBox.Show("Silakan ubah data sebelum menyimpan!", "Tidak Ada Perubahan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validasi duplikat
                if (IsDuplicatePelanggan(txtNama.Text, txtNoTelepon.Text, selectedPelangganId))
                {
                    MessageBox.Show("Data pelanggan dengan nama dan nomor telepon yang sama sudah ada!", "Duplikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validasi input
                if (!IsNamaValid(txtNama.Text))
                {
                    MessageBox.Show("Nama tidak boleh mengandung angka!");
                    return;
                }

                if (txtNoTelepon.Enabled && !IsNoTeleponValid(txtNoTelepon.Text))
                {
                    MessageBox.Show("No. Telepon hanya boleh angka, minimal 9 digit dan maximal 13 !");
                    return;
                }
            
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_UpdatePelanggan", conn, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@PelangganID", selectedPelangganId);
                    cmd.Parameters.AddWithValue("@Nama_Pelanggan", txtNama.Text);
                    cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text);
                    cmd.Parameters.AddWithValue("@NoTelepon", txtNoTelepon.Text);

                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                    _cache.Remove(_cacheKey);

                    MessageBox.Show("Data pelanggan berhasil diupdate.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                    ClearInput();
                    selectedPelangganId = -1;
                }
                catch (SqlException ex)
                {
                    try { transaction.Rollback(); } catch { }
                    MessageBox.Show("Gagal update pelanggan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (selectedPelangganId == -1)
            {
                MessageBox.Show("Pilih data pelanggan yang akan dihapus.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Yakin ingin menghapus data ini?", "Konfirmasi", MessageBoxButtons.YesNo);
            if (result != DialogResult.Yes)
                return;
            using (SqlConnection conn = new SqlConnection(kn.connectionString()))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    SqlCommand cmd = new SqlCommand("sp_DeletePelanggan", conn, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@PelangganID", selectedPelangganId);

                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                    _cache.Remove(_cacheKey);

                    MessageBox.Show("Data pelanggan berhasil dihapus.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                    ClearInput();
                    selectedPelangganId = -1;
                }
                catch (SqlException ex)
                {
                    try { transaction.Rollback(); } catch { }
                    MessageBox.Show("Gagal hapus pelanggan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private void LoadData()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            DataTable dt;

            if (_cache.Contains(_cacheKey))
            {
                dt = (DataTable)_cache.Get(_cacheKey); // Ambil dari cache
            }
            else
            {
                dt = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter("SELECT PelangganID, Nama_Pelanggan, Alamat, NoTelepon FROM Pelanggan", kn.connectionString()))
                {
                    da.Fill(dt);
                }
                _cache.Add(_cacheKey, dt, _cachePolicy); // Simpan ke cache
            }

            dataGridView1.DataSource = dt;
            sw.Stop();

            if (lblLoadTime != null)
                lblLoadTime.Text = $"Waktu Load: {sw.ElapsedMilliseconds} ms ";
        }


        private void ClearInput()
        {
            txtNama.Clear();
            txtAlamat.Clear();
            txtNoTelepon.Clear();
        }

        private void FormPelanggan_Load(object sender, EventArgs e)
        {
            LoadData();

            // Hubungkan event CellClick ke fungsi handler
            dataGridView1.CellClick += dataGridView1_CellClick;
            EnsureIndexes();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                // Gunakan nama kolom baru "ID"
                selectedPelangganId = Convert.ToInt32(row.Cells["PelangganID"].Value);
                txtNama.Text = row.Cells["Nama_Pelanggan"].Value.ToString();
                txtAlamat.Text = row.Cells["Alamat"].Value.ToString();
                txtNoTelepon.Text = row.Cells["NoTelepon"].Value.ToString();
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void FormPelanggan_Load_1(object sender, EventArgs e)
        {
            LoadData();

            // Hubungkan event CellClick ke fungsi handler
            dataGridView1.CellClick += dataGridView1_CellClick;
            EnsureIndexes();

        }

        private bool IsNamaValid(string nama)
        {
            return !nama.Any(char.IsDigit); // true kalau tidak ada angka
        }

        private bool IsNoTeleponValid(string no)
        {
            no = no.Trim(); // hapus spasi depan-belakang
            return !string.IsNullOrEmpty(no) && no.All(char.IsDigit) && no.Length >= 9 && no.Length <= 13;
        }

        private bool IsDuplicatePelanggan(string nama, string noTelepon, int excludeId = -1)
        {
            using (SqlConnection conn = new SqlConnection(kn.connectionString()))
            {
                conn.Open();
                string query = @"SELECT COUNT(*) FROM Pelanggan 
                         WHERE Nama_Pelanggan = @nama 
                         AND NoTelepon = @noTelp 
                         AND PelangganID != @id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@nama", nama);
                    cmd.Parameters.AddWithValue("@noTelp", noTelepon);
                    cmd.Parameters.AddWithValue("@id", excludeId); // untuk pengecualian data yang sedang di-edit

                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

    }
}
