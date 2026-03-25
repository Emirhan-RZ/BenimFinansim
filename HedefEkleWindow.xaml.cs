using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.Sqlite;

namespace BenimFinansim
{
    public partial class HedefEkleWindow : Window
    {
        public HedefEkleWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Kaydet_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBaslik.Text) || string.IsNullOrWhiteSpace(txtToplam.Text))
            {
                MessageBox.Show("Lütfen başlık ve toplam tutar girin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Değerleri okuyoruz
                string tur = cmbTur.SelectedIndex == 0 ? "Hedef" : "Borc";
                string ikon = ((ComboBoxItem)cmbIkon.SelectedItem).Content.ToString() ?? "🎯";
                string baslik = txtBaslik.Text;
                
                double toplam = double.Parse(txtToplam.Text.Replace('.', ','));
                double biriken = 0;
                
                // Eğer biriken alanına bir şey yazıldıysa onu da al
                if (!string.IsNullOrWhiteSpace(txtBiriken.Text))
                {
                    biriken = double.Parse(txtBiriken.Text.Replace('.', ','));
                }

                // Veritabanına kaydet (YENİ VE GÜVENLİ BAĞLANTI)
                using (var baglanti = new SqliteConnection(DatabaseManager.BaglantiCumlesi))
                {
                    baglanti.Open();
                    var komut = baglanti.CreateCommand();
                    komut.CommandText = "INSERT INTO Hedefler (Baslik, Tur, ToplamTutar, BirikenTutar, Ikon) VALUES (@baslik, @tur, @toplam, @biriken, @ikon)";
                    komut.Parameters.AddWithValue("@baslik", baslik);
                    komut.Parameters.AddWithValue("@tur", tur);
                    komut.Parameters.AddWithValue("@toplam", toplam);
                    komut.Parameters.AddWithValue("@biriken", biriken);
                    komut.Parameters.AddWithValue("@ikon", ikon);
                    komut.ExecuteNonQuery();
                }
                
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hedef eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}