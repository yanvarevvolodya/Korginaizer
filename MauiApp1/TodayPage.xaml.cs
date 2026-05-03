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
}