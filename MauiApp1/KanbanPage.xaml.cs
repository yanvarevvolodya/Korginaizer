using MauiApp1.Services;
using MauiApp1.ViewModels;
using MauiApp1.Models;
using System.Diagnostics;
using MauiApp1.Extensions;

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