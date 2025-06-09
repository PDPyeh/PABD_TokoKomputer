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
                MessageBox.Show("No. Telepon hanya boleh angka!");
                return;
            }

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

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (selectedPelangganId == -1)
            {
                MessageBox.Show("Pilih data pelanggan yang akan diubah.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validasi input
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

            conn.Open();
            SqlTransaction transaction = conn.BeginTransaction();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_DeletePelanggan", conn, transaction);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@PelangganID", selectedPelangganId);

                cmd.ExecuteNonQuery();
                transaction.Commit();

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
