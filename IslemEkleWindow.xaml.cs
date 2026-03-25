using System;
using System.Windows;
using Microsoft.Data.Sqlite;

namespace BenimFinansim
{
    public partial class IslemEkleWindow : Window
    {
        public string IslemTipi { get; set; } // "Gelir" veya "Gider"

        public IslemEkleWindow(string tip)
        {
            InitializeComponent();
            IslemTipi = tip;
            this.Title = tip + " Ekle";

            KategorileriYukle();
        }

        private void Kaydet_Click(object sender, RoutedEventArgs e)
        {
            // 1. BOŞLUK KONTROLÜ
            if (string.IsNullOrWhiteSpace(txtBaslik.Text) || string.IsNullOrWhiteSpace(txtMiktar.Text))
            {
                MessageBox.Show("Lütfen tüm alanları doldurun!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. SAYI KONTROLÜ (Programın çökmesini engeller)
            if (!double.TryParse(txtMiktar.Text.Replace('.', ','), out double miktar))
            {
                MessageBox.Show("Lütfen miktar kısmına geçerli bir sayı girin!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try 
            {
                // YENİ VE GÜVENLİ BAĞLANTI
                using (var baglanti = new SqliteConnection(DatabaseManager.BaglantiCumlesi))
                {
                    baglanti.Open();
                    var komut = baglanti.CreateCommand();
                    komut.CommandText = "INSERT INTO Islemler (Baslik, Miktar, Tip, Hesap, Kategori, Tarih) VALUES (@baslik, @miktar, @tip, 'Cüzdan', @kat, @tarih)";
                    
                    komut.Parameters.AddWithValue("@baslik", txtBaslik.Text.Trim());
                    komut.Parameters.AddWithValue("@miktar", miktar);
                    komut.Parameters.AddWithValue("@tip", IslemTipi);
                    
                    // Kategori seçili değilse "Diğer" yazsın
                    string seciliKategori = cmbKategori.SelectedItem?.ToString() ?? "Diğer";
                    komut.Parameters.AddWithValue("@kat", seciliKategori);
                    
                    komut.Parameters.AddWithValue("@tarih", DateTime.Now.ToString("yyyy-MM-dd"));

                    komut.ExecuteNonQuery();
                }
                
                // Başarılı mesajı vermeye gerek yok, direkt kapansın (UX için daha iyi)
                this.Close(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı hatası: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void KategorileriYukle()
        {
            try
            {
                cmbKategori.Items.Clear();

                // YENİ VE GÜVENLİ BAĞLANTI
                using (var baglanti = new SqliteConnection(DatabaseManager.BaglantiCumlesi))
                {
                    baglanti.Open();
                    var komut = baglanti.CreateCommand();
                    komut.CommandText = "SELECT Ad FROM Kategoriler ORDER BY Ad ASC";
                    
                    using (var reader = komut.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbKategori.Items.Add(reader.GetString(0));
                        }
                    }
                }
                
                if (cmbKategori.Items.Count > 0)
                    cmbKategori.SelectedIndex = 0;
                else
                    cmbKategori.Items.Add("Genel"); // Eğer hiç kategori yoksa boş kalmasın
            }
            catch (Exception ex)
            {
                Console.WriteLine("Kategoriler yüklenemedi: " + ex.Message);
            }
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}