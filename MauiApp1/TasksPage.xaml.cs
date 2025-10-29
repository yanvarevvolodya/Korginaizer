using MauiApp1.Views;

namespace MauiApp1
{
    public partial class TasksPage : BasePage
    {
        private bool _isCategoriesPanelVisible = true;
        private const string PanelStateKey = "CategoriesPanelState";
        private Grid _categoriesPanel;
        private ScrollView _tasksContainer;
        private Grid _mainContentGrid;

        public TasksPage()
        {
            InitializeComponent();

            SetHeaderTitle("Задачи");
            SetActiveButton("TasksPage");

            var toggleButton = new Button
            {
                Text = "☰",
                BackgroundColor = Colors.Transparent,
                WidthRequest = 60,
                TextColor = Colors.White
            };
            toggleButton.Clicked += OnToggleCategoriesClicked;
            SetHeaderButton(toggleButton);

            SetPageContent(CreateTasksContent());

            _isCategoriesPanelVisible = Preferences.Get(PanelStateKey, true);
            ApplyInitialPanelState();
        }

        private View CreateTasksContent()
        {
            // Создаем зеленую панель
            _categoriesPanel = new Grid
            {
                BackgroundColor = Color.FromArgb("#000062"),
                TranslationX = -200,
                WidthRequest = 200
            };

            var stackLayout = new StackLayout
            {
                Spacing = 10,
                Padding = new Thickness(10, 40, 10, 0)
            };

            stackLayout.Children.Add(new Button
            {
                Text = "Создать список",
                BackgroundColor = Color.FromArgb("#000033"),
                TextColor = Colors.White
            });

            stackLayout.Children.Add(new Label
            {
                Text = "Мои списки:",
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold
            });

            stackLayout.Children.Add(new Button
            {
                Text = "Личные",
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.White
            });

            stackLayout.Children.Add(new Button
            {
                Text = "Работа",
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.White
            });

            stackLayout.Children.Add(new Button
            {
                Text = "Покупки",
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.White
            });

            _categoriesPanel.Children.Add(stackLayout);

            // Создаем контейнер задач
            _tasksContainer = new ScrollView();

            var tasksStackLayout = new StackLayout { Padding = 20 };

            tasksStackLayout.Children.Add(new Label
            {
                Text = "Список задач...",
                FontSize = 16,
                TextColor = Colors.White
            });

            tasksStackLayout.Children.Add(new Frame
            {
                BackgroundColor = Color.FromArgb("#f5f5f5"),
                Padding = 15,
                Content = new Label { Text = "Пример задачи 1" }
            });

            tasksStackLayout.Children.Add(new Frame
            {
                BackgroundColor = Color.FromArgb("#f5f5f5"),
                Padding = 15,
                Content = new Label { Text = "Пример задачи 2" }
            });

            _tasksContainer.Content = tasksStackLayout;

            // Создаем основной Grid
            _mainContentGrid = new Grid();

            // Добавляем колонки
            _mainContentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 200 });
            _mainContentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            // Добавляем строку
            _mainContentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

            // Добавляем элементы в Grid с указанием позиций
            _mainContentGrid.Children.Add(_categoriesPanel);
            Grid.SetColumn(_categoriesPanel, 0);
            Grid.SetRow(_categoriesPanel, 0);

            _mainContentGrid.Children.Add(_tasksContainer);
            Grid.SetColumn(_tasksContainer, 0);
            Grid.SetColumnSpan(_tasksContainer, 2);
            Grid.SetRow(_tasksContainer, 0);

            return _mainContentGrid;
        }

        private void ApplyInitialPanelState()
        {
            if (_categoriesPanel == null || _tasksContainer == null) return;

            if (_isCategoriesPanelVisible)
            {
                _categoriesPanel.TranslationX = 0;
                Grid.SetColumn(_tasksContainer, 1);
                Grid.SetColumnSpan(_tasksContainer, 1);
                _categoriesPanel.IsVisible = true;
            }
            else
            {
                _categoriesPanel.TranslationX = -200;
                Grid.SetColumn(_tasksContainer, 0);
                Grid.SetColumnSpan(_tasksContainer, 2);
                _categoriesPanel.IsVisible = false;
            }
        }

        private async void OnToggleCategoriesClicked(object sender, EventArgs e)
        {
            if (_categoriesPanel == null || _tasksContainer == null) return;

            _isCategoriesPanelVisible = !_isCategoriesPanelVisible;
            Preferences.Set(PanelStateKey, _isCategoriesPanelVisible);

            if (_isCategoriesPanelVisible)
            {
                await ShowCategoriesPanel();
            }
            else
            {
                await HideCategoriesPanel();
            }
        }

        private async Task ShowCategoriesPanel()
        {
            _categoriesPanel.IsVisible = true;

            // Анимация появления
            await Task.WhenAll(
                _categoriesPanel.TranslateTo(0, 0, 300, Easing.CubicOut),
                _tasksContainer.TranslateTo(200, 0, 300, Easing.CubicOut)
            );

            // Обновляем позицию в Grid
            Grid.SetColumn(_tasksContainer, 1);
            Grid.SetColumnSpan(_tasksContainer, 1);
            _tasksContainer.TranslationX = 0;
        }

        private async Task HideCategoriesPanel()
        {
            // Анимация скрытия
            await Task.WhenAll(
                _categoriesPanel.TranslateTo(-200, 0, 300, Easing.CubicOut),
                _tasksContainer.TranslateTo(0, 0, 300, Easing.CubicOut)
            );

            _categoriesPanel.IsVisible = false;

            // Обновляем позицию в Grid
            Grid.SetColumn(_tasksContainer, 0);
            Grid.SetColumnSpan(_tasksContainer, 2);
            _tasksContainer.TranslationX = 0;
        }
    }
}