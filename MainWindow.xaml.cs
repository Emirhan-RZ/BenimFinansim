using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Microsoft.Data.Sqlite;
using ClosedXML.Excel;
using System.IO;

namespace BenimFinansim
{
    public class IslemModel
    {
        public int Id { get; set; }
        public string Baslik { get; set; } = string.Empty;
        public string Kategori { get; set; } = string.Empty;
        public string Tarih { get; set; } = string.Empty;
        public string MiktarMetni { get; set; } = string.Empty;
        public SolidColorBrush RenkBrush { get; set; } = null!;
    }

    public class KategoriRaporModel
    {
        public string KategoriAdi { get; set; } = string.Empty;
        public double Tutar { get; set; }
        public string TutarMetni { get; set; } = string.Empty;
        public SolidColorBrush RenkBrush { get; set; } = null!;
    }

    public class VarlikPortfoyModel
    {
        public string Ikon { get; set; } = string.Empty;
        public string VarlikAdi { get; set; } = string.Empty;
        public string MiktarMetni { get; set; } = string.Empty;
        public string MaliyetMetni { get; set; } = string.Empty;
        public string GuncelFiyatMetni { get; set; } = string.Empty;
        public string ToplamDegerMetni { get; set; } = string.Empty;
        public string KarZararMetni { get; set; } = string.Empty;
        public SolidColorBrush KarZararRenk { get; set; } = null!;
        public string Tur { get; set; } = string.Empty;
    }

    public class ButceModel
    {
        public string Kategori { get; set; } = string.Empty;
        public string LimitMetni { get; set; } = string.Empty;
        public double Yuzde { get; set; }
        public SolidColorBrush RenkBrush { get; set; } = null!;
        public SolidColorBrush YaziRenkBrush { get; set; } = null!;
    }

    public class HedefModel
    {
        public int Id { get; set; }
        public string Baslik { get; set; } = string.Empty;
        public string Ikon { get; set; } = string.Empty;
        public string OzetMetni { get; set; } = string.Empty;
        public string YuzdeMetni { get; set; } = string.Empty;
        public double Yuzde { get; set; }
        public SolidColorBrush RenkBrush { get; set; } = null!;
        public SolidColorBrush ArkaPlanBrush { get; set; } = null!;
        public SolidColorBrush YaziRenkBrush { get; set; } = null!;
    }

    public class KategoriModel
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
    }

    public partial class MainWindow : Window, System.ComponentModel.INotifyPropertyChanged
    {
        public ISeries[] BuAySerisi { get; set; } = null!;
        public ISeries[] GecenAySerisi { get; set; } = null!;
        public ISeries[] BakiyeSerisi { get; set; } = null!;
        public Axis[] BakiyeEkseni { get; set; } = null!;
        public ISeries[] Son7GunSerileri { get; set; } = null!;
        public Axis[] Son7GunEkseni { get; set; } = null!;
        public System.Collections.ObjectModel.ObservableCollection<ISeries> RaporSerisi { get; set; } = new System.Collections.ObjectModel.ObservableCollection<ISeries>();

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void YenileBildir(string name)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }

        public MainWindow()
        {
            InitializeComponent();
            DatabaseManager.VeritabaniniKur();
            VerileriGuncelle();
        }

        private double DbDenDegerGetir(SqliteConnection baglanti, string sqlSorgusu)
        {
            var komut = baglanti.CreateCommand();
            komut.CommandText = sqlSorgusu;
            var sonuc = komut.ExecuteScalar();
            return (sonuc != DBNull.Value && sonuc != null) ? Convert.ToDouble(sonuc) : 0;
        }

        private async void VerileriGuncelle()
        {
            try
            {
                using (var baglanti = new SqliteConnection(DatabaseManager.BaglantiCumlesi))
                {
                    baglanti.Open();

                    double toplamGelir = DbDenDegerGetir(baglanti, "SELECT SUM(Miktar) FROM Islemler WHERE Tip='Gelir'");
                    double toplamGider = DbDenDegerGetir(baglanti, "SELECT SUM(Miktar) FROM Islemler WHERE Tip='Gider'");
                    double tlBakiye = toplamGelir - toplamGider;

                    if (txtAnaBakiye != null) txtAnaBakiye.Text = $"₺{tlBakiye:N2}";
                    if (txtCuzdan != null) txtCuzdan.Text = $"₺{tlBakiye:N2}";

                    double usdKuru = await KurServisi.KuruGetir("USD");
                    double eurKuru = await KurServisi.KuruGetir("EUR");
                    double altinKuru = await KurServisi.MadenGetir("gram-altin");
                    double gumusKuru = await KurServisi.MadenGetir("gumus");

                    double usdMiktar = DbDenDegerGetir(baglanti, "SELECT SUM(Miktar) FROM Varliklar WHERE Tur='USD'");
                    double eurMiktar = DbDenDegerGetir(baglanti, "SELECT SUM(Miktar) FROM Varliklar WHERE Tur='EUR'");
                    double gldMiktar = DbDenDegerGetir(baglanti, "SELECT SUM(Miktar) FROM Varliklar WHERE Tur='GLD'");
                    double slvMiktar = DbDenDegerGetir(baglanti, "SELECT SUM(Miktar) FROM Varliklar WHERE Tur='SLV'");

                    if (txtDolarKur != null) txtDolarKur.Text = $"(₺{usdKuru:N2})";
                    if (txtEuroKur != null) txtEuroKur.Text = $"(₺{eurKuru:N2})";
                    if (txtAltinKur != null) txtAltinKur.Text = $"(₺{altinKuru:N2})";
                    if (txtGumusKur != null) txtGumusKur.Text = $"(₺{gumusKuru:N2})";

                    if (txtDolarVarlik != null) txtDolarVarlik.Text = $"${usdMiktar:N2}";
                    if (txtEuroVarlik != null) txtEuroVarlik.Text = $"€{eurMiktar:N2}";
                    if (txtAltinVarlik != null) txtAltinVarlik.Text = $"{gldMiktar:N2}g";
                    if (txtGumusVarlik != null) txtGumusVarlik.Text = $"{slvMiktar:N2}g";

                    double toplamNetVarlik = tlBakiye + (usdMiktar * usdKuru) + (eurMiktar * eurKuru) + (gldMiktar * altinKuru) + (slvMiktar * gumusKuru);
                    if (txtToplamVarlik != null) txtToplamVarlik.Text = $"₺{toplamNetVarlik:N2}";

                    if (txtNakitGelir != null) txtNakitGelir.Text = $"₺{toplamGelir:N2}";
                    if (txtNakitGider != null) txtNakitGider.Text = $"-₺{toplamGider:N2}";
                    if (txtNakitNet != null) txtNakitNet.Text = $"₺{(toplamGelir - toplamGider):N2}";

                    string buAyStr = DateTime.Now.ToString("yyyy-MM");
                    string gecenAyStr = DateTime.Now.AddMonths(-1).ToString("yyyy-MM");

                    double buAyGelir = DbDenDegerGetir(baglanti, $"SELECT SUM(Miktar) FROM Islemler WHERE Tip='Gelir' AND strftime('%Y-%m', Tarih) = '{buAyStr}'");
                    double buAyGider = DbDenDegerGetir(baglanti, $"SELECT SUM(Miktar) FROM Islemler WHERE Tip='Gider' AND strftime('%Y-%m', Tarih) = '{buAyStr}'");
                    double gecenAyGelir = DbDenDegerGetir(baglanti, $"SELECT SUM(Miktar) FROM Islemler WHERE Tip='Gelir' AND strftime('%Y-%m', Tarih) = '{gecenAyStr}'");
                    double gecenAyGider = DbDenDegerGetir(baglanti, $"SELECT SUM(Miktar) FROM Islemler WHERE Tip='Gider' AND strftime('%Y-%m', Tarih) = '{gecenAyStr}'");

                    if (txtBuAyGelir != null) txtBuAyGelir.Text = $"₺{buAyGelir:N2}";
                    if (txtBuAyGider != null) txtBuAyGider.Text = $"-₺{buAyGider:N2}";
                    if (txtGecenAyGelir != null) txtGecenAyGelir.Text = $"₺{gecenAyGelir:N2}";
                    if (txtGecenAyGider != null) txtGecenAyGider.Text = $"-₺{gecenAyGider:N2}";

                    double buAyHarcamaYuzdesi = (buAyGelir > 0) ? (buAyGider / buAyGelir) * 100 : ((buAyGider > 0) ? 100 : 0);
                    double gecenAyHarcamaYuzdesi = (gecenAyGelir > 0) ? (gecenAyGider / gecenAyGelir) * 100 : ((gecenAyGider > 0) ? 100 : 0);

                    if (txtBuAyYuzde != null) txtBuAyYuzde.Text = $"%{Math.Round(buAyHarcamaYuzdesi)}";
                    if (txtGecenAyYuzde != null) txtGecenAyYuzde.Text = $"%{Math.Round(gecenAyHarcamaYuzdesi)}";

                    BuAySerisi = (buAyGelir == 0 && buAyGider == 0)
                        ? new ISeries[] { new PieSeries<double> { Values = new double[] { 1 }, Fill = new SolidColorPaint(SKColor.Parse("#E0E0E0")), InnerRadius = 25 } }
                        : new ISeries[] {
                            new PieSeries<double> { Values = new double[] { buAyGelir }, Fill = new SolidColorPaint(SKColor.Parse("#4CAF50")), InnerRadius=25 },
                            new PieSeries<double> { Values = new double[] { buAyGider }, Fill = new SolidColorPaint(SKColor.Parse("#F44336")), InnerRadius=25 }
                        };

                    GecenAySerisi = (gecenAyGelir == 0 && gecenAyGider == 0)
                        ? new ISeries[] { new PieSeries<double> { Values = new double[] { 1 }, Fill = new SolidColorPaint(SKColor.Parse("#E0E0E0")), InnerRadius = 25 } }
                        : new ISeries[] {
                            new PieSeries<double> { Values = new double[] { gecenAyGelir }, Fill = new SolidColorPaint(SKColor.Parse("#4CAF50")), InnerRadius=25 },
                            new PieSeries<double> { Values = new double[] { gecenAyGider }, Fill = new SolidColorPaint(SKColor.Parse("#F44336")), InnerRadius=25 }
                        };

                    string[] gunIsimleri = new string[7];
                    double[] yediGunGelir = new double[7];
                    double[] yediGunGider = new double[7];
                    double[] yediGunBakiye = new double[7];
                    double geciciBakiye = tlBakiye;

                    for (int i = 6; i >= 0; i--)
                    {
                        DateTime gun = DateTime.Now.AddDays(-i);
                        string formatliTarih = gun.ToString("yyyy-MM-dd");
                        gunIsimleri[6 - i] = gun.ToString("dd MMM");
                        yediGunGelir[6 - i] = DbDenDegerGetir(baglanti, $"SELECT SUM(Miktar) FROM Islemler WHERE Tip='Gelir' AND Tarih='{formatliTarih}'");
                        yediGunGider[6 - i] = DbDenDegerGetir(baglanti, $"SELECT SUM(Miktar) FROM Islemler WHERE Tip='Gider' AND Tarih='{formatliTarih}'");
                    }

                    for (int i = 6; i >= 0; i--)
                    {
                        yediGunBakiye[i] = geciciBakiye;
                        geciciBakiye = geciciBakiye - yediGunGelir[i] + yediGunGider[i];
                    }

                    Son7GunSerileri = new ISeries[] {
                        new ColumnSeries<double> { Name = "Gider", Values = yediGunGider, Fill = new SolidColorPaint(SKColor.Parse("#F44336")), MaxBarWidth=25, Padding=2 },
                        new ColumnSeries<double> { Name = "Gelir", Values = yediGunGelir, Fill = new SolidColorPaint(SKColor.Parse("#4CAF50")), MaxBarWidth=25, Padding=2 }
                    };
                    Son7GunEkseni = new Axis[] { new Axis { Labels = gunIsimleri, LabelsPaint = new SolidColorPaint(SKColor.Parse("#666666")), TextSize = 12 } };

                    BakiyeSerisi = new ISeries[]
                    {
                        new LineSeries<double>
                        {
                            Values = yediGunBakiye,
                            Name = "Toplam Bakiye",
                            Fill = null,
                            GeometrySize = 10,
                            Stroke = new SolidColorPaint(SKColors.DeepSkyBlue) { StrokeThickness = 4 }
                        }
                    };

                    var islemListesi = new List<IslemModel>();
                    var listKomut = baglanti.CreateCommand();
                    listKomut.CommandText = "SELECT Id, Baslik, Kategori, Miktar, Tip, Tarih FROM Islemler ORDER BY Tarih DESC, Id DESC LIMIT 4";

                    using (var okuyucu = listKomut.ExecuteReader())
                    {
                        while (okuyucu.Read())
                        {
                            bool isGelir = okuyucu.GetString(4) == "Gelir";
                            islemListesi.Add(new IslemModel
                            {
                                Id = okuyucu.GetInt32(0),
                                Baslik = okuyucu.GetString(1),
                                Kategori = okuyucu.GetString(2),
                                Tarih = Convert.ToDateTime(okuyucu.GetString(5)).ToString("dd.MM.yyyy"),
                                MiktarMetni = isGelir ? $"+₺{okuyucu.GetDouble(3):N2}" : $"-₺{okuyucu.GetDouble(3):N2}",
                                RenkBrush = isGelir ? new SolidColorBrush(Color.FromRgb(76, 175, 80)) : new SolidColorBrush(Color.FromRgb(244, 67, 54))
                            });
                        }
                    }
                    if (lstSonIslemler != null) lstSonIslemler.ItemsSource = islemListesi;
                    if (txtIslemYok != null) txtIslemYok.Visibility = islemListesi.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                }

                DataContext = null;
                DataContext = this;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Güncelleme hatası: " + ex.Message);
            }
        }

        public void YatirimEkle_Click(object sender, RoutedEventArgs e)
        {
            VarlikEkleWindow pencere = new VarlikEkleWindow();
            pencere.Owner = this;
            pencere.ShowDialog();
            VerileriGuncelle();
        }

        public void GelirEkle_Click(object sender, RoutedEventArgs e)
        {
            IslemEkleWindow pencere = new IslemEkleWindow("Gelir");
            pencere.Owner = this;
            pencere.ShowDialog();
            VerileriGuncelle();
        }

        public void GiderEkle_Click(object sender, RoutedEventArgs e)
        {
            IslemEkleWindow pencere = new IslemEkleWindow("Gider");
            pencere.Owner = this;
            pencere.ShowDialog();
            VerileriGuncelle();
        }

        private void MenuButonlariniSifirla()
        {
            if (btnGenelBakis == null || btnIslemler == null || btnPlanlanmis == null || btnRaporlar == null || btnPortfoy == null || btnButce == null || btnAyarlar == null) return;

            var pasifArkaPlan = new SolidColorBrush(Colors.Transparent);
            var pasifYaziRengi = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));

            btnGenelBakis.Background = pasifArkaPlan; btnGenelBakis.Foreground = pasifYaziRengi; btnGenelBakis.FontWeight = FontWeights.Normal;
            btnIslemler.Background = pasifArkaPlan; btnIslemler.Foreground = pasifYaziRengi; btnIslemler.FontWeight = FontWeights.Normal;
            btnPlanlanmis.Background = pasifArkaPlan; btnPlanlanmis.Foreground = pasifYaziRengi; btnPlanlanmis.FontWeight = FontWeights.Normal;
            btnRaporlar.Background = pasifArkaPlan; btnRaporlar.Foreground = pasifYaziRengi; btnRaporlar.FontWeight = FontWeights.Normal;
            btnPortfoy.Background = pasifArkaPlan; btnPortfoy.Foreground = pasifYaziRengi; btnPortfoy.FontWeight = FontWeights.Normal;
            btnButce.Background = pasifArkaPlan; btnButce.Foreground = pasifYaziRengi; btnButce.FontWeight = FontWeights.Normal;
            btnAyarlar.Background = pasifArkaPlan; btnAyarlar.Foreground = pasifYaziRengi; btnAyarlar.FontWeight = FontWeights.Normal;
        }

        private void MenuGenelBakis_Click(object sender, RoutedEventArgs e)
        {
            if (pnlGenelBakis == null) return;
            pnlIslemler.Visibility = Visibility.Collapsed;
            pnlPlanlanmis.Visibility = Visibility.Collapsed;
            pnlRaporlar.Visibility = Visibility.Collapsed;
            pnlPortfoy.Visibility = Visibility.Collapsed;
            if (pnlButce != null) pnlButce.Visibility = Visibility.Collapsed;
            if (pnlAyarlar != null) pnlAyarlar.Visibility = Visibility.Collapsed;

            pnlGenelBakis.Visibility = Visibility.Visible;

            VerileriGuncelle();

            MenuButonlariniSifirla();
            btnGenelBakis.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
            btnGenelBakis.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976D2"));
            btnGenelBakis.FontWeight = FontWeights.SemiBold;
        }

        private void MenuIslemler_Click(object sender, RoutedEventArgs e)
        {
            if (pnlIslemler == null) return;
            pnlGenelBakis.Visibility = Visibility.Collapsed;
            pnlPlanlanmis.Visibility = Visibility.Collapsed;
            pnlRaporlar.Visibility = Visibility.Collapsed;
            pnlPortfoy.Visibility = Visibility.Collapsed;
            if (pnlButce != null) pnlButce.Visibility = Visibility.Collapsed;
            if (pnlAyarlar != null) pnlAyarlar.Visibility = Visibility.Collapsed;

            pnlIslemler.Visibility = Visibility.Visible;

            TumIslemleriYukle();

            MenuButonlariniSifirla();
            btnIslemler.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
            btnIslemler.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976D2"));
            btnIslemler.FontWeight = FontWeights.SemiBold;
        }

        private void MenuPlanlanmis_Click(object sender, RoutedEventArgs e)
        {
            if (pnlPlanlanmis == null) return;
            pnlGenelBakis.Visibility = Visibility.Collapsed;
            pnlIslemler.Visibility = Visibility.Collapsed;
            pnlRaporlar.Visibility = Visibility.Collapsed;
            pnlPortfoy.Visibility = Visibility.Collapsed;
            if (pnlButce != null) pnlButce.Visibility = Visibility.Collapsed;
            if (pnlAyarlar != null) pnlAyarlar.Visibility = Visibility.Collapsed;

            pnlPlanlanmis.Visibility = Visibility.Visible;

            PlanlanmisOdemeleriYukle();

            MenuButonlariniSifirla();
            btnPlanlanmis.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
            btnPlanlanmis.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976D2"));
            btnPlanlanmis.FontWeight = FontWeights.SemiBold;
        }

        private void MenuRaporlar_Click(object sender, RoutedEventArgs e)
        {
            if (pnlRaporlar == null) return;
            pnlGenelBakis.Visibility = Visibility.Collapsed;
            pnlIslemler.Visibility = Visibility.Collapsed;
            pnlPlanlanmis.Visibility = Visibility.Collapsed;
            pnlPortfoy.Visibility = Visibility.Collapsed;
            if (pnlButce != null) pnlButce.Visibility = Visibility.Collapsed;
            if (pnlAyarlar != null) pnlAyarlar.Visibility = Visibility.Collapsed;

            pnlRaporlar.Visibility = Visibility.Visible;

            MenuButonlariniSifirla();
            btnRaporlar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
            btnRaporlar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976D2"));
            btnRaporlar.FontWeight = FontWeights.SemiBold;

            RaporlariYukle();
        }

        private void MenuPortfoy_Click(object sender, RoutedEventArgs e)
        {
            if (pnlPortfoy == null) return;
            pnlGenelBakis.Visibility = Visibility.Collapsed;
            pnlIslemler.Visibility = Visibility.Collapsed;
            pnlPlanlanmis.Visibility = Visibility.Collapsed;
            pnlRaporlar.Visibility = Visibility.Collapsed;
            if (pnlButce != null) pnlButce.Visibility = Visibility.Collapsed;
            if (pnlAyarlar != null) pnlAyarlar.Visibility = Visibility.Collapsed;

            pnlPortfoy.Visibility = Visibility.Visible;

            MenuButonlariniSifirla();
            btnPortfoy.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
            btnPortfoy.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976D2"));
            btnPortfoy.FontWeight = FontWeights.SemiBold;

            PortfoyYukle();
        }

        private void MenuButce_Click(object sender, RoutedEventArgs e)
        {
            if (pnlButce == null) return;
            pnlGenelBakis.Visibility = Visibility.Collapsed;
            pnlIslemler.Visibility = Visibility.Collapsed;
            pnlPlanlanmis.Visibility = Visibility.Collapsed;
            pnlRaporlar.Visibility = Visibility.Collapsed;
            pnlPortfoy.Visibility = Visibility.Collapsed;
            if (pnlAyarlar != null) pnlAyarlar.Visibility = Visibility.Collapsed;

            pnlButce.Visibility = Visibility.Visible;

            MenuButonlariniSifirla();
            btnButce.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
            btnButce.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976D2"));
            btnButce.FontWeight = FontWeights.SemiBold;

            ButceVeHedefleriYukle();
        }

        private void MenuAyarlar_Click(object sender, RoutedEventArgs e)
        {
            if (pnlAyarlar == null) return;
            pnlGenelBakis.Visibility = Visibility.Collapsed;
            pnlIslemler.Visibility = Visibility.Collapsed;
            pnlPlanlanmis.Visibility = Visibility.Collapsed;
            pnlRaporlar.Visibility = Visibility.Collapsed;
            pnlPortfoy.Visibility = Visibility.Collapsed;
            pnlButce.Visibility = Visibility.Collapsed;

            pnlAyarlar.Visibility = Visibility.Visible;

            MenuButonlariniSifirla();
            btnAyarlar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
            btnAyarlar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976D2"));
            btnAyarlar.FontWeight = FontWeights.SemiBold;

            KategorileriYukle();
        }

        private void TumIslemleriYukle()
        {
            try
            {
                string aramaMetni = txtIslemAra?.Text.Trim().ToLower() ?? "";
                int seciliFiltre = cmbIslemTipFiltre?.SelectedIndex ?? 0;

                using (var baglanti = new SqliteConnection(DatabaseManager.BaglantiCumlesi))
                {
                    baglanti.Open();
                    var islemListesi = new List<IslemModel>();

                    string sorgu = "SELECT Id, Baslik, Kategori, Miktar, Tip, Tarih FROM Islemler WHERE 1=1 ";

                    if (seciliFiltre == 1) sorgu += "AND Tip = 'Gelir' ";
                    else if (seciliFiltre == 2) sorgu += "AND Tip = 'Gider' ";

                    if (!string.IsNullOrEmpty(aramaMetni))
                    {
                        sorgu += "AND (LOWER(Baslik) LIKE @arama OR LOWER(Kategori) LIKE @arama) ";
                    }

                    sorgu += "ORDER BY Tarih DESC, Id DESC";

                    var komut = baglanti.CreateCommand();
                    komut.CommandText = sorgu;

                    if (!string.IsNullOrEmpty(aramaMetni))
                    {
                        komut.Parameters.AddWithValue("@arama", $"%{aramaMetni}%");
                    }

                    using (var okuyucu = komut.ExecuteReader())
                    {
                        while (okuyucu.Read())
                        {
                            bool isGelir = okuyucu.GetString(4) == "Gelir";
                            islemListesi.Add(new IslemModel
                            {
                                Id = okuyucu.GetInt32(0),
                                Baslik = okuyucu.GetString(1),
                                Kategori = okuyucu.GetString(2),
                                Tarih = Convert.ToDateTime(okuyucu.GetString(5)).ToString("dd.MM.yyyy"),
                                MiktarMetni = isGelir ? $"+₺{okuyucu.GetDouble(3):N2}" : $"-₺{okuyucu.GetDouble(3):N2}",
                                RenkBrush = isGelir ? new SolidColorBrush(Color.FromRgb(16, 185, 129)) : new SolidColorBrush(Color.FromRgb(239, 68, 68))
                            });
                        }
                    }

                    if (dgIslemler != null)
                    {
                        dgIslemler.ItemsSource = islemListesi;
                        if (txtFiltreSonucYok != null)
                            txtFiltreSonucYok.Visibility = islemListesi.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine("Tablo yüklenemedi: " + ex.Message); }
        }

        private void TxtIslemAra_TextChanged(object sender, TextChangedEventArgs e)
        {
            TumIslemleriYukle();
        }

        private void CmbIslemFiltre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TumIslemleriYukle();
        }

        private void BtnIslemSil_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Tag != null)
            {
                int silinecekId = (int)btn.Tag;
                var cevap = MessageBox.Show("Bu işlemi kalıcı olarak silmek istediğinize emin misiniz?", "Kayıt Silinecek", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (cevap == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var baglanti = new SqliteConnection(DatabaseManager.BaglantiCumlesi))
                        {
                            baglanti.Open();
                            var komut = baglanti.CreateCommand();
                            komut.CommandText = "DELETE FROM Islemler WHERE Id = @id";
                            komut.Parameters.AddWithValue("@id", silinecekId);
                            komut.ExecuteNonQuery();
                        }

                        VerileriGuncelle();
                        TumIslemleriYukle();
                    }
                    catch (Exception ex) { MessageBox.Show("Silme işlemi sırasında hata oluştu: " + ex.Message); }
                }
            }
        }

        private void YeniOdemePlanla_Click(object sender, RoutedEventArgs e)
        {
            OdemePlanlaWindow pencere = new OdemePlanlaWindow();
            pencere.Owner = this;
            pencere.ShowDialog();
            PlanlanmisOdemeleriYukle();
        }

        private void PlanlanmisOdemeleriYukle()
        {
            try
            {
                using (var baglanti = new SqliteConnection(DatabaseManager.BaglantiCumlesi))
                {
                    baglanti.Open();
                    double tlBakiye = DbDenDegerGetir(baglanti, "SELECT SUM(Miktar) FROM Islemler WHERE Tip='Gelir'") - DbDenDegerGetir(baglanti, "SELECT SUM(Miktar) FROM Islemler WHERE Tip='Gider'");

                    var liste = new List<IslemModel>();
                    double toplamPlanlananGider = 0;

                    var komut = baglanti.CreateCommand();
                    komut.CommandText = "SELECT Id, Baslik, Miktar, OdemeGunu, Kategori FROM PlanlanmisOdemeler ORDER BY OdemeGunu ASC";

                    using (var okuyucu = komut.ExecuteReader())
                    {
                        while (okuyucu.Read())
                        {
                            int id = okuyucu.GetInt32(0);
                            double miktar = okuyucu.GetDouble(2);
                            toplamPlanlananGider += miktar;

                            liste.Add(new IslemModel
                            {
                                Id = id,
                                Baslik = okuyucu.GetString(1),
                                Tarih = $"Her ayın {okuyucu.GetInt32(3)}'i",
                                MiktarMetni = $"-₺{miktar:N2}",
                                RenkBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"))
                            });
                        }
                    }

                    if (lstPlanlanmisOdemeler != null) lstPlanlanmisOdemeler.ItemsSource = liste;
                    if (txtPlanlananToplam != null) txtPlanlananToplam.Text = $"₺{toplamPlanlananGider:N2}";
                    if (txtPlanlananKalan != null) txtPlanlananKalan.Text = $"₺{(tlBakiye - toplamPlanlananGider):N2}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ödemeler yüklenemedi: " + ex.Message);
            }
        }

        private void BtnOdemeSil_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Tag != null)
            {
                int silinecekId = (int)btn.Tag;
                MessageBoxResult cevap = MessageBox.Show("Bu planlanmış ödemeyi silmek istediğinize emin misiniz?", "Ödemeyi Sil", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (cevap == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var baglanti = new SqliteConnection(DatabaseManager.BaglantiCumlesi))
                        {
                            baglanti.Open();
                            var komut = baglanti.CreateCommand();
                            komut.CommandText = "DELETE FROM PlanlanmisOdemeler WHERE Id = @id";
                            komut.Parameters.AddWithValue("@id", silinecekId);
                            komut.ExecuteNonQuery();
                        }
                        PlanlanmisOdemeleriYukle();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Silme işlemi sırasında hata oluştu: " + ex.Message);
                    }
                }
            }
        }

        private void RaporlariYukle()
        {
            if (cmbRaporDonem == null || lstRaporKategori == null) return;
            ComboBoxItem seciliItem = (ComboBoxItem)cmbRaporDonem.SelectedItem;
            if (seciliItem == null) return;

            string secilenDonem = seciliItem.Content?.ToString() ?? "Bu Ay";

            DateTime simdi = DateTime.Now;
            string baslangicTarihi = "";
            string bitisTarihi = "";

            if (secilenDonem == "Bu Ay")
            {
                baslangicTarihi = new DateTime(simdi.Year, simdi.Month, 1).ToString("dd.MM.yyyy");
                bitisTarihi = new DateTime(simdi.Year, simdi.Month, DateTime.DaysInMonth(simdi.Year, simdi.Month)).ToString("dd.MM.yyyy");
            }
            else if (secilenDonem == "Geçen Ay")
            {
                DateTime gecenAy = simdi.AddMonths(-1);
                baslangicTarihi = new DateTime(gecenAy.Year, gecenAy.Month, 1).ToString("dd.MM.yyyy");
                bitisTarihi = new DateTime(gecenAy.Year, gecenAy.Month, DateTime.DaysInMonth(gecenAy.Year, gecenAy.Month)).ToString("dd.MM.yyyy");
            }
            else // Bu Yıl
            {
                baslangicTarihi = "01.01." + simdi.Year;
                bitisTarihi = "31.12." + simdi.Year;
            }

            try
            {
                using (var baglanti = new SqliteConnection(DatabaseManager.BaglantiCumlesi))
                {
                    baglanti.Open();
                    var komut = baglanti.CreateCommand();

                    komut.CommandText = @"SELECT Kategori, SUM(Miktar) 
                                         FROM Islemler 
                                         WHERE Tip = 'Gider' 
                                         AND Tarih BETWEEN @bas AND @bit 
                                         GROUP BY Kategori 
                                         ORDER BY SUM(Miktar) DESC";

                    komut.Parameters.AddWithValue("@bas", baslangicTarihi);
                    komut.Parameters.AddWithValue("@bit", bitisTarihi);

                    var kategoriListesi = new List<KategoriRaporModel>();
                    string[] renkler = { "#3B82F6", "#EF4444", "#F59E0B", "#10B981", "#8B5CF6", "#EC4899", "#14B8A6" };
                    int renkIndex = 0;

                    RaporSerisi.Clear();

                    using (var okuyucu = komut.ExecuteReader())
                    {
                        while (okuyucu.Read())
                        {
                            string kategoriAdi = okuyucu.GetString(0);
                            double miktar = Math.Abs(okuyucu.GetDouble(1));

                            string hexRenk = renkler[renkIndex % renkler.Length];
                            var firca = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexRenk));

                            kategoriListesi.Add(new KategoriRaporModel
                            {
                                KategoriAdi = kategoriAdi,
                                Tutar = miktar,
                                TutarMetni = $"-₺{miktar:N2}",
                                RenkBrush = firca
                            });

                            RaporSerisi.Add(new PieSeries<double>
                            {
                                Values = new double[] { miktar },
                                Name = kategoriAdi,
                                Fill = new SolidColorPaint(SKColor.Parse(hexRenk)),
                                InnerRadius = 50,
                                Pushout = 3,
                                HoverPushout = 10
                            });
                            renkIndex++;
                        }
                    }

                    if (txtRaporVeriYok != null)
                        txtRaporVeriYok.Visibility = (RaporSerisi.Count == 0) ? Visibility.Visible : Visibility.Collapsed;

                    if (RaporSerisi.Count == 0)
                    {
                        RaporSerisi.Add(new PieSeries<double> { Values = new double[] { 1 }, Fill = new SolidColorPaint(SKColor.Parse("#F1F5F9")), InnerRadius = 50, HoverPushout = 0 });
                    }

                    lstRaporKategori.ItemsSource = kategoriListesi;
                }
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }

        private void cmbRaporDonem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RaporlariYukle();
        }

        public void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        public void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        public void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal) WindowState = WindowState.Maximized;
            else WindowState = WindowState.Normal;
        }

        public void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void PortfoyYukle()
        {
            try
            {
                double usdKuru = await KurServisi.KuruGetir("USD");
                double eurKuru = await KurServisi.KuruGetir("EUR");
                double altinKuru = await KurServisi.MadenGetir("gram-altin");
                double gumusKuru = await KurServisi.MadenGetir("gumus");

                using (var baglanti = new SqliteConnection(DatabaseManager.BaglantiCumlesi))
                {
                    baglanti.Open();
                    var komut = baglanti.CreateCommand();

                    komut.CommandText = @"
                        SELECT 
                            Tur, 
                            SUM(Miktar) as ToplamMiktar, 
                            SUM(Miktar * AlisFiyati) / SUM(Miktar) as OrtalamaMaliyet 
                        FROM Varliklar 
                        GROUP BY Tur 
                        HAVING SUM(Miktar) > 0";

                    var portfoyListesi = new List<VarlikPortfoyModel>();
                    double genelToplamKarZarar = 0;

                    using (var okuyucu = komut.ExecuteReader())
                    {
                        while (okuyucu.Read())
                        {
                            string tur = okuyucu.GetString(0);
                            double miktar = okuyucu.GetDouble(1);
                            double ortMaliyet = okuyucu.GetDouble(2);

                            double guncelFiyat = 0;
                            string ikon = "";
                            string isim = "";
                            string birim = "";

                            switch (tur)
                            {
                                case "USD": guncelFiyat = usdKuru; ikon = "💵"; isim = "Amerikan Doları"; birim = "$"; break;
                                case "EUR": guncelFiyat = eurKuru; ikon = "💶"; isim = "Euro"; birim = "€"; break;
                                case "GLD": guncelFiyat = altinKuru; ikon = "🪙"; isim = "Gram Altın"; birim = "g"; break;
                                case "SLV": guncelFiyat = gumusKuru; ikon = "🥈"; isim = "Gram Gümüş"; birim = "g"; break;
                            }

                            double yatirilanAnaPara = miktar * ortMaliyet;
                            double suAnkiDeger = miktar * guncelFiyat;
                            double karZararTL = suAnkiDeger - yatirilanAnaPara;
                            double karZararYuzde = (karZararTL / yatirilanAnaPara) * 100;

                            genelToplamKarZarar += karZararTL;

                            SolidColorBrush renk = karZararTL >= 0
                                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"))
                                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));

                            string isaret = karZararTL >= 0 ? "+" : "";

                            portfoyListesi.Add(new VarlikPortfoyModel
                            {
                                Tur = tur,
                                Ikon = ikon,
                                VarlikAdi = isim,
                                MiktarMetni = $"{miktar:N2} {birim}",
                                MaliyetMetni = $"Ort. Maliyet: ₺{ortMaliyet:N2}",
                                GuncelFiyatMetni = $"Güncel: ₺{guncelFiyat:N2}",
                                ToplamDegerMetni = $"₺{suAnkiDeger:N2}",
                                KarZararMetni = $"{isaret}₺{karZararTL:N2} (%{karZararYuzde:N2})",
                                KarZararRenk = renk
                            });
                        }
                    }

                    if (lstPortfoy != null) lstPortfoy.ItemsSource = portfoyListesi;

                    if (txtToplamKarZarar != null)
                    {
                        string genelIsaret = genelToplamKarZarar >= 0 ? "+" : "";
                        txtToplamKarZarar.Text = $"{genelIsaret}₺{genelToplamKarZarar:N2}";
                        txtToplamKarZarar.Foreground = genelToplamKarZarar >= 0
                            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"))
                            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Portföy Hatası: " + ex.Message, "Hata Yakalandı", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButceVeHedefleriYukle()
        {
            try
            {
                using (var baglanti = new SqliteConnection(DatabaseManager.BaglantiCumlesi))
                {
                    baglanti.Open();

                    var hedefListesi = new List<HedefModel>();
                    var cmdHedef = baglanti.CreateCommand();
                    cmdHedef.CommandText = "SELECT Id, Baslik, Tur, ToplamTutar, BirikenTutar, Ikon FROM Hedefler";
                    using (var reader = cmdHedef.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string baslik = reader.GetString(1);
                            bool isBorc = reader.GetString(2) == "Borc";
                            double toplam = reader.GetDouble(3);
                            double biriken = reader.GetDouble(4);
                            string ikon = reader.GetString(5);

                            double yuzde = (biriken / toplam) * 100;
                            if (yuzde > 100) yuzde = 100;

                            hedefListesi.Add(new HedefModel
                            {
                                Id = id,
                                Baslik = baslik,
                                Ikon = ikon,
                                Yuzde = yuzde,
                                YuzdeMetni = $"%{Math.Round(yuzde)}",
                                OzetMetni = isBorc ? $"Kalan: ₺{(toplam - biriken):N0} / Toplam: ₺{toplam:N0}" : $"Biriken: ₺{biriken:N0} / Hedef: ₺{toplam:N0}",
                                RenkBrush = isBorc ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E11D48")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")),
                                ArkaPlanBrush = isBorc ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF1F2")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8FAFC")),
                                YaziRenkBrush = isBorc ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BE123C")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"))
                            });
                        }
                    }
                    if (lstHedefler != null) lstHedefler.ItemsSource = hedefListesi;

                    var butceListesi = new List<ButceModel>();
                    var cmdButce = baglanti.CreateCommand();
                    cmdButce.CommandText = "SELECT Kategori, AylikLimit FROM Butceler";
                    string buAy = DateTime.Now.ToString("yyyy-MM");

                    using (var reader = cmdButce.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string kategori = reader.GetString(0);
                            double limit = reader.GetDouble(1);

                            double harcanan = DbDenDegerGetir(baglanti, $"SELECT SUM(Miktar) FROM Islemler WHERE Tip='Gider' AND Kategori='{kategori}' AND strftime('%Y-%m', Tarih) = '{buAy}'");

                            double yuzde = (harcanan / limit) * 100;
                            bool tehlike = yuzde >= 85;

                            butceListesi.Add(new ButceModel
                            {
                                Kategori = kategori,
                                Yuzde = yuzde > 100 ? 100 : yuzde,
                                LimitMetni = $"₺{harcanan:N0} / ₺{limit:N0}",
                                RenkBrush = tehlike ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6")),
                                YaziRenkBrush = tehlike ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B"))
                            });
                        }
                    }
                    if (lstButceler != null) lstButceler.ItemsSource = butceListesi;
                }
            }
            catch (Exception ex) { Console.WriteLine("Bütçe Hata: " + ex.Message); }
        }

        // EKSİK OLAN BÜTÇE SİLME METODU
        private void BtnButceSil_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Tag != null)
            {
                string kategoriAdi = btn.Tag.ToString();
                var cevap = MessageBox.Show($"'{kategoriAdi}' kategorisi için tanımlanmış bütçeyi silmek istediğinize emin misiniz?", "Bütçe Silme", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (cevap == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var baglanti = new SqliteConnection(DatabaseManager.BaglantiCumlesi))
                        {
                            baglanti.Open();
                            var komut = baglanti.CreateCommand();
                            komut.CommandText = "DELETE FROM Butceler WHERE Kategori = @kategori";
                            komut.Parameters.AddWithValue("@kategori", kategoriAdi);
                            komut.ExecuteNonQuery();
                        }

                        ButceVeHedefleriYukle();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Bütçe silinirken hata oluştu: " + ex.Message);
                    }
                }
            }
        }

        // EKSİK OLAN HEDEF SİLME METODU
        private void BtnHedefSil_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Tag != null)
            {
                int hedefId = (int)btn.Tag;
                var cevap = MessageBox.Show("Bu hedefi/birikimi kalıcı olarak silmek istediğinize emin misiniz?", "Hedef Silme", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (cevap == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var baglanti = new SqliteConnection(DatabaseManager.BaglantiCumlesi))
                        {
                            baglanti.Open();
                            var komut = baglanti.CreateCommand();
                            komut.CommandText = "DELETE FROM Hedefler WHERE Id = @id";
                            komut.Parameters.AddWithValue("@id", hedefId);
                            komut.ExecuteNonQuery();
                        }

                        ButceVeHedefleriYukle();
                        VerileriGuncelle();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Hedef silinirken hata oluştu: " + ex.Message);
                    }
                }
            }
        }

        private void HedefParaEkle_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Tag != null)
            {
                int hedefId = (int)btn.Tag;
                double eklenecekTutar = 500;

                try
                {
                    using (var baglanti = new SqliteConnection(DatabaseManager.BaglantiCumlesi))
                    {
                        baglanti.Open();

                        string hedefBaslik = "Hedef";
                        var cmdBilgi = baglanti.CreateCommand();
                        cmdBilgi.CommandText = "SELECT Baslik FROM Hedefler WHERE Id = @id";
                        cmdBilgi.Parameters.AddWithValue("@id", hedefId);
                        using (var reader = cmdBilgi.ExecuteReader()) { if (reader.Read()) hedefBaslik = reader.GetString(0); }

                        var cmdGuncelle = baglanti.CreateCommand();
                        cmdGuncelle.CommandText = "UPDATE Hedefler SET BirikenTutar = BirikenTutar + @tutar WHERE Id = @id";
                        cmdGuncelle.Parameters.AddWithValue("@tutar", eklenecekTutar);
                        cmdGuncelle.Parameters.AddWithValue("@id", hedefId);
                        cmdGuncelle.ExecuteNonQuery();

                        var cmdIslem = baglanti.CreateCommand();
                        cmdIslem.CommandText = "INSERT INTO Islemler (Baslik, Miktar, Tip, Hesap, Kategori, Tarih) VALUES (@baslik, @miktar, 'Gider', 'Cüzdan', 'Birikim/Yatırım', @tarih)";
                        cmdIslem.Parameters.AddWithValue("@baslik", $"{hedefBaslik} (Para Aktarımı)");
                        cmdIslem.Parameters.AddWithValue("@miktar", eklenecekTutar);
                        cmdIslem.Parameters.AddWithValue("@tarih", DateTime.Now.ToString("yyyy-MM-dd"));
                        cmdIslem.ExecuteNonQuery();
                    }

                    ButceVeHedefleriYukle();
                    VerileriGuncelle();
                }
                catch (Exception ex) { MessageBox.Show("Para aktarılırken hata: " + ex.Message); }
            }
        }

        private void YeniHedefEkle_Click(object sender, RoutedEventArgs e)
        {
            HedefEkleWindow pencere = new HedefEkleWindow();
            pencere.Owner = this;
            pencere.ShowDialog();

            ButceVeHedefleriYukle();
            VerileriGuncelle();
        }

        private void BtnYedekAl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.FileName = $"finansim_yedek_{DateTime.Now:yyyyMMdd}.db";
                dialog.Filter = "Veritabanı Dosyası (*.db)|*.db";

                if (dialog.ShowDialog() == true)
                {
                    string dbYolu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BenimFinansim", "finansim.db");
                    File.Copy(dbYolu, dialog.FileName, true);

                    MessageBox.Show("Mükemmel! Tüm finansal verilerinizin yedeği başarıyla alındı.", "Yedekleme Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yedek alınırken teknik bir hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnYedekYukle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult onay = MessageBox.Show("Uyarı: Mevcut sistemdeki verileriniz silinecek ve seçtiğiniz yedek dosyası yüklenecek. Emin misiniz?", "Yedeği Yükle", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (onay == MessageBoxResult.Yes)
                {
                    Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
                    dialog.Filter = "Veritabanı Dosyası (*.db)|*.db";

                    if (dialog.ShowDialog() == true)
                    {
                        string dbYolu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BenimFinansim", "finansim.db");
                        File.Copy(dialog.FileName, dbYolu, true);

                        MessageBox.Show("Yedek başarıyla yüklendi! Lütfen uygulamanın yeni verileri okuması için programı kapatıp tekrar açın.", "Geri Yükleme Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

                        VerileriGuncelle();
                        KategorileriYukle();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yedek yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void KategorileriYukle()
        {
            try
            {
                using (var baglanti = new SqliteConnection(DatabaseManager.BaglantiCumlesi))
                {
                    baglanti.Open();

                    new SqliteCommand("CREATE TABLE IF NOT EXISTS Kategoriler (Id INTEGER PRIMARY KEY AUTOINCREMENT, Ad TEXT NOT NULL)", baglanti).ExecuteNonQuery();

                    long count = (long)new SqliteCommand("SELECT COUNT(*) FROM Kategoriler", baglanti).ExecuteScalar();
                    if (count == 0)
                    {
                        string[] varsayilanlar = { "Market & Gıda", "Fatura & Faturalar", "Eğlence", "Maaş", "Eğitim", "Sağlık", "Diğer" };
                        foreach (var k in varsayilanlar)
                        {
                            var cmd = baglanti.CreateCommand();
                            cmd.CommandText = "INSERT INTO Kategoriler (Ad) VALUES (@ad)";
                            cmd.Parameters.AddWithValue("@ad", k);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    var liste = new List<KategoriModel>();
                    var cmdList = baglanti.CreateCommand();
                    cmdList.CommandText = "SELECT Id, Ad FROM Kategoriler ORDER BY Ad ASC";
                    using (var reader = cmdList.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            liste.Add(new KategoriModel { Id = reader.GetInt32(0), Ad = reader.GetString(1) });
                        }
                    }

                    if (lstKategorilerAyarlar != null) lstKategorilerAyarlar.ItemsSource = liste;
                }
            }
            catch (Exception ex) { Console.WriteLine("Kategori yükleme hatası: " + ex.Message); }
        }

        private void BtnKategoriEkle_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtYeniKategori.Text)) return;

            try
            {
                using (var baglanti = new SqliteConnection(DatabaseManager.BaglantiCumlesi))
                {
                    baglanti.Open();
                    var cmd = baglanti.CreateCommand();
                    cmd.CommandText = "INSERT INTO Kategoriler (Ad) VALUES (@ad)";
                    cmd.Parameters.AddWithValue("@ad", txtYeniKategori.Text.Trim());
                    cmd.ExecuteNonQuery();
                }

                txtYeniKategori.Text = "";
                KategorileriYukle();
            }
            catch (Exception ex) { MessageBox.Show("Kategori eklenemedi: " + ex.Message); }
        }

        private void BtnKategoriSil_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Tag != null)
            {
                int id = (int)btn.Tag;
                try
                {
                    using (var baglanti = new SqliteConnection(DatabaseManager.BaglantiCumlesi))
                    {
                        baglanti.Open();
                        var cmd = baglanti.CreateCommand();
                        cmd.CommandText = "DELETE FROM Kategoriler WHERE Id = @id";
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                    KategorileriYukle();
                }
                catch (Exception ex) { MessageBox.Show("Kategori silinemedi: " + ex.Message); }
            }
        }

        private void BtnExcelAktar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.FileName = $"BenimFinansim_Islemler_{DateTime.Now:yyyyMMdd}.xlsx";
                dialog.Filter = "Excel Dosyası (*.xlsx)|*.xlsx";

                if (dialog.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("İşlem Geçmişi");

                        worksheet.Cell(1, 1).Value = "Tarih";
                        worksheet.Cell(1, 2).Value = "Başlık (Açıklama)";
                        worksheet.Cell(1, 3).Value = "Kategori";
                        worksheet.Cell(1, 4).Value = "Tip";
                        worksheet.Cell(1, 5).Value = "Miktar (TL)";

                        var baslikSatiri = worksheet.Range("A1:E1");
                        baslikSatiri.Style.Font.Bold = true;
                        baslikSatiri.Style.Fill.BackgroundColor = XLColor.LightGray;

                        int satir = 2;
                        using (var baglanti = new SqliteConnection(DatabaseManager.BaglantiCumlesi))
                        {
                            baglanti.Open();
                            var komut = baglanti.CreateCommand();
                            komut.CommandText = "SELECT Tarih, Baslik, Kategori, Tip, Miktar FROM Islemler ORDER BY Tarih DESC";

                            using (var reader = komut.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    worksheet.Cell(satir, 1).Value = reader.GetString(0);
                                    worksheet.Cell(satir, 2).Value = reader.GetString(1);
                                    worksheet.Cell(satir, 3).Value = reader.GetString(2);

                                    string tip = reader.GetString(3);
                                    worksheet.Cell(satir, 4).Value = tip;
                                    worksheet.Cell(satir, 5).Value = reader.GetDouble(4);

                                    if (tip == "Gelir")
                                        worksheet.Cell(satir, 5).Style.Font.FontColor = XLColor.SeaGreen;
                                    else
                                        worksheet.Cell(satir, 5).Style.Font.FontColor = XLColor.Crimson;

                                    satir++;
                                }
                            }
                        }

                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(dialog.FileName);
                    }

                    MessageBox.Show("İşlemleriniz başarıyla Excel'e aktarıldı!", "İşlem Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Excel'e aktarılırken bir hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnVarlikSil_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Tag != null)
            {
                string silinecekTur = btn.Tag.ToString();
                var cevap = MessageBox.Show("Bu varlık türündeki tüm kayıtları kalıcı olarak silmek (sıfırlamak) istediğinize emin misiniz?", "Varlığı Sıfırla", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (cevap == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var baglanti = new SqliteConnection(DatabaseManager.BaglantiCumlesi))
                        {
                            baglanti.Open();
                            var komut = baglanti.CreateCommand();
                            komut.CommandText = "DELETE FROM Varliklar WHERE Tur = @tur";
                            komut.Parameters.AddWithValue("@tur", silinecekTur);
                            komut.ExecuteNonQuery();
                        }

                        VerileriGuncelle();
                        PortfoyYukle();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Silme işlemi sırasında hata oluştu: " + ex.Message);
                    }
                }
            }
        }
    }
}