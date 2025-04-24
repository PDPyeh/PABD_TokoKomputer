using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UCP1PABD;

namespace PABD_TokoKomputer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        // Fungsi reusable buat nampilin form ke dalam panelMain
        private void LoadFormInPanel(Form frm)
        {
            panelMain.Controls.Clear();
            frm.TopLevel = false;
            frm.FormBorderStyle = FormBorderStyle.None;
            frm.Dock = DockStyle.Fill;
            panelMain.Controls.Add(frm);
            frm.Show();
        }

        private void btnPelanggan_Click(object sender, EventArgs e)
        {
            LoadFormInPanel(new FormPelanggan());
        }

        private void btnProduk_Click(object sender, EventArgs e)
        {
            LoadFormInPanel(new FormProduk());
        }

        private void btnPemesanan_Click(object sender, EventArgs e)
        {
            LoadFormInPanel(new FormPemesanan());
        }

        private void btnPembayaran_Click(object sender, EventArgs e)
        {
            LoadFormInPanel(new FormPembayaran());
        }
    }
}
