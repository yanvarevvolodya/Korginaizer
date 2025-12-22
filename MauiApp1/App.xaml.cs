namespace MauiApp1
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            // MainPage НЕ устанавливаем здесь
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            // Явно создаём Shell и окно
            var shell = new AppShell();
            var window = new Window(shell);

#if WINDOWS || MACCATALYST
    window.MinimumHeight = 670;
    window.MinimumWidth = 760;
#endif

            return window;
        }
    }
}