using System.Diagnostics;

namespace MauiApp1
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var shell = new AppShell();
            var window = new Window(shell);

#if WINDOWS || MACCATALYST
    window.MinimumHeight = 670;
    window.MinimumWidth = 760;
#endif

            return window;
        }

        protected override void OnStart()
        {
            base.OnStart();
            RequestNotificationPermission();
        }

        private async void RequestNotificationPermission()
        {
#if ANDROID
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                var status = await Permissions.RequestAsync<Permissions.PostNotifications>();
                Debug.WriteLine($"[PERM] Разрешение на уведомления: {status}");
            }
#endif
        }
    }
}