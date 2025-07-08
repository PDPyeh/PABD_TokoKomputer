using PABD_TokoKomputer;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PABD_TokoKomputer
{
    public partial class FormLogin : Form
    {
        koneksi kn = new koneksi();
        string strkonek = "";
        public FormLogin()
        {
            InitializeComponent();
            strkonek = kn.connectionString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(strkonek))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM Users WHERE Username=@user AND Password=@pass";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@user", textBox1.Text);
                cmd.Parameters.AddWithValue("@pass", textBox2.Text);

                int count = (int)cmd.ExecuteScalar();
                if (count > 0 && textBox1.Text == "admin") // hanya admin
                {
                    Program.IsLoggedIn = true;
                    Program.Username = textBox1.Text;
                    this.Close(); // kembali ke Program.cs dan buka MainForm
                }
                else
                {
                    MessageBox.Show("Username atau Password Salah! silahkan diulang");
                }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Yakin mau keluar?", "Konfirmasi Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }
    }
}
