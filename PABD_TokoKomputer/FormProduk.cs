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

namespace PABD_TokoKomputer
{
    public partial class FormProduk : Form
    {
        SqlConnection conn = new SqlConnection(Database.ConnectionString);
        int selectedID = 0;

        public FormProduk()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Produk", conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dataGridView1.DataSource = dt;
            conn.Close();
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (txtNamaProduk.Text != "" && txtMerk.Text != "" && txtKategori.Text != "" && txtHarga.Text != "")
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO Produk (NamaProduk, NamaMerk, KategoriProduk, Harga) VALUES (@nama, @merk, @kategori, @harga)", conn);
                cmd.Parameters.AddWithValue("@nama", txtNamaProduk.Text);
                cmd.Parameters.AddWithValue("@merk", txtMerk.Text);
                cmd.Parameters.AddWithValue("@kategori", txtKategori.Text);
                cmd.Parameters.AddWithValue("@harga", decimal.Parse(txtHarga.Text));
                cmd.ExecuteNonQuery();
                conn.Close();
                LoadData();
                ClearInput();
            }
            else MessageBox.Show("Lengkapi semua input!");
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (selectedID != 0)
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Produk SET NamaProduk=@nama, NamaMerk=@merk, KategoriProduk=@kategori, Harga=@harga WHERE ProdukID=@id", conn);
                cmd.Parameters.AddWithValue("@nama", txtNamaProduk.Text);
                cmd.Parameters.AddWithValue("@merk", txtMerk.Text);
                cmd.Parameters.AddWithValue("@kategori", txtKategori.Text);
                cmd.Parameters.AddWithValue("@harga", decimal.Parse(txtHarga.Text));
                cmd.Parameters.AddWithValue("@id", selectedID);
                cmd.ExecuteNonQuery();
                conn.Close();
                LoadData();
                ClearInput();
            }
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (selectedID != 0)
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM Produk WHERE ProdukID=@id", conn);
                cmd.Parameters.AddWithValue("@id", selectedID);
                cmd.ExecuteNonQuery();
                conn.Close();
                LoadData();
                ClearInput();
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                selectedID = Convert.ToInt32(row.Cells["ProdukID"].Value);
                txtNamaProduk.Text = row.Cells["NamaProduk"].Value.ToString();
                txtMerk.Text = row.Cells["NamaMerk"].Value.ToString();
                txtKategori.Text = row.Cells["KategoriProduk"].Value.ToString();
                txtHarga.Text = row.Cells["Harga"].Value.ToString();
            }
        }

        private void ClearInput()
        {
            txtNamaProduk.Text = "";
            txtMerk.Text = "";
            txtKategori.Text = "";
            txtHarga.Text = "";
            selectedID = 0;
        }
    }
}
