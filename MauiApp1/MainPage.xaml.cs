namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnMenuClicked(object sender, EventArgs e)
        {
            // Пока просто сообщение
            DisplayAlert("Меню", "Здесь будет боковое меню", "OK");
        }

        private void OnAddTaskClicked(object sender, EventArgs e)
        {
            string taskText = NewTaskEntry.Text;
            if (!string.IsNullOrWhiteSpace(taskText))
            {
                DisplayAlert("Успех!", $"Задача '{taskText}' добавлена!", "OK");
                NewTaskEntry.Text = string.Empty;
            }
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }
    }
}
