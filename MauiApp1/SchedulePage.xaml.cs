using MauiApp1.Services;
using MauiApp1.ViewModels;

namespace MauiApp1;

public partial class SchedulePage : ContentPage
{
	public SchedulePage()
	{
		InitializeComponent();
		var dbService = new DatabaseService();
		BindingContext = new ScheduleViewModel(dbService);
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		(BindingContext as ScheduleViewModel)?.LoadTasksAsync();
    }
}