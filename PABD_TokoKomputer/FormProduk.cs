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

namespace UCP1PABD
{
    public partial class FormProduk : Form
    {

        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly string _cacheKey = "PelangganData";
        private readonly CacheItemPolicy _cachePolicy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5) // cache selama 5 menit
        };
        private SqlConnection conn = new SqlConnection("Data Source=LAPTOP-Q7EVPB6K\\PRADIPAYOGANANDA;Initial Catalog=SistemTokoComputerPABD_1;Integrated Security=True");
        private int selectedID = 0;

        public FormProduk()
        {
            InitializeComponent();
        }

        private void LoadData()
        {
            SqlDataAdapter da = new SqlDataAdapter("SELECT ProdukID, NamaProduk, NamaMerk, KategoriProduk, Harga, Stok FROM Produk", conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dataGridView1.DataSource = dt;
        

        }

        private void ClearInput()
        {
            txtNamaProduk.Clear();
            txtMerk.Clear();
            txtKategori.Clear();
            txtHarga.Clear();
            txtStok.Clear();

        }

        private void txtNamaProduk_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (txtNamaProduk.Text != "" && txtMerk.Text != "" && txtKategori.Text != "" && txtHarga.Text != "" && txtStok.Text != "")
            {
                if (!IsHargaValid(txtHarga.Text))
                {
                    MessageBox.Show("Harga hanya boleh angka!");
                    return;
                }

                if (!IsStokValid(txtStok.Text))
                {
                    MessageBox.Show("Stok harus berupa angka 0 atau lebih!");
                    return;
                }

                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    SqlCommand cmd = new SqlCommand("sp_InsertProduk", conn, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@nama", txtNamaProduk.Text);
                    cmd.Parameters.AddWithValue("@merk", txtMerk.Text);
                    cmd.Parameters.AddWithValue("@kategori", txtKategori.Text);
                    cmd.Parameters.AddWithValue("@harga", decimal.Parse(txtHarga.Text));
                    cmd.Parameters.AddWithValue("@stok", int.Parse(txtStok.Text));

                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                    _cache.Remove(_cacheKey);

                    MessageBox.Show("Data produk berhasil ditambahkan!");
                    LoadData();
                    ClearInput();
                }
                catch (SqlException ex)
                {
                    try { transaction.Rollback(); } catch { }
                    MessageBox.Show("Gagal tambah produk: " + ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
            else
            {
                MessageBox.Show("Lengkapi semua input!");
            }
        }

        private void FormProduk_Load(object sender, EventArgs e)
        {
            LoadData();
            dataGridView1.CellClick += dataGridView1_CellClick;
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (selectedID != 0)
            {
                var result = MessageBox.Show("Yakin ingin menghapus produk ini?", "Konfirmasi", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        SqlCommand cmd = new SqlCommand("sp_DeleteProduk", conn, transaction);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id", selectedID);

                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                        _cache.Remove(_cacheKey);

                        MessageBox.Show("Data produk berhasil dihapus!");
                        LoadData();
                        ClearInput();
                        selectedID = 0;
                    }
                    catch (SqlException ex)
                    {
                        try { transaction.Rollback(); } catch { }
                        MessageBox.Show("Gagal hapus produk: " + ex.Message);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            else
            {
                MessageBox.Show("Pilih data yang ingin dihapus terlebih dahulu.");
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = this.dataGridView1.Rows[e.RowIndex];
                selectedID = Convert.ToInt32(row.Cells["ProdukID"].Value);
                txtNamaProduk.Text = row.Cells["NamaProduk"].Value.ToString();
                txtMerk.Text = row.Cells["NamaMerk"].Value.ToString();
                txtKategori.Text = row.Cells["KategoriProduk"].Value.ToString();
                txtHarga.Text = row.Cells["Harga"].Value.ToString();
                txtStok.Text = row.Cells["Stok"].Value.ToString();

            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (selectedID != 0)
            {
                if (txtNamaProduk.Text != "" && txtMerk.Text != "" && txtKategori.Text != "" && txtHarga.Text != "" && txtStok.Text != "")
                {
                    if (!IsHargaValid(txtHarga.Text))
                    {
                        MessageBox.Show("Harga hanya boleh angka!");
                        return;
                    }

                    if (!IsStokValid(txtStok.Text))
                    {
                        MessageBox.Show("Stok harus berupa angka 0 atau lebih!");
                        return;
                    }

                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        SqlCommand cmd = new SqlCommand("sp_UpdateProduk", conn, transaction);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id", selectedID);
                        cmd.Parameters.AddWithValue("@nama", txtNamaProduk.Text);
                        cmd.Parameters.AddWithValue("@merk", txtMerk.Text);
                        cmd.Parameters.AddWithValue("@kategori", txtKategori.Text);
                        cmd.Parameters.AddWithValue("@harga", decimal.Parse(txtHarga.Text));
                        cmd.Parameters.AddWithValue("@stok", int.Parse(txtStok.Text));

                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                        _cache.Remove(_cacheKey);

                        MessageBox.Show("Data produk berhasil diubah!");
                        LoadData();
                        ClearInput();
                    }
                    catch (SqlException ex)
                    {
                        try { transaction.Rollback(); } catch { }
                        MessageBox.Show("Gagal ubah produk: " + ex.Message);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Lengkapi semua input untuk mengedit!");
                }
            }
            else
            {
                MessageBox.Show("Pilih data yang ingin diedit terlebih dahulu.");
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void FormProduk_Load_1(object sender, EventArgs e)
        {
            LoadData();
            dataGridView1.CellClick += dataGridView1_CellClick;
        }


        private bool IsHargaValid(string input)
        {
            return decimal.TryParse(input, out _);
        }


        private bool IsStokValid(string input)
        {
            return int.TryParse(input, out int stok) && stok >= 0;
        }

        private void txtStok_TextChanged(object sender, EventArgs e)
        {

        }
    }

}
