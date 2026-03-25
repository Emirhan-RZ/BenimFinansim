using System;
using System.Windows;
using Microsoft.Data.Sqlite;

namespace BenimFinansim
{
    public partial class OdemePlanlaWindow : Window
    {
        public OdemePlanlaWindow() { InitializeComponent(); }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove(); }
        private void BtnClose_Click(object sender, RoutedEventArgs e) { this.Close(); }

        private void Kaydet_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtBaslik.Text) || string.IsNullOrEmpty(txtMiktar.Text) || string.IsNullOrEmpty(txtGun.Text)) return;

            try 
            {
                double miktar = double.Parse(txtMiktar.Text.Replace('.', ','));
                int gun = int.Parse(txtGun.Text); // Ayın kaçıncı günü

                // YENİ VE GÜVENLİ BAĞLANTI
                using (var baglanti = new SqliteConnection(DatabaseManager.BaglantiCumlesi))
                {
                    baglanti.Open();
                    var komut = baglanti.CreateCommand();
                    komut.CommandText = "INSERT INTO PlanlanmisOdemeler (Baslik, Miktar, OdemeGunu, Kategori) VALUES (@baslik, @miktar, @gun, 'Gider')";
                    komut.Parameters.AddWithValue("@baslik", txtBaslik.Text);
                    komut.Parameters.AddWithValue("@miktar", miktar);
                    komut.Parameters.AddWithValue("@gun", gun);
                    komut.ExecuteNonQuery();
                }
                this.Close();
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }
    }
}