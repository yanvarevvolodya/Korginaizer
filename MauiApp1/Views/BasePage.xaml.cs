namespace MauiApp1.Views
{
    public partial class BasePage : ContentPage
    {
        public BasePage()
        {
            InitializeComponent();
        }

        // Общие методы навигации
        protected void OnTasksButtonClicked(object sender, EventArgs e)
        {
            if (!(this is TasksPage))
            {
                Application.Current.MainPage = new TasksPage();
            }
        }

        protected void OnCalendarButtonClicked(object sender, EventArgs e)
        {
            if (!(this is CalendarPage))
            {
                Application.Current.MainPage = new CalendarPage();
            }
        }

        protected void OnSettingsButtonClicked(object sender, EventArgs e)
        {
            if (!(this is SettingsPage))
            {
                Application.Current.MainPage = new SettingsPage();
            }
        }

        // Методы для настройки страницы
        protected void SetHeaderTitle(string title)
        {
            HeaderTitleLabel.Text = title;
        }

        protected void SetHeaderButton(View button)
        {
            HeaderButtonArea.Content = button;
        }

        protected void SetPageContent(View content)
        {
            PageContentArea.Content = content;
        }

        // Подсветка активной кнопки
        protected void SetActiveButton(string activePage)
        {
            // Сбрасываем все кнопки
            TasksButton.BackgroundColor = Colors.Transparent;
            CalendarButton.BackgroundColor = Colors.Transparent;
            SettingsButton.BackgroundColor = Colors.Transparent;

            // Подсвечиваем активную
            switch (activePage)
            {
                case "TasksPage":
                    TasksButton.BackgroundColor = Color.FromArgb("#1a4d80");
                    break;
                case "CalendarPage":
                    CalendarButton.BackgroundColor = Color.FromArgb("#1a4d80");
                    break;
                case "SettingsPage":
                    SettingsButton.BackgroundColor = Color.FromArgb("#1a4d80");
                    break;
            }
        }
    }
}