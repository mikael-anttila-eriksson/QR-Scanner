using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using UIApp.Views;
using UIApp.ViewModels;

namespace UIApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register pages and viewmodels for DI (skeletons for MVP implementation)
            builder.Services.AddTransient<ScannerPage>();
            builder.Services.AddTransient<HistoryPage>();
            builder.Services.AddTransient<ResultDetailPage>();

            builder.Services.AddTransient<ScannerViewModel>();
            builder.Services.AddTransient<HistoryViewModel>();
            builder.Services.AddTransient<ResultDetailViewModel>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
