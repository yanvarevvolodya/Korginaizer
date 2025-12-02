namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

        }

        private void NavigateToPage(ContentPage page)
        {
            MainFrame.Content = page.Content;

            if (page.BindingContext != null)
            {
                MainFrame.BindingContext = page.BindingContext;
            }
        }

        private void UpdateActiveButton(string activeButton)
        {
            // Сбрасываем все кнопки
            TasksButton.BackgroundColor = Colors.Transparent;
            CalendarButton.BackgroundColor = Colors.Transparent;
            SettingsButton.BackgroundColor = Colors.Transparent;

            // Подсвечиваем активную
            switch (activeButton)
            {
                case "Tasks":
                    TasksButton.BackgroundColor = Color.FromArgb("#1a4d80");
                    break;
                case "Calendar":
                    CalendarButton.BackgroundColor = Color.FromArgb("#1a4d80");
                    break;
                case "Settings":
                    SettingsButton.BackgroundColor = Color.FromArgb("#1a4d80");
                    break;
            }
        }

        private void TasksButton_Clicked(object sender, EventArgs e)
        {
            NavigateToPage(new TasksPage());
            UpdateActiveButton("Tasks");
        }

        private void CalendarButton_Clicked(object sender, EventArgs e)
        {
            NavigateToPage(new CalendarPage());
            UpdateActiveButton("Calendar");
        }

        private void SettingsButton_Clicked(object sender, EventArgs e)
        {
            NavigateToPage(new SettingsPage());
            UpdateActiveButton("Settings");
        }
    }
}
