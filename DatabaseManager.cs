using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace BenimFinansim
{
    public class DatabaseManager
    {
        // Veritabanının yolunu dinamik olarak bilgisayardaki güvenli AppData klasörüne kuruyoruz
        private static string GetDbPath()
        {
            // C:\Users\Kullanici\AppData\Roaming klasörünü bulur
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            
            // Orada "BenimFinansim" adında bir klasör yolu oluşturur
            string folderPath = Path.Combine(appData, "BenimFinansim");

            // Eğer bu klasör bilgisayarda yoksa (ilk kez açılıyorsa) oluşturur
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Tam dosya yolunu döner: ...\AppData\Roaming\BenimFinansim\finansim.db
            return Path.Combine(folderPath, "finansim.db");
        }

        // Diğer sınıflardan bağlantı açarken kullanacağımız ortak bağlantı cümlesi
        public static string BaglantiCumlesi => $"Data Source={GetDbPath()};";

        // Uygulama ilk açıldığında tabloları kuracak olan metod
        public static void VeritabaniniKur()
        {
            // Bağlantıyı yeni güvenli AppData yoluna göre açıyoruz
            using (var baglanti = new SqliteConnection(BaglantiCumlesi))
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