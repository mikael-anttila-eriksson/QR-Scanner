using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using UIApp.Views;
using UIApp.ViewModels;
using SQLitePCL;
using ZXing.Net.Maui.Controls;

namespace UIApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // Initialize native SQLite provider (required by sqlite-net-pcl)
            Batteries_V2.Init();

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseBarcodeReader(); // Add ZXing controls to MAUI
            
            

            // Register pages and viewmodels for DI
            builder.Services.AddTransient<ScannerPage>();
            builder.Services.AddTransient<HistoryPage>();
            builder.Services.AddTransient<ResultDetailPage>();
            builder.Services.AddTransient<GeneratorPage>();
            builder.Services.AddTransient<SettingsPage>();

            builder.Services.AddTransient<ScannerViewModel>();
            builder.Services.AddTransient<HistoryViewModel>();
            builder.Services.AddTransient<ResultDetailViewModel>();
            builder.Services.AddTransient<GeneratorViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();

            // Register services (concrete types used directly; no interfaces for this simple app)
            builder.Services.AddSingleton<Services.SqliteStorageService>();
            builder.Services.AddSingleton<Services.ScannerService>();
            // Sound service: default no-op implementation (platform-specific implementations may override)
            builder.Services.AddSingleton<Services.ISoundService, Services.NoOpSoundService>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
