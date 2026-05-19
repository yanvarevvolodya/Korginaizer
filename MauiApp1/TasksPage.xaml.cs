using MauiApp1.Extensions;
using MauiApp1.Models;
using MauiApp1.Services;
using MauiApp1.ViewModels;
using System.ComponentModel;
using System.Diagnostics;

namespace MauiApp1
{
    public partial class TasksPage : ContentPage
    {
        public TasksPage()
        {
            InitializeComponent();
            var databaseService = new DatabaseService();
            var viewModel = new TasksViewModel(databaseService);
            BindingContext = viewModel;
        }

        private void OnEntryCompleted(object sender, EventArgs e)
        {
            if (BindingContext is TasksViewModel viewModel)
                viewModel.AddTaskCommand.Execute(null);
        }

        //private void OnDeleteTaskClicked(object sender, EventArgs e)
        //{
        //    if (sender is Button button && button.BindingContext is TaskItem task)
        //    {
        //        if (BindingContext is TasksViewModel viewModel)
        //            viewModel.DeleteTaskCommand.Execute(task);
        //    }
        //}

        private async void OnTaskCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.BindingContext is TaskItem task)
            {
                checkBox.CheckedChanged -= OnTaskCheckedChanged;
                try
                {
                    if (BindingContext is TasksViewModel viewModel)
                        await viewModel.ToggleTaskStatusCommand.ExecuteAsync(task);
                }
                finally
                {
                    checkBox.CheckedChanged += OnTaskCheckedChanged;
                }
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var vm = BindingContext as TasksViewModel;
            if (vm != null)
            {
                _ = vm.LoadTasks();
            }
        }
    }
}