namespace MauiApp1;

public partial class CalendarPage : ContentPage
{
	public CalendarPage()
	{
		InitializeComponent();
	}

    private void OnTasksButtonClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new TasksPage();
    }

    private void OnSettingsButtonClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new SettingsPage();
    }

    private void OnCalendarButtonClicked(object sender, EventArgs e)
    {
        // Уже в календаре
    }
}