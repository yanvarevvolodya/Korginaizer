using MauiApp1.Services;
using MauiApp1.ViewModels;

namespace MauiApp1
{
    public partial class KanbanPage : ContentPage
    {
        public KanbanPage()
        {
            try
            {
                InitializeComponent();

                var databaseService = new DatabaseService();
                var viewModel = new KanbanViewModel(databaseService);
                BindingContext = viewModel;
            }
            catch (Exception ex)
            {
                DisplayAlert("Ошибка", $"Не удалось загрузить страницу: {ex.Message}", "OK");
            }
        }
    }
}