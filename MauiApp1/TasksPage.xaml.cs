namespace MauiApp1
{
    public partial class TasksPage : ContentPage
    {
        public TasksPage()
        {
            InitializeComponent();

            LoadTasksContent();
        }

        private void OnToggleCategoriesButtonClicked(object sender, EventArgs e)
        {
            // Если текущий контент - TasksContent, вызываем его метод
            if (MainContentArea.Content is Views.TasksContent tasksContent)
            {
                tasksContent.ToggleCategoriesPanel();
            }
        }

        private void LoadTasksContent()
        {
            MainContentArea.Content = new Views.TasksContent();
        }

        private void LoadCalendarContent()
        {
            MainContentArea.Content = new Views.CalendarContent();
        }

        private void LoadSettingsContent()
        {
            MainContentArea.Content = new Views.SettingsContent();
        }


        // Обработчики кнопок (добавьте их в XAML с Clicked)
        private void OnTasksButtonClicked(object sender, EventArgs e)
        {
            LoadTasksContent();
            HeaderTitleLabel.Text = "Задачи";
        }

        private void OnCalendarButtonClicked(object sender, EventArgs e)
        {
            LoadCalendarContent();
            HeaderTitleLabel.Text = "Календарь";
        }

        private void OnSettingsButtonClicked(object sender, EventArgs e)
        {
            LoadSettingsContent();
            HeaderTitleLabel.Text = "Настройки";
        }
    }
}