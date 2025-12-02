using MauiApp1.Models;
using MauiApp1.Services;
using MauiApp1.ViewModels;

namespace MauiApp1
{
    public partial class TasksPage : ContentPage
    {
        public TasksPage()
        {
            InitializeComponent();

            // Создаем ViewModel вручную
            var databaseService = new DatabaseService();
            var viewModel = new TasksViewModel(databaseService);
            BindingContext = viewModel;
        }

        public TasksPage(TasksViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        //private void OnEntryCompleted(object sender, EventArgs e)
        //{
        //    if (BindingContext is TasksViewModel viewModel)
        //    {
        //        viewModel.AddTaskCommand.Execute(null);
        //    }
        //}

        //private void OnDeleteTaskClicked(object sender, EventArgs e)
        //{
        //    if (sender is Button button && button.BindingContext is TaskItem task)
        //    {
        //        if (BindingContext is TasksViewModel viewModel)
        //        {
        //            viewModel.DeleteTaskCommand.Execute(task);
        //        }
        //    }
        //}

        private void OnTaskCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.BindingContext is TaskItem task)
            {
                if (BindingContext is TasksViewModel viewModel)
                {
                    viewModel.ToggleTaskCommand.Execute(task);
                }
            }
        }

        //private void OnShowPathClicked(object sender, EventArgs e)
        //{
        //    var path = new DatabaseService().GetDatabasePath();
        //    DisplayAlert("Путь к базе", $"База находится по пути:\n{path}", "OK");

        //    // Также выводим в консоль
        //    Console.WriteLine($"DB PATH: {path}");
        //}

    }
}