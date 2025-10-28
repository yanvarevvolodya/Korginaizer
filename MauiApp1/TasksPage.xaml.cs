namespace MauiApp1
{
    public partial class TasksPage : ContentPage
    {
        private bool _isCategoriesPanelVisible = true;
        private const string PanelStateKey = "CategoriesPanelState";

        public TasksPage()
        {
            InitializeComponent();

            // Загружаем сохраненное состояние
            _isCategoriesPanelVisible = Preferences.Get(PanelStateKey, true);
            ApplyInitialPanelState();
        }



        private void ApplyInitialPanelState()
        {
            if (CategoriesPanel == null || TasksContainer == null) return;

            if (_isCategoriesPanelVisible)
            {
                CategoriesPanel.TranslationX = 0;
                TasksContainer.TranslationX = 200;
                CategoriesPanel.IsVisible = true;
            }
            else
            {
                CategoriesPanel.TranslationX = -200;
                TasksContainer.TranslationX = 0;
                CategoriesPanel.IsVisible = false;
            }
        }

        private async void OnToggleCategoriesButtonClicked(object sender, EventArgs e)
        {
            if (CategoriesPanel == null || TasksContainer == null) return;

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
            CategoriesPanel.IsVisible = true;
            await Task.WhenAll(
                CategoriesPanel.TranslateTo(0, 0, 300, Easing.CubicOut),
                TasksContainer.TranslateTo(200, 0, 300, Easing.CubicOut)
            );
        }

        private async Task HideCategoriesPanel()
        {
            await Task.WhenAll(
                CategoriesPanel.TranslateTo(-200, 0, 300, Easing.CubicOut),
                TasksContainer.TranslateTo(0, 0, 300, Easing.CubicOut)
            );
            CategoriesPanel.IsVisible = false;
        }

        // Обработчики для кнопок навигации
        private void OnTasksButtonClicked(object sender, EventArgs e)
        {
            // Уже на задачах
        }

        private void OnCalendarButtonClicked(object sender, EventArgs e)
        {
            // Просто меняем главную страницу
            Application.Current.MainPage = new CalendarPage();
        }

        private void OnSettingsButtonClicked(object sender, EventArgs e)
        {
            Application.Current.MainPage = new SettingsPage();
        }


        private void LoadCalendarContent()
        {
            // Скрываем зеленую панель при переходе в календарь
            if (_isCategoriesPanelVisible)
            {
                _isCategoriesPanelVisible = false;
                ApplyInitialPanelState();
            }

            // Загружаем CalendarContent
            // MainContentArea.Content = new Views.CalendarContent();
        }

        private void LoadSettingsContent()
        {
            // Скрываем зеленую панель при переходе в настройки
            if (_isCategoriesPanelVisible)
            {
                _isCategoriesPanelVisible = false;
                ApplyInitialPanelState();
            }

            // Загружаем SettingsContent
            // MainContentArea.Content = new Views.SettingsContent();
        }
    }
}