namespace MauiApp1
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                // Для Windows
                MainPage = new TasksPage();
            }
            else
            {
                // Для Android
                MainPage = new AppShell();
            }
        }        
    }
}