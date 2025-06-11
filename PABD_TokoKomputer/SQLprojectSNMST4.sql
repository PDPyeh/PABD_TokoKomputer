CREATE DATABASE SistemTokoComputerPABD_1;

USE SistemTokoComputerPABD_1;

CREATE TABLE Pelanggan (
    PelangganID INT PRIMARY KEY IDENTITY,
    Nama_Pelanggan VARCHAR(100) NOT NULL,
    Alamat VARCHAR(255) NOT NULL,
    NoTelepon CHAR(15) NOT NULL,
    CONSTRAINT chk_notelp CHECK (NoTelepon LIKE '08%' AND LEN(NoTelepon) <= 12) -- Tambahkan validasi prefix 08
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

ALTER TABLE Pembayaran
ADD CONSTRAINT DF_TanggalPembayaran DEFAULT GETDATE() FOR TanggalPembayaran;



ALTER TABLE Produk
ADD Stok INT;

UPDATE Produk
SET Stok = 50;

SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Produk' AND COLUMN_NAME = 'Stok';

select*from Produk

ALTER TABLE Pemesanan DROP CONSTRAINT FK_PelangganID;
ALTER TABLE Pemesanan DROP CONSTRAINT FK_ProdukID;
ALTER TABLE Pembayaran DROP CONSTRAINT FK_PemesananID;


ALTER TABLE Pemesanan
ADD CONSTRAINT FK_PelangganID
FOREIGN KEY (PelangganID) REFERENCES Pelanggan(PelangganID) ON DELETE CASCADE;

ALTER TABLE Pemesanan
ADD CONSTRAINT FK_ProdukID
FOREIGN KEY (ProdukID) REFERENCES Produk(ProdukID);

ALTER TABLE Pembayaran
ADD CONSTRAINT FK_PemesananID
FOREIGN KEY (PemesananID) REFERENCES Pemesanan(PemesananID) ON DELETE CASCADE;

-- Drop dulu FK lama
ALTER TABLE Pemesanan
DROP CONSTRAINT FK_ProdukID;

-- Buat ulang dengan CASCADE
ALTER TABLE Pemesanan
ADD CONSTRAINT FK_ProdukID
FOREIGN KEY (ProdukID) REFERENCES Produk(ProdukID) ON DELETE CASCADE;

SELECT 
    fk.name AS ForeignKey,
    t.name AS TableName,
    c.name AS ColumnName,
    rt.name AS ReferencedTable
FROM 
    sys.foreign_keys fk
JOIN 
    sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
JOIN 
    sys.tables t ON fkc.parent_object_id = t.object_id
JOIN 
    sys.columns c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
JOIN 
    sys.tables rt ON fkc.referenced_object_id = rt.object_id
WHERE 
    t.name = 'Pembayaran' AND c.name = 'PelangganID';

ALTER TABLE Pemesanan
DROP CONSTRAINT FK_PelangganID;

ALTER TABLE Pemesanan
ADD CONSTRAINT FK_PelangganID
FOREIGN KEY (PelangganID) REFERENCES Pelanggan(PelangganID) ON DELETE CASCADE;

ALTER TABLE Pembayaran
DROP CONSTRAINT FK_PemesananID;

ALTER TABLE Pembayaran
ADD CONSTRAINT FK_PemesananID
FOREIGN KEY (PemesananID) REFERENCES Pemesanan(PemesananID) ON DELETE CASCADE;
-- SP_Pemesanan

CREATE PROCEDURE TambahPemesanan
    @PelangganID INT,
    @ProdukID INT,
    @Jumlah INT,
    @Status_Pesanan VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Stok INT;
        SELECT @Stok = Stok FROM Produk WHERE ProdukID = @ProdukID;

        IF @Jumlah > @Stok
        BEGIN
            THROW 50001, 'Jumlah pemesanan melebihi stok produk.', 1;
        END

        -- Kurangi stok
        UPDATE Produk
        SET Stok = Stok - @Jumlah
        WHERE ProdukID = @ProdukID;

        -- Tambahkan pemesanan
        INSERT INTO Pemesanan (PelangganID, ProdukID, TanggalPemesanan, Status_Pesanan, Jumlah)
        VALUES (@PelangganID, @ProdukID, GETDATE(), @Status_Pesanan, @Jumlah);

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        THROW 50002, @ErrMsg, 1;
    END CATCH
END;

CREATE PROCEDURE UpdatePemesanan
    @PemesananID INT,
    @PelangganID INT,
    @ProdukID INT,
    @Jumlah INT,
    @Status_Pesanan VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Dapatkan jumlah lama & produk lama
        DECLARE @JumlahLama INT, @ProdukIDLama INT;
        SELECT @JumlahLama = Jumlah, @ProdukIDLama = ProdukID FROM Pemesanan WHERE PemesananID = @PemesananID;

        -- Balikin stok lama dulu
        UPDATE Produk SET Stok = Stok + @JumlahLama WHERE ProdukID = @ProdukIDLama;

        -- Cek stok baru
        DECLARE @Stok INT;
        SELECT @Stok = Stok FROM Produk WHERE ProdukID = @ProdukID;

        IF @Jumlah > @Stok
        BEGIN
            THROW 50003, 'Jumlah baru melebihi stok.', 1;
        END

        -- Kurangi stok baru
        UPDATE Produk SET Stok = Stok - @Jumlah WHERE ProdukID = @ProdukID;

        -- Update pemesanan
        UPDATE Pemesanan
        SET PelangganID = @PelangganID,
            ProdukID = @ProdukID,
            TanggalPemesanan = GETDATE(),
            Status_Pesanan = @Status_Pesanan,
            Jumlah = @Jumlah
        WHERE PemesananID = @PemesananID;

        -- Update pembayaran (jika ada)
        DECLARE @Harga DECIMAL(18,2);
        SELECT @Harga = Harga FROM Produk WHERE ProdukID = @ProdukID;

        UPDATE Pembayaran
        SET JumlahPembayaran = @Harga * @Jumlah
        WHERE PemesananID = @PemesananID;

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        DECLARE @Msg NVARCHAR(4000) = ERROR_MESSAGE();
        THROW 50004, @Msg, 1;
    END CATCH
END;

CREATE PROCEDURE HapusPemesanan
    @PemesananID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Ambil data sebelum hapus
        DECLARE @Jumlah INT, @ProdukID INT;
        SELECT @Jumlah = Jumlah, @ProdukID = ProdukID FROM Pemesanan WHERE PemesananID = @PemesananID;

        -- Balikin stok
        UPDATE Produk SET Stok = Stok + @Jumlah WHERE ProdukID = @ProdukID;

        -- Hapus dari pembayaran (jika belum pakai cascade)
        DELETE FROM Pembayaran WHERE PemesananID = @PemesananID;

        -- Hapus pemesanan
        DELETE FROM Pemesanan WHERE PemesananID = @PemesananID;

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        DECLARE @Msg NVARCHAR(4000) = ERROR_MESSAGE();
        THROW 50005, @Msg, 1;
    END CATCH
END;

-- SP_PELANGGN

CREATE PROCEDURE sp_InsertPelanggan
    @Nama_Pelanggan VARCHAR(100),
    @Alamat VARCHAR(255),
    @NoTelepon CHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        INSERT INTO Pelanggan (Nama_Pelanggan, Alamat, NoTelepon)
        VALUES (@Nama_Pelanggan, @Alamat, @NoTelepon);
        
        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        THROW 50001, @ErrMsg, 1;
    END CATCH
END;
GO

CREATE PROCEDURE sp_UpdatePelanggan
    @PelangganID INT,
    @Nama_Pelanggan VARCHAR(100),
    @Alamat VARCHAR(255),
    @NoTelepon CHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        UPDATE Pelanggan 
        SET Nama_Pelanggan = @Nama_Pelanggan,
            Alamat = @Alamat,
            NoTelepon = @NoTelepon
        WHERE PelangganID = @PelangganID;
        
        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        THROW 50002, @ErrMsg, 1;
    END CATCH
END;
GO

CREATE PROCEDURE sp_DeletePelanggan
    @PelangganID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        DELETE FROM Pelanggan WHERE PelangganID = @PelangganID;
        
        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        THROW 50003, @ErrMsg, 1;
    END CATCH
END;
GO
-- SP_Produk

-- Tambah Produk
CREATE PROCEDURE sp_InsertProduk
    @nama NVARCHAR(100),
    @merk NVARCHAR(100),
    @kategori NVARCHAR(100),
    @harga DECIMAL(18,2),
    @stok INT
AS
BEGIN
    INSERT INTO Produk (NamaProduk, NamaMerk, KategoriProduk, Harga, Stok)
    VALUES (@nama, @merk, @kategori, @harga, @stok)
END
GO

-- Update Produk
CREATE PROCEDURE sp_UpdateProduk
    @id INT,
    @nama NVARCHAR(100),
    @merk NVARCHAR(100),
    @kategori NVARCHAR(100),
    @harga DECIMAL(18,2),
    @stok INT
AS
BEGIN
    UPDATE Produk
    SET NamaProduk = @nama, NamaMerk = @merk, KategoriProduk = @kategori, Harga = @harga, Stok = @stok
    WHERE ProdukID = @id
END
GO

-- Hapus Produk
CREATE PROCEDURE sp_DeleteProduk
    @id INT
AS
BEGIN
    DELETE FROM Produk WHERE ProdukID = @id
END
GO


-- SP_Pembayaran

CREATE PROCEDURE sp_InsertPembayaran
    @PemesananID INT,
    @TanggalPembayaran DATE,
    @JumlahPembayaran DECIMAL(18,2),
    @StatusPembayaran VARCHAR(50)
AS
BEGIN
    INSERT INTO Pembayaran (PemesananID, TanggalPembayaran, JumlahPembayaran, StatusPembayaran)
    VALUES (@PemesananID, @TanggalPembayaran, @JumlahPembayaran, @StatusPembayaran)
END

CREATE PROCEDURE sp_UpdatePembayaran
    @PembayaranID INT,
    @PemesananID INT,
    @TanggalPembayaran DATE,
    @JumlahPembayaran DECIMAL(18,2),
    @StatusPembayaran VARCHAR(50)
AS
BEGIN
    UPDATE Pembayaran
    SET PemesananID = @PemesananID,
        TanggalPembayaran = @TanggalPembayaran,
        JumlahPembayaran = @JumlahPembayaran,
        StatusPembayaran = @StatusPembayaran
    WHERE PembayaranID = @PembayaranID
END

CREATE PROCEDURE sp_DeletePembayaran
    @PembayaranID INT
AS
BEGIN
    DELETE FROM Pembayaran WHERE PembayaranID = @PembayaranID
END
