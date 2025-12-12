using MauiApp1.Models;
using MauiApp1.Services;
using MauiApp1.ViewModels;
using System.ComponentModel;

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
                    // Анимируем перемещение панели
                    await SidePanel.TranslateTo(-(250 - vm.SidePanelWidth), 0, 250, Easing.CubicOut);

                    // Синхронизируем видимость оверлея с шириной панели
                    // Оверлей становится видимым, когда панель начинает выезжать (ширина > 0)
                    if (vm.SidePanelWidth > 0 && !Overlay.IsVisible)
                    {
                        Overlay.IsVisible = true;
                        Overlay.InputTransparent = false; // Теперь он может перехватывать нажатия
                        await Overlay.FadeTo(0.4, 150); // Плавное появление с затемнением
                    }
                    else if (vm.SidePanelWidth == 0 && Overlay.IsVisible)
                    {
                        await Overlay.FadeTo(0, 150); // Плавное исчезновение
                        Overlay.IsVisible = false;
                        Overlay.InputTransparent = true; // Снова отключаем реакцию на нажатия
                    }
                }
                else if (e.PropertyName == nameof(TasksViewModel.IsSidePanelVisible))
                {
                    SidePanel.IsVisible = vm.IsSidePanelVisible;
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



    }
}