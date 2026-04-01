using System;
using System.Windows;
using System.Windows.Threading;

namespace BenimFinansim;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // 1. ÖNCE: Uygulama genelinde bir hata olursa yakalaması için bu satırı ekliyoruz
        DispatcherUnhandledException += App_DispatcherUnhandledException;

        try
        {
            // 2. KRİTİK ADIM: Uygulama daha açılmadan veritabanını kuruyoruz.
            // Bu sayede MainWindow açıldığında tablolar çoktan hazır olmuş oluyor.
            DatabaseManager.VeritabaniniKur();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Veritabanı başlatılırken hata oluştu: {ex.Message}", "Kritik Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        base.OnStartup(e);
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // Uygulama çalışırken beklenmedik bir hata olursa (SQL hatası, DLL hatası vb.) 
        // sessizce kapanmak yerine bize ne olduğunu söylesin.
        MessageBox.Show($"Beklenmedik bir hata oluştu:\n\n{e.Exception.Message}", "Uygulama Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
        
        // Hatanın uygulamayı tamamen çökertmesini engellemek için (isteğe bağlı)
        e.Handled = true; 
    }
}