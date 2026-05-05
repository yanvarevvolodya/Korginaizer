using CommunityToolkit.Maui;
using MauiApp1.Services;
using MauiApp1.ViewModels;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;


namespace MauiApp1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseLocalNotification(config =>
                {
                    config.AddAndroid(android =>
                    {
                        android.AddChannel(new NotificationChannelRequest
                        {
                            Id = "default_channel_id",
                            Name = "Основные уведомления",
                            Description = "Напоминания о задачах",
                            Importance = AndroidImportance.High,
                            EnableSound = true,
                            EnableVibration = true
                        });
                    });
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddTransient<TasksViewModel>();
            builder.Services.AddTransient<TasksPage>();
            builder.Services.AddTransient<CalendarPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<MainPage>();

            return builder.Build();
        }
    }
}
