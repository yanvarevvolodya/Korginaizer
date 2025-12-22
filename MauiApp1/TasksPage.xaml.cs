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

            // Создаем ViewModel вручную
            var databaseService = new DatabaseService();
            var viewModel = new TasksViewModel(databaseService);
            BindingContext = viewModel;

            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private async void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (BindingContext is TasksViewModel vm)
            {
                if (e.PropertyName == nameof(TasksViewModel.SidePanelWidth))
                {
                    // Управляем видимостью панели
                    if (vm.SidePanelWidth > 0 && !SidePanel.IsVisible)
                    {
                        SidePanel.IsVisible = true;
                        SidePanel.TranslationX = -250; // Начинаем с полностью скрытого состояния
                        await SidePanel.TranslateTo(-(250 - vm.SidePanelWidth), 0, 250, Easing.CubicOut);
                    }
                    else if (vm.SidePanelWidth == 0 && SidePanel.IsVisible)
                    {
                        await SidePanel.TranslateTo(-250, 0, 250, Easing.CubicIn);
                        SidePanel.IsVisible = false;
                    }
                    else if (vm.SidePanelWidth > 0)
                    {
                        await SidePanel.TranslateTo(-(250 - vm.SidePanelWidth), 0, 250, Easing.CubicOut);
                    }

                    // Синхронизируем видимость оверлея
                    if (vm.SidePanelWidth > 0 && !Overlay.IsVisible)
                    {
                        Overlay.IsVisible = true;
                        Overlay.InputTransparent = false;
                        await Overlay.FadeTo(0.4, 150);
                    }
                    else if (vm.SidePanelWidth == 0 && Overlay.IsVisible)
                    {
                        await Overlay.FadeTo(0, 150);
                        Overlay.IsVisible = false;
                        Overlay.InputTransparent = true;
                    }
                }
                else if (e.PropertyName == nameof(TasksViewModel.IsSidePanelVisible))
                {
                    // Обработка прямой смены видимости
                    if (vm.IsSidePanelVisible && !SidePanel.IsVisible)
                    {
                        SidePanel.IsVisible = true;
                        SidePanel.TranslationX = -250;
                        await SidePanel.TranslateTo(0, 0, 250, Easing.CubicOut);
                    }
                    else if (!vm.IsSidePanelVisible && SidePanel.IsVisible)
                    {
                        await SidePanel.TranslateTo(-250, 0, 250, Easing.CubicIn);
                        SidePanel.IsVisible = false;
                    }
                }
            }
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            if (Handler != null)
            {
                // Добавляем обработчик касания к оверлею
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) =>
                {
                    if (BindingContext is TasksViewModel vm && vm.IsSidePanelVisible)
                    {
                        // Выполняем команду закрытия панели
                        vm.ToggleSidePanelCommand.Execute(null);
                    }
                };
                Overlay.GestureRecognizers.Add(tapGesture);
            }
        }

        public TasksPage(TasksViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void OnEntryCompleted(object sender, EventArgs e)
        {
            if (BindingContext is TasksViewModel viewModel)
            {
                viewModel.AddTaskCommand.Execute(null);
            }
        }

        private void OnDeleteTaskClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is TaskItem task)
            {
                if (BindingContext is TasksViewModel viewModel)
                {
                    viewModel.DeleteTaskCommand.Execute(task);
                }
            }
        }

        private async void OnTaskCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.BindingContext is TaskItem task)
            {
                // Временно отключаем обработку, чтобы избежать циклов
                checkBox.CheckedChanged -= OnTaskCheckedChanged;

                try
                {
                    Debug.WriteLine($"CheckBox изменен: ID={task.Id}, IsCompleted={task.IsCompleted}");

                    // Даем UI время на обновление
                    await Task.Delay(100);

                    if (BindingContext is TasksViewModel viewModel)
                    {
                        // Сохраняем задачу в базу
                        await viewModel.SaveTaskStatusAsync(task);
                    }
                }
                finally
                {
                    // Возвращаем обработчик
                    checkBox.CheckedChanged += OnTaskCheckedChanged;
                }
            }
        }
    }
}