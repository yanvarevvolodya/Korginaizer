namespace MauiApp1.Views
{
    public partial class BasePage : ContentPage
    {
        private Grid _sidePanel;

        public BasePage()
        {
            InitializeComponent();
            UpdatePlatformSpecificUI();
        }

        private void UpdatePlatformSpecificUI()
        {
            if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                // Для Windows - добавляем боковую панель
                AddSidePanelForWindows();
            }
            // Для Android оставляем как есть - без боковой панели
        }

        private void AddSidePanelForWindows()
        {
            // Перестраиваем MainGrid для Windows
            MainGrid.ColumnDefinitions.Clear();
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 70 }); // Боковая панель
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // Контент

            // Создаем боковую панель
            _sidePanel = new Grid
            {
                BackgroundColor = Color.FromArgb("#000033"),
                ZIndex = 999
            };

            var stackLayout = new StackLayout { Padding = new Thickness(0, 40, 0, 0) };

            var tasksButton = new Button
            {
                ImageSource = "tasks_icon.png",
                Text = "Задачи",
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.White,
                HeightRequest = 70
            };
            tasksButton.Clicked += OnTasksButtonClicked;

            var calendarButton = new Button
            {
                ImageSource = "calendar_icon.png",
                Text = "Календарь",
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.White,
                HeightRequest = 70
            };
            calendarButton.Clicked += OnCalendarButtonClicked;

            var settingsButton = new Button
            {
                ImageSource = "settings_icon.png",
                Text = "Настройки",
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.White,
                HeightRequest = 70
            };
            settingsButton.Clicked += OnSettingsButtonClicked;

            stackLayout.Children.Add(tasksButton);
            stackLayout.Children.Add(calendarButton);
            stackLayout.Children.Add(settingsButton);

            _sidePanel.Children.Add(stackLayout);

            // Добавляем боковую панель в Grid
            MainGrid.Children.Add(_sidePanel);
            Grid.SetColumn(_sidePanel, 0);
            Grid.SetRow(_sidePanel, 0);
            Grid.SetRowSpan(_sidePanel, 2);

            // Перемещаем существующие элементы в правильные колонки
            Grid.SetColumn(HeaderTitleLabel.Parent as Grid, 1);
            Grid.SetColumn(PageContentArea, 1);
        }

        // Обновленные методы для работы с кнопками боковой панели
        protected void SetActiveButton(string activePage)
        {
            if (_sidePanel == null) return; // На Android боковой панели нет

            // Находим кнопки в визуальном дереве
            var stackLayout = _sidePanel.Children[0] as StackLayout;
            if (stackLayout == null) return;

            var tasksButton = stackLayout.Children[0] as Button;
            var calendarButton = stackLayout.Children[1] as Button;
            var settingsButton = stackLayout.Children[2] as Button;

            if (tasksButton == null || calendarButton == null || settingsButton == null) return;

            // Сбрасываем все кнопки
            tasksButton.BackgroundColor = Colors.Transparent;
            calendarButton.BackgroundColor = Colors.Transparent;
            settingsButton.BackgroundColor = Colors.Transparent;

            // Подсвечиваем активную
            switch (activePage)
            {
                case "TasksPage":
                    tasksButton.BackgroundColor = Color.FromArgb("#1a4d80");
                    break;
                case "CalendarPage":
                    calendarButton.BackgroundColor = Color.FromArgb("#1a4d80");
                    break;
                case "SettingsPage":
                    settingsButton.BackgroundColor = Color.FromArgb("#1a4d80");
                    break;
            }
        }

        // Остальные методы без изменений...
        protected void OnTasksButtonClicked(object sender, EventArgs e)
        {
            if (!(this is TasksPage))
            {
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    Application.Current.MainPage = new TasksPage();
                }
                else
                {
                    Shell.Current.GoToAsync("//TasksPage");
                }
            }
        }

        protected void OnCalendarButtonClicked(object sender, EventArgs e)
        {
            if (!(this is CalendarPage))
            {
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    Application.Current.MainPage = new CalendarPage();
                }
                else
                {
                    Shell.Current.GoToAsync("//CalendarPage");
                }
            }
        }

        protected void OnSettingsButtonClicked(object sender, EventArgs e)
        {
            if (!(this is SettingsPage))
            {
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    Application.Current.MainPage = new SettingsPage();
                }
                else
                {
                    Shell.Current.GoToAsync("//SettingsPage");
                }
                
            }
        }

        // Методы для настройки страницы остаются без изменений
        protected void SetHeaderTitle(string title)
        {
            HeaderTitleLabel.Text = title;
        }

        protected void SetHeaderButton(View button)
        {
            HeaderButtonArea.Content = button;
        }

        protected void SetPageContent(View content)
        {
            PageContentArea.Content = content;
        }
    }
}