using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace BenimFinansim
{
    public class DatabaseManager
    {
        // 1. Veritabanının yolunu dinamik olarak bilgisayardaki güvenli AppData klasörüne kuruyoruz
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

        // 2. Diğer sınıflardan bağlantı açarken kullanacağımız ortak bağlantı cümlesi
        public static string BaglantiCumlesi => $"Data Source={GetDbPath()};";

        // 3. Uygulama ilk açıldığında tabloları kuracak olan metod
        public static void VeritabaniniKur()
{
    // Yol zaten GetDbPath içinde klasör oluşturuyor, burası tamam.
    string dbPath = GetDbPath(); 

    using (var baglanti = new SqliteConnection(BaglantiCumlesi))
    {
                baglanti.Open();
                var komut = baglanti.CreateCommand();

                // --- TABLO OLUŞTURMA İŞLEMLERİ ---

                // A. İŞLEMLER TABLOSU (Gelir ve Giderler)
                komut.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Islemler (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Baslik TEXT NOT NULL,
                        Miktar REAL NOT NULL,
                        Tip TEXT NOT NULL,
                        Hesap TEXT NOT NULL,
                        Kategori TEXT,
                        Tarih TEXT NOT NULL
                    )";
                komut.ExecuteNonQuery();

                // B. VARLIKLAR TABLOSU (Döviz ve Altın kasası)
                komut.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Varliklar (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Tur TEXT NOT NULL,
                        Miktar REAL NOT NULL
                    )";
                komut.ExecuteNonQuery();

                // C. PLANLANMIŞ ÖDEMELER TABLOSU
                komut.CommandText = @"
                    CREATE TABLE IF NOT EXISTS PlanlanmisOdemeler (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Baslik TEXT,
                        Miktar REAL,
                        OdemeGunu INTEGER,
                        Kategori TEXT
                    )";
                komut.ExecuteNonQuery();

                // D. BÜTÇELER TABLOSU
                komut.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Butceler (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                        Kategori TEXT, 
                        AylikLimit REAL
                    )";
                komut.ExecuteNonQuery();

                // E. HEDEFLER TABLOSU
                komut.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Hedefler (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                        Baslik TEXT, 
                        Tur TEXT, 
                        ToplamTutar REAL, 
                        BirikenTutar REAL, 
                        Ikon TEXT
                    )";
                komut.ExecuteNonQuery();

                // --- SÜTUN GÜNCELLEMELERİ (ALTER TABLE) ---

                // VARLIKLAR TABLOSUNA "AlisFiyati" SÜTUNUNU EKLE
                try
                {
                    // Sütun yoksa ekler, varsa hata verir (catch bloğu hatayı yutar)
                    komut.CommandText = "ALTER TABLE Varliklar ADD COLUMN AlisFiyati REAL DEFAULT 0";
                    komut.ExecuteNonQuery();
                }
                catch 
                { 
                    // Sütun zaten mevcutsa buraya düşer, programın çalışmasına engel olmaz.
                }

            } // Bağlantı burada otomatik olarak kapanır (using bloğu sayesinde)
        }
    }
}