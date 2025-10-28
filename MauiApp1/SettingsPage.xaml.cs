namespace MauiApp1
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void OnTasksButtonClicked(object sender, EventArgs e)
        {
            Application.Current.MainPage = new TasksPage();
        }

        private void OnSettingsButtonClicked(object sender, EventArgs e)
        {
            
        }

        private void OnCalendarButtonClicked(object sender, EventArgs e)
        {
            Application.Current.MainPage = new CalendarPage();
        }
    }
}