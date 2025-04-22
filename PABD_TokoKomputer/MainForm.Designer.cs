namespace PABD_TokoKomputer
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnPelanggan = new System.Windows.Forms.Button();
            this.btnProduk = new System.Windows.Forms.Button();
            this.btnPemesanan = new System.Windows.Forms.Button();
            this.btnPembayaran = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.panelMain = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // btnPelanggan
            // 
            this.btnPelanggan.Location = new System.Drawing.Point(3, 45);
            this.btnPelanggan.Name = "btnPelanggan";
            this.btnPelanggan.Size = new System.Drawing.Size(146, 47);
            this.btnPelanggan.TabIndex = 0;
            this.btnPelanggan.Text = "Pelanggan";
            this.btnPelanggan.UseVisualStyleBackColor = true;
            this.btnPelanggan.Click += new System.EventHandler(this.btnPelanggan_Click);
            // 
            // btnProduk
            // 
            this.btnProduk.Location = new System.Drawing.Point(3, 98);
            this.btnProduk.Name = "btnProduk";
            this.btnProduk.Size = new System.Drawing.Size(146, 47);
            this.btnProduk.TabIndex = 1;
            this.btnProduk.Text = "Produk";
            this.btnProduk.UseVisualStyleBackColor = true;
            this.btnProduk.Click += new System.EventHandler(this.btnProduk_Click);
            // 
            // btnPemesanan
            // 
            this.btnPemesanan.Location = new System.Drawing.Point(3, 151);
            this.btnPemesanan.Name = "btnPemesanan";
            this.btnPemesanan.Size = new System.Drawing.Size(146, 47);
            this.btnPemesanan.TabIndex = 2;
            this.btnPemesanan.Text = "Pemesanan";
            this.btnPemesanan.UseVisualStyleBackColor = true;
            this.btnPemesanan.Click += new System.EventHandler(this.btnPemesanan_Click);
            // 
            // btnPembayaran
            // 
            this.btnPembayaran.Location = new System.Drawing.Point(3, 204);
            this.btnPembayaran.Name = "btnPembayaran";
            this.btnPembayaran.Size = new System.Drawing.Size(146, 47);
            this.btnPembayaran.TabIndex = 3;
            this.btnPembayaran.Text = "Pembayaran";
            this.btnPembayaran.UseVisualStyleBackColor = true;
            this.btnPembayaran.Click += new System.EventHandler(this.btnPembayaran_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("HelveticaNowText Bold", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(-5, -1);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(166, 43);
            this.label1.TabIndex = 4;
            this.label1.Text = "Main Menu";
            // 
            // panelMain
            // 
            this.panelMain.Location = new System.Drawing.Point(165, 23);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(858, 540);
            this.panelMain.TabIndex = 5;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1036, 574);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnPembayaran);
            this.Controls.Add(this.btnPemesanan);
            this.Controls.Add(this.btnProduk);
            this.Controls.Add(this.btnPelanggan);
            this.Name = "MainForm";
            this.Text = "Form2";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPelanggan;
        private System.Windows.Forms.Button btnProduk;
        private System.Windows.Forms.Button btnPemesanan;
        private System.Windows.Forms.Button btnPembayaran;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelMain;
    }
}