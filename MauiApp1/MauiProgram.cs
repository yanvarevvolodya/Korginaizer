using Microsoft.Extensions.Logging;
using MauiApp1.Services;
using MauiApp1.ViewModels;


namespace MauiApp1
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

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            // Регистрируем сервисы
            builder.Services.AddSingleton<DatabaseService>();

            // Регистрируем ViewModels
            builder.Services.AddTransient<TasksViewModel>();

            // Регистрируем Pages
            builder.Services.AddTransient<TasksPage>();
            builder.Services.AddTransient<CalendarPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<MainPage>();

            return builder.Build();
        }
    }
}
