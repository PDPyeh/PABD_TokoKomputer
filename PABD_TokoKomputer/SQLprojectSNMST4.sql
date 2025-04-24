CREATE DATABASE SistemTokoComputerPABD_1;

USE SistemTokoComputerPABD_1;

CREATE TABLE Pelanggan (
    PelangganID INT PRIMARY KEY IDENTITY,
    Nama_Pelanggan VARCHAR(100) NOT NULL,
    Alamat VARCHAR(255) NOT NULL,
    NoTelepon CHAR(15) NOT NULL,
    CONSTRAINT chk_notelp CHECK (NoTelepon LIKE '08%' AND LEN(NoTelepon) <= 12) -- Tambahkan validasi "08"
);

CREATE TABLE Produk (
    ProdukID INT PRIMARY KEY IDENTITY,
    NamaProduk VARCHAR(100) NOT NULL,
    NamaMerk VARCHAR(100) NOT NULL, -- Menambahkan NamaMerk
    KategoriProduk VARCHAR(100) NOT NULL,
    Harga DECIMAL(18, 2) NOT NULL
);

CREATE TABLE Pemesanan (
    PemesananID INT PRIMARY KEY IDENTITY,
    PelangganID INT NOT NULL,
    ProdukID INT NOT NULL,
    TanggalPemesanan DATETIME NOT NULL, -- Diubah menjadi DATETIME
    Status_Pesanan VARCHAR(20) NOT NULL,
    Jumlah INT NOT NULL,
    CONSTRAINT FK_ProdukID FOREIGN KEY (ProdukID) REFERENCES Produk(ProdukID),
    CONSTRAINT FK_PelangganID FOREIGN KEY (PelangganID) REFERENCES Pelanggan(PelangganID),
    CONSTRAINT chk_status_pesanan CHECK (Status_Pesanan IN ('Diproses', 'Dikirim')) -- Tambahkan constraint
);

CREATE TABLE Pembayaran (
    PembayaranID INT PRIMARY KEY IDENTITY,
    PemesananID INT NOT NULL,
    TanggalPembayaran DATETIME NOT NULL, -- Diubah menjadi DATETIME
    JumlahPembayaran DECIMAL(18, 2) NOT NULL,
    StatusPembayaran VARCHAR(20) NOT NULL,
    CONSTRAINT FK_PemesananID FOREIGN KEY (PemesananID) REFERENCES Pemesanan(PemesananID),
    CONSTRAINT chk_status_pembayaran CHECK (StatusPembayaran IN ('Sukses', 'Gagal')) -- Tambahkan constraint
);

CREATE TABLE Users (
    Username VARCHAR(50) PRIMARY KEY,
    Password VARCHAR(50) NOT NULL
);

INSERT INTO Users VALUES ('admin', '1234');

select*from Produk;
select*from Pelanggan;



