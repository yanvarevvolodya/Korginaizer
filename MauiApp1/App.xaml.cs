namespace MauiApp1
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();


            //if (DeviceInfo.Platform == DevicePlatform.WinUI)
            //{
            //    // Для Windows
            //    MainPage = new AppShell();
            //}
            //else
            //{
            //    // Для Android
            //    MainPage = new AppShell();
            //}
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}