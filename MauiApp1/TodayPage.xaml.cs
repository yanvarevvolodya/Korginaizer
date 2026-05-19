using MauiApp1.Models;
using MauiApp1.Services;
using MauiApp1.ViewModels;

namespace MauiApp1;

public partial class TodayPage : ContentPage
{
	public TodayPage()
	{
		InitializeComponent();
		var dbService = new DatabaseService();
		BindingContext = new TodayViewModel(dbService);
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		var vm = BindingContext as TodayViewModel;
		if (vm != null)
		{
			_ = vm.LoadDataForDate();
        }
    }
}