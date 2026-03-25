using Microsoft.Data.Sqlite;
using System.IO;

namespace BenimFinansim
{
    public class DatabaseManager
    {
        // Veritabanı dosyamızın adı (Proje klasöründe otomatik oluşacak)
        private static string dbName = "finansim.db";

        // Uygulama ilk açıldığında tabloları kuracak olan metod
        public static void VeritabaniniKur()
        {
            // Bağlantıyı açıyoruz (Dosya yoksa kendisi sıfırdan oluşturur)
            using (var baglanti = new SqliteConnection($"Data Source={dbName}"))
            {
                baglanti.Open();

                // 1. İŞLEMLER TABLOSU (Gelir ve Giderleri tutacağımız tablo)
                var komut = baglanti.CreateCommand();
                komut.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Islemler (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Baslik TEXT NOT NULL,         -- Örn: Market, Maaş, Eğlence
                        Miktar REAL NOT NULL,         -- Örn: 150.50, 5000
                        Tip TEXT NOT NULL,            -- Örn: 'Gelir' veya 'Gider'
                        Hesap TEXT NOT NULL,          -- Örn: 'Cüzdan' veya 'Banka'
                        Kategori TEXT,                -- Örn: 'Gıda', 'Ev'
                        Tarih TEXT NOT NULL           -- Örn: '2026-03-22'
                    )";
                
                // Komutu çalıştırıp tabloyu inşa ediyoruz
                komut.ExecuteNonQuery();

                // 2. VARLIKLAR TABLOSU (Döviz ve Altın tutacağımız kasa)
                komut.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Varliklar (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Tur TEXT NOT NULL,         -- Örn: 'USD', 'EUR', 'GLD'
                        Miktar REAL NOT NULL       -- Örn: 150.50, 10
                    )";
                komut.ExecuteNonQuery();

                // 3. PLANLANMIŞ ÖDEMELER TABLOSU (Her ayın kaçıncı günü ödenecek vb.)
                komut.CommandText = @"CREATE TABLE IF NOT EXISTS PlanlanmisOdemeler (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Baslik TEXT,
                    Miktar REAL,
                    OdemeGunu INTEGER,
                    Kategori TEXT
                )";
                komut.ExecuteNonQuery();

                // 4. YENİ: BÜTÇELER TABLOSU
                komut.CommandText = @"CREATE TABLE IF NOT EXISTS Butceler (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT, Kategori TEXT, AylikLimit REAL)";
                komut.ExecuteNonQuery();

                // 5. YENİ: HEDEFLER VE BORÇLAR TABLOSU
                komut.CommandText = @"CREATE TABLE IF NOT EXISTS Hedefler (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT, Baslik TEXT, Tur TEXT, ToplamTutar REAL, BirikenTutar REAL, Ikon TEXT)";
                komut.ExecuteNonQuery();

                // VARLIKLAR TABLOSUNA "ALIŞ FİYATI" SÜTUNUNU ENJEKTE ET (Bağlantı kapanmadan önce burada olmalı!)
                try
                {
                    komut.CommandText = "ALTER TABLE Varliklar ADD COLUMN AlisFiyati REAL DEFAULT 0";
                    komut.ExecuteNonQuery();
                }
                catch 
                { 
                    // Eğer sütun zaten daha önceden eklendiyse sistem çökmesin, sessizce geçsin diye try-catch içine aldık.
                }

            } // <--- Bağlantıyı kapatan parantez BURADA olmalı!
        }
    }
}