using System.Diagnostics;
using MauiApp1.Models;
using MauiApp1.Services;
using MauiApp1.ViewModels;

namespace MauiApp1
{
    public partial class CalendarPage : ContentPage
    {
        private Grid _calendarGrid;
        private CalendarViewModel _viewModel;

        public CalendarPage()
        {
            InitializeComponent();

            _calendarGrid = this.FindByName<Grid>("CalendarGrid");

            var databaseService = new DatabaseService();
            _viewModel = new CalendarViewModel(databaseService);
            BindingContext = _viewModel;

            // Подписываемся на изменения
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CalendarViewModel.CalendarDays))
            {
                // Когда дни обновляются, пересоздаем сетку
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    CreateCalendarGrid();
                    UpdateDaySelection();
                });
            }
            else if (e.PropertyName == nameof(CalendarViewModel.SelectedDate))
            {
                // Когда меняется выбранная дата, обновляем выделение
                MainThread.BeginInvokeOnMainThread(UpdateDaySelection);
            }
        }

        private void CreateCalendarGrid()
        {
            if (_calendarGrid == null || _viewModel?.CalendarDays == null)
                return;

            _calendarGrid.Children.Clear();
            Debug.WriteLine($"Создание сетки с {_viewModel.CalendarDays.Count} днями");

            int dayIndex = 0;
            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    if (dayIndex < _viewModel.CalendarDays.Count)
                    {
                        var day = _viewModel.CalendarDays[dayIndex];
                        var dayView = CreateDayView(day);

                        Microsoft.Maui.Controls.Grid.SetRow(dayView, row); // Явно указываем пространство имен
                        Microsoft.Maui.Controls.Grid.SetColumn(dayView, col);
                        _calendarGrid.Children.Add(dayView);

                        dayIndex++;
                    }
                }
            }
        }

        private View CreateDayView(CalendarDay day)
        {
            var frame = new Frame
            {
                Padding = 8,
                Margin = 1,
                BackgroundColor = day.BackgroundColor,
                BorderColor = day.BorderColor,
                CornerRadius = 8,
                HasShadow = false
            };

            var grid = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Auto }
                },
                RowSpacing = 4
            };

            // Номер дня
            var dayLabel = new Label
            {
                Text = day.DayNumber,
                FontSize = 16,
                FontAttributes = day.IsToday ? FontAttributes.Bold : FontAttributes.None,
                TextColor = day.TextColor,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            Microsoft.Maui.Controls.Grid.SetRow(dayLabel, 0);
            grid.Children.Add(dayLabel);

            // Индикатор событий
            if (day.HasEvents)
            {
                var dotsGrid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = GridLength.Auto }
                    },
                    ColumnSpacing = 2,
                    HorizontalOptions = LayoutOptions.Center
                };

                int dotCount = Math.Min(3, day.EventCount);
                for (int i = 0; i < dotCount; i++)
                {
                    var dot = new BoxView
                    {
                        WidthRequest = 4,
                        HeightRequest = 4,
                        Color = day.EventIndicatorColor,
                        CornerRadius = 2
                    };

                    Microsoft.Maui.Controls.Grid.SetColumn(dot, i);
                    dotsGrid.Children.Add(dot);
                }

                Microsoft.Maui.Controls.Grid.SetRow(dotsGrid, 1);
                grid.Children.Add(dotsGrid);
            }

            // Кнопка для выбора дня
            var selectButton = new Button
            {
                BackgroundColor = Colors.Transparent,
                BorderColor = Colors.Transparent,
                Command = _viewModel.SelectDayCommand,
                CommandParameter = day
            };

            Microsoft.Maui.Controls.Grid.SetRowSpan(selectButton, 2);
            grid.Children.Add(selectButton);

            frame.Content = grid;
            return frame;
        }

        private void UpdateDaySelection()
        {
            if (_viewModel?.CalendarDays == null || _calendarGrid?.Children == null)
                return;

            // Находим и обновляем все ячейки
            for (int i = 0; i < Math.Min(_viewModel.CalendarDays.Count, _calendarGrid.Children.Count); i++)
            {
                if (_calendarGrid.Children[i] is Frame frame &&
                    frame.Content is Grid grid &&
                    grid.Children[0] is Label dayLabel)
                {
                    var day = _viewModel.CalendarDays[i];

                    // Обновляем цвета
                    frame.BackgroundColor = day.BackgroundColor;
                    frame.BorderColor = day.BorderColor;
                    dayLabel.TextColor = day.TextColor;
                    dayLabel.FontAttributes = day.IsToday ? FontAttributes.Bold : FontAttributes.None;

                    // Обновляем индикаторы событий
                    UpdateEventIndicators(grid, day);
                }
            }
        }

        private void UpdateEventIndicators(Grid parentGrid, CalendarDay day)
        {
            // Удаляем старые индикаторы
            // Ищем Grid в первой строке (индикаторы событий)
            var oldIndicator = parentGrid.Children.FirstOrDefault(c =>
                c is Grid childGrid && Microsoft.Maui.Controls.Grid.GetRow(childGrid) == 1);

            if (oldIndicator != null)
                parentGrid.Children.Remove(oldIndicator);

            // Добавляем новые индикаторы если есть события
            if (day.HasEvents)
            {
                var dotsGrid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = GridLength.Auto }
                    },
                    ColumnSpacing = 2,
                    HorizontalOptions = LayoutOptions.Center
                };

                int dotCount = Math.Min(3, day.EventCount);
                for (int i = 0; i < dotCount; i++)
                {
                    var dot = new BoxView
                    {
                        WidthRequest = 4,
                        HeightRequest = 4,
                        Color = day.EventIndicatorColor,
                        CornerRadius = 2
                    };

                    Microsoft.Maui.Controls.Grid.SetColumn(dot, i);
                    dotsGrid.Children.Add(dot);
                }

                Microsoft.Maui.Controls.Grid.SetRow(dotsGrid, 1);
                parentGrid.Children.Add(dotsGrid);
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_viewModel != null)
            {
                await _viewModel.InitializeCalendarAsync();
                CreateCalendarGrid();

                //var db = new DatabaseService();
                //await db.DebugCalendarEventsAsync();
            }
        }
    }
}