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

namespace UCP1PABD
{
    public partial class FormPelanggan : Form
    {
        private SqlConnection conn = new SqlConnection("Data Source=LAPTOP-Q7EVPB6K\\PRADIPAYOGANANDA;Initial Catalog=SistemTokoComputerPABD_1;Integrated Security=True");
        private int selectedPelangganId = -1;
        public FormPelanggan()
        {
            InitializeComponent();
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (txtNama.Text != "" && txtAlamat.Text != "" && txtNoTelepon.Text != "")
            {
                if (!IsNamaValid(txtNama.Text))
                {
                    MessageBox.Show("Nama tidak boleh mengandung angka!");
                    return;
                }

                if (!IsNoTeleponValid(txtNoTelepon.Text))
                {
                    MessageBox.Show("No. Telepon hanya boleh angka!");
                    return;
                }

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
            if (selectedPelangganId != -1)
            {
                if (!IsNamaValid(txtNama.Text))
                {
                    MessageBox.Show("Nama tidak boleh mengandung angka!");
                    return;
                }

                if (!IsNoTeleponValid(txtNoTelepon.Text))
                {
                    MessageBox.Show("No. Telepon hanya boleh angka!");
                    return;
                }

                conn.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Pelanggan SET Nama_Pelanggan = @nama, Alamat = @alamat, NoTelepon = @no WHERE PelangganID = @id", conn);
                cmd.Parameters.AddWithValue("@nama", txtNama.Text);
                cmd.Parameters.AddWithValue("@alamat", txtAlamat.Text);
                cmd.Parameters.AddWithValue("@no", txtNoTelepon.Text);
                cmd.Parameters.AddWithValue("@id", selectedPelangganId);
                cmd.ExecuteNonQuery();
                conn.Close();
                LoadData();
                ClearInput();
                selectedPelangganId = -1;
            }
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (selectedPelangganId != -1)
            {
                var result = MessageBox.Show("Yakin ingin menghapus data ini?", "Konfirmasi", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM Pelanggan WHERE PelangganID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", selectedPelangganId);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    LoadData();
                    ClearInput();
                    selectedPelangganId = -1;
                }
            }
            else
            {
                MessageBox.Show("Pilih data yang ingin dihapus terlebih dahulu.");
            }
        }

        private void LoadData()
        {
            SqlDataAdapter da = new SqlDataAdapter("SELECT PelangganID, Nama_Pelanggan, Alamat, NoTelepon FROM Pelanggan", conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dataGridView1.DataSource = dt;
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
        
        }

        private bool IsNamaValid(string nama)
        {
            return !nama.Any(char.IsDigit); // true kalau tidak ada angka
        }

        private bool IsNoTeleponValid(string no)
        {
            return no.All(char.IsDigit); // true kalau semua karakter adalah angka
        }

    }
}
