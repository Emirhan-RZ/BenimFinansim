using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.Sqlite;

namespace BenimFinansim
{
    public partial class VarlikEkleWindow : Window
    {
        public VarlikEkleWindow()
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
            // Basit güvenlik kontrolü
            if (cmbTur.SelectedItem == null || string.IsNullOrWhiteSpace(txtMiktar.Text))
            {
                MessageBox.Show("Lütfen tür ve miktar girin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Tag değerini alıyoruz (USD, EUR, GLD, SLV)
                string turTag = ((ComboBoxItem)cmbTur.SelectedItem).Tag.ToString() ?? "USD";
                
                // Nokta/Virgül fark etmeksizin matematiğe çeviriyoruz
                double miktar = double.Parse(txtMiktar.Text.Replace('.', ','));
                
                // YENİ: Alış fiyatını oku (Eğer boş bırakılırsa 0 kabul et)
                double alisFiyati = 0;
                if (!string.IsNullOrWhiteSpace(txtAlisFiyati.Text))
                {
                    alisFiyati = double.Parse(txtAlisFiyati.Text.Replace('.', ','));
                }

                // Satış işlemi yapılıyorsa miktarı eksi (-) yapıyoruz ki kasadan düşsün
                if (cmbIslemTipi.SelectedIndex == 1) // 1 = Sat (Çıkar)
                {
                    miktar = -miktar;
                }

                using (var baglanti = new SqliteConnection("Data Source=finansim.db"))
                {
                    baglanti.Open();
                    var komut = baglanti.CreateCommand();
                    
                    // SQL Sorgusuna "AlisFiyati" parametresi eklendi
                    komut.CommandText = "INSERT INTO Varliklar (Tur, Miktar, AlisFiyati) VALUES (@tur, @miktar, @alis)";
                    komut.Parameters.AddWithValue("@tur", turTag);
                    komut.Parameters.AddWithValue("@miktar", miktar);
                    komut.Parameters.AddWithValue("@alis", alisFiyati);
                    komut.ExecuteNonQuery();
                }
                
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Varlık eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}