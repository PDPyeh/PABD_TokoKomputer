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
    public partial class FormPembayaran : Form
    {
        private SqlConnection conn = new SqlConnection("Data Source=LAPTOP-Q7EVPB6K\\PRADIPAYOGANANDA;Initial Catalog=SistemTokoComputerPABD_1;Integrated Security=True");
        int selectedID = 0;
        public FormPembayaran()
        {
            InitializeComponent();
            MessageBox.Show(cbStatusBayar == null ? "ComboBox NULL" : "ComboBox OK");

            LoadComboBox();
            LoadData();

        }

        private void LoadComboBox()
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("SELECT PemesananID FROM Pemesanan", conn);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                cbPemesanan.Items.Add(reader["PemesananID"].ToString());
            }
            reader.Close();
            conn.Close();
        }

        private void LoadData()
        {
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Pembayaran", conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dataGridView1.DataSource = dt;
            conn.Close();
        }

        private void FormPembayaran_Load(object sender, EventArgs e)
        {

            cbStatusBayar.Items.Clear(); // bersihin dulu
            cbStatusBayar.Items.Add("Sukses");
            cbStatusBayar.Items.Add("Gagal");

        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("INSERT INTO Pembayaran (PemesananID, TanggalPembayaran, JumlahPembayaran, StatusPembayaran) VALUES (@id, @tgl, @jml, @sts)", conn);
            cmd.Parameters.AddWithValue("@id", int.Parse(cbPemesanan.Text));
            cmd.Parameters.AddWithValue("@tgl", dtpTanggal.Value);
            cmd.Parameters.AddWithValue("@jml", decimal.Parse(txtJumlah.Text));
            cmd.Parameters.AddWithValue("@sts", cbStatusBayar.Text);
            cmd.ExecuteNonQuery();
            conn.Close();
            LoadData();
            ClearInput();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (selectedID != 0)
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Pembayaran SET PemesananID=@id, TanggalPembayaran=@tgl, JumlahPembayaran=@jml, StatusPembayaran=@sts WHERE PembayaranID=@payid", conn);
                cmd.Parameters.AddWithValue("@id", int.Parse(cbPemesanan.Text));
                cmd.Parameters.AddWithValue("@tgl", dtpTanggal.Value);
                cmd.Parameters.AddWithValue("@jml", decimal.Parse(txtJumlah.Text));
                cmd.Parameters.AddWithValue("@sts", cbStatusBayar.Text);
                cmd.Parameters.AddWithValue("@payid", selectedID);
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
                SqlCommand cmd = new SqlCommand("DELETE FROM Pembayaran WHERE PembayaranID=@id", conn);
                cmd.Parameters.AddWithValue("@id", selectedID);
                cmd.ExecuteNonQuery();
                conn.Close();
                LoadData();
                ClearInput();
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
                txtJumlah.Text = row.Cells["JumlahPembayaran"].Value.ToString();
                cbStatusBayar.Text = row.Cells["StatusPembayaran"].Value.ToString();
            }

        }

        private void ClearInput()
        {
            cbPemesanan.SelectedIndex = -1;
            cbStatusBayar.SelectedIndex = -1;
            txtJumlah.Clear();
            dtpTanggal.Value = DateTime.Now;
            selectedID = 0;
        }

        private void cbStatusBayar_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
