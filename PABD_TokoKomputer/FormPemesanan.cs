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
    public partial class FormPemesanan : Form
    {
        private SqlConnection conn = new SqlConnection("Data Source=LAPTOP-Q7EVPB6K\\PRADIPAYOGANANDA;Initial Catalog=SistemTokoComputerPABD_1;Integrated Security=True");
        private int selectedID = 0;
        private int stokSaatIni = 0;


        public FormPemesanan()
        {
            InitializeComponent();
            LoadComboBoxData();
            LoadData();
            cbStatus.Items.Add("Diproses");
            cbStatus.Items.Add("Dikirim");
        }
        private void LoadComboBoxData()
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

        private void LoadData()
        {
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Pemesanan", conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dataGridView1.DataSource = dt;
            conn.Close();
        }
        private void dtpTanggal_ValueChanged(object sender, EventArgs e)
        {

        }

        private void cbPelanggan_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnTambah_Click(object sender, EventArgs e)
        {

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

            conn.Open();
            SqlTransaction transaction = conn.BeginTransaction();

            try
            {
                SqlCommand cmd = new SqlCommand("UpdatePemesanan", conn, transaction);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@PemesananID", selectedID);
                cmd.Parameters.AddWithValue("@PelangganID", ((KeyValuePair<int, string>)cbPelanggan.SelectedItem).Key);
                cmd.Parameters.AddWithValue("@ProdukID", ((KeyValuePair<int, string>)cbProduk.SelectedItem).Key);
                cmd.Parameters.AddWithValue("@Jumlah", jumlahStok);
                cmd.Parameters.AddWithValue("@Status_Pesanan", cbStatus.Text);

                cmd.ExecuteNonQuery();
                transaction.Commit();

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

            conn.Open();
            SqlTransaction transaction = conn.BeginTransaction();

            try
            {
                SqlCommand cmd = new SqlCommand("HapusPemesanan", conn, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PemesananID", selectedID);

                cmd.ExecuteNonQuery();
                transaction.Commit();

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
                dtpTanggal.Value = Convert.ToDateTime(row.Cells["TanggalPemesanan"].Value);
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

                using (SqlConnection tempConn = new SqlConnection(conn.ConnectionString))
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
