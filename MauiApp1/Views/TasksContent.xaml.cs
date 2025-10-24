namespace MauiApp1.Views;

public partial class TasksContent : ContentView
{
    private bool _isPanelVisible = true;
    public TasksContent()
	{
		InitializeComponent();
    }

    public void ToggleCategoriesPanel()
    {
        _isPanelVisible = !_isPanelVisible;

        if (_isPanelVisible)
        {
            // Показываем панель
            CategoriesPanel.IsVisible = true;
            Grid.SetColumn(TasksArea, 2);
            Grid.SetColumnSpan(TasksArea, 1);
        }
        else
        {
            // Скрываем панель
            CategoriesPanel.IsVisible = false;
            Grid.SetColumn(TasksArea, 1);
            Grid.SetColumnSpan(TasksArea, 2);
        }
    }
}