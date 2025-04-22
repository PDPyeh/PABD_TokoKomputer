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
    public partial class FormPelanggan : Form
    {
        SqlConnection conn = new SqlConnection(Database.ConnectionString);
        int selectedID = 0;

        public FormPelanggan()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Pelanggan", conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dataGridView1.DataSource = dt;
            conn.Close();
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (txtNama.Text != "" && txtAlamat.Text != "" && txtNoTelepon.Text != "")
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO Pelanggan (Nama_Pelanggan, Alamat, NoTelepon) VALUES (@nama, @alamat, @no)", conn);
                cmd.Parameters.AddWithValue("@nama", txtNama.Text);
                cmd.Parameters.AddWithValue("@alamat", txtAlamat.Text);
                cmd.Parameters.AddWithValue("@no", txtNoTelepon.Text);
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
                SqlCommand cmd = new SqlCommand("UPDATE Pelanggan SET Nama_Pelanggan=@nama, Alamat=@alamat, NoTelepon=@no WHERE PelangganID=@id", conn);
                cmd.Parameters.AddWithValue("@nama", txtNama.Text);
                cmd.Parameters.AddWithValue("@alamat", txtAlamat.Text);
                cmd.Parameters.AddWithValue("@no", txtNoTelepon.Text);
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
                SqlCommand cmd = new SqlCommand("DELETE FROM Pelanggan WHERE PelangganID=@id", conn);
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
                selectedID = Convert.ToInt32(row.Cells["PelangganID"].Value);
                txtNama.Text = row.Cells["Nama_Pelanggan"].Value.ToString();
                txtAlamat.Text = row.Cells["Alamat"].Value.ToString();
                txtNoTelepon.Text = row.Cells["NoTelepon"].Value.ToString();
            }
        }

        private void ClearInput()
        {
            txtNama.Text = "";
            txtAlamat.Text = "";
            txtNoTelepon.Text = "";
            selectedID = 0;
        }
    }
}
