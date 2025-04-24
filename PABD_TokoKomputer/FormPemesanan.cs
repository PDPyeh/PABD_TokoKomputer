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
            conn.Open();
            SqlCommand cmd = new SqlCommand("INSERT INTO Pemesanan (PelangganID, ProdukID, TanggalPemesanan, Status_Pesanan, Jumlah) VALUES (@pel, @prd, @tgl, @sts, @jml)", conn);
            cmd.Parameters.AddWithValue("@pel", ((KeyValuePair<int, string>)cbPelanggan.SelectedItem).Key);
            cmd.Parameters.AddWithValue("@prd", ((KeyValuePair<int, string>)cbProduk.SelectedItem).Key);
            cmd.Parameters.AddWithValue("@tgl", dtpTanggal.Value);
            cmd.Parameters.AddWithValue("@sts", cbStatus.Text);
            cmd.Parameters.AddWithValue("@jml", int.Parse(txtJumlah.Text));
            cmd.ExecuteNonQuery();
            conn.Close();
            LoadData();
            ClearInput();
            int jumlah;
            if (string.IsNullOrWhiteSpace(txtJumlah.Text) || !int.TryParse(txtJumlah.Text, out jumlah))
            {
                MessageBox.Show("Jumlah harus diisi dengan angka!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            cmd.Parameters.AddWithValue("@jml", jumlah);

        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (selectedID != 0)
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Pemesanan SET PelangganID=@pel, ProdukID=@prd, TanggalPemesanan=@tgl, Status_Pesanan=@sts, Jumlah=@jml WHERE PemesananID=@id", conn);
                cmd.Parameters.AddWithValue("@pel", ((KeyValuePair<int, string>)cbPelanggan.SelectedItem).Key);
                cmd.Parameters.AddWithValue("@prd", ((KeyValuePair<int, string>)cbProduk.SelectedItem).Key);
                cmd.Parameters.AddWithValue("@tgl", dtpTanggal.Value);
                cmd.Parameters.AddWithValue("@sts", cbStatus.Text);
                cmd.Parameters.AddWithValue("@jml", int.Parse(txtJumlah.Text));
                cmd.Parameters.AddWithValue("@id", selectedID);
                cmd.ExecuteNonQuery();
                conn.Close();
                LoadData();
                ClearInput();
                int jumlah;
                if (string.IsNullOrWhiteSpace(txtJumlah.Text) || !int.TryParse(txtJumlah.Text, out jumlah))
                {
                    MessageBox.Show("Jumlah harus diisi dengan angka!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                cmd.Parameters.AddWithValue("@jml", jumlah);

            }
        }


        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (selectedID != 0)
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("DELETE FROM Pemesanan WHERE PemesananID=@id", conn);
                cmd.Parameters.AddWithValue("@id", selectedID);
                cmd.ExecuteNonQuery();
                conn.Close();
                LoadData();
                ClearInput();
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
    }
}
