using MauiApp1.Models;
using MauiApp1.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MauiApp1.ViewModels
{
    public class TasksViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private TaskList _selectedList;
        private bool _isSidePanelVisible = true;
        private double _sidePanelWidth = 200;

        public ObservableCollection<TaskList> TaskLists { get; } = new();
        public ObservableCollection<TaskItem> Tasks { get; } = new();

        public TasksViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            // Команды для панели
            ToggleSidePanelCommand = new Command(ToggleSidePanel);
            ShowAddListModalCommand = new Command(async () => await ShowAddListModal());
            ShowAddTaskModalCommand = new Command(async () => await ShowAddTaskModal());

            // Команды для задач
            DeleteTaskCommand = new Command<TaskItem>(async (task) => await DeleteTask(task));
            ToggleTaskCommand = new Command<TaskItem>(async (task) => await ToggleTask(task));

            // Загружаем данные
            LoadTaskLists();
        }

        // СВОЙСТВА (связываются с XAML)
        public bool IsSidePanelVisible
        {
            get => _isSidePanelVisible;
            set => SetProperty(ref _isSidePanelVisible, value);
        }

        public double SidePanelWidth
        {
            get => _sidePanelWidth;
            set => SetProperty(ref _sidePanelWidth, value);
        }

        public TaskList SelectedList
        {
            get => _selectedList;
            set
            {
                if (SetProperty(ref _selectedList, value) && value != null)
                {
                    LoadTasks(value.Id);
                }
            }
        }

        // КОМАНДЫ (связываются с XAML через Command="{Binding ...}")
        public ICommand ToggleSidePanelCommand { get; }
        public ICommand ShowAddListModalCommand { get; }
        public ICommand ShowAddTaskModalCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand ToggleTaskCommand { get; }

        // МЕТОД ДЛЯ ПАНЕЛИ (без сложных анимаций)
        private void ToggleSidePanel()
        {
            IsSidePanelVisible = !IsSidePanelVisible;
            SidePanelWidth = IsSidePanelVisible ? 200 : 0;
        }

        // МОДАЛЬНЫЕ ОКНА
        private async Task ShowAddListModal()
        {
            string listName = await Application.Current.MainPage.DisplayPromptAsync(
                "Новый список",
                "Введите название списка:",
                "Создать",
                "Отмена",
                "Например: Работа");

            if (!string.IsNullOrWhiteSpace(listName))
            {
                await AddNewList(listName);
            }
        }

        private async Task ShowAddTaskModal()
        {
            if (SelectedList == null)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    "Сначала выберите список", "OK");
                return;
            }

            string taskTitle = await Application.Current.MainPage.DisplayPromptAsync(
                "Новая задача",
                "Введите название задачи:",
                "Добавить",
                "Отмена",
                "Например: Купить молоко");

            if (!string.IsNullOrWhiteSpace(taskTitle))
            {
                string taskDescription = await Application.Current.MainPage.DisplayPromptAsync(
                    "Описание задачи",
                    "Введите описание (необязательно):",
                    "Добавить",
                    "Пропустить",
                    "Например: Молоко 2,5%");

                await AddNewTask(taskTitle, taskDescription);
            }
        }

        // РАБОТА С ДАННЫМИ
        private async void LoadTaskLists()
        {
            try
            {
                var lists = await _databaseService.GetTaskListsAsync();
                TaskLists.Clear();

                foreach (var list in lists)
                {
                    // Загружаем количество задач для каждого списка
                    var tasks = await _databaseService.GetTasksByListIdAsync(list.Id);
                    list.TaskCount = tasks.Count; // Заполняем TaskCount
                    TaskLists.Add(list);
                }

                // Выбираем первый список если есть
                if (lists.Any() && SelectedList == null)
                {
                    SelectedList = lists.First();
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось загрузить списки: {ex.Message}", "OK");
            }
        }

        private async void LoadTasks(int listId)
        {
            try
            {
                var tasks = await _databaseService.GetTasksByListIdAsync(listId);
                Tasks.Clear();

                // Сначала невыполненные, потом выполненные
                foreach (var task in tasks.OrderBy(t => t.IsCompleted).ThenBy(t => t.CreatedDate))
                {
                    Tasks.Add(task);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось загрузить задачи: {ex.Message}", "OK");
            }
        }

        private async Task AddNewList(string name)
        {
            try
            {
                var newList = new TaskList
                {
                    Name = name.Trim(),
                    Description = "Новый список задач",
                    Color = GetRandomColor(),
                    TaskCount = 0
                };

                await _databaseService.SaveTaskListAsync(newList);
                TaskLists.Add(newList);
                SelectedList = newList;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось создать список: {ex.Message}", "OK");
            }
        }

        private async Task AddNewTask(string title, string description = null)
        {
            try
            {
                var newTask = new TaskItem
                {
                    Title = title.Trim(),
                    Description = description?.Trim(),
                    TaskListId = SelectedList.Id,
                    CreatedDate = DateTime.Now
                };

                await _databaseService.SaveTaskAsync(newTask);

                // Увеличиваем счетчик задач в списке
                SelectedList.TaskCount++;

                // Обновляем отображение списка
                var index = TaskLists.IndexOf(SelectedList);
                TaskLists[index] = SelectedList; // Запустит обновление

                // Добавляем задачу в список
                Tasks.Add(newTask);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось добавить задачу: {ex.Message}", "OK");
            }
        }

        private async Task DeleteTask(TaskItem task)
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Удаление",
                $"Удалить задачу '{task.Title}'?",
                "Да", "Нет");

            if (confirm)
            {
                try
                {
                    await _databaseService.DeleteTaskAsync(task);

                    // Уменьшаем счетчик задач в списке
                    if (SelectedList != null)
                    {
                        SelectedList.TaskCount--;
                        var index = TaskLists.IndexOf(SelectedList);
                        TaskLists[index] = SelectedList;
                    }

                    Tasks.Remove(task);
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка",
                        $"Не удалось удалить задачу: {ex.Message}", "OK");
                }
            }
        }

        private async Task ToggleTask(TaskItem task)
        {
            try
            {
                task.IsCompleted = !task.IsCompleted;
                await _databaseService.SaveTaskAsync(task);

                // Обновляем отображение
                var index = Tasks.IndexOf(task);
                Tasks.RemoveAt(index);

                if (task.IsCompleted)
                    Tasks.Add(task); // Выполненные - в конец
                else
                    Tasks.Insert(0, task); // Невыполненные - в начало
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось обновить задачу: {ex.Message}", "OK");
            }
        }

        private string GetRandomColor()
        {
            var colors = new[]
            {
                "#FF6B6B", "#4ECDC4", "#45B7D1", "#96CEB4", "#FFEAA7",
                "#DDA0DD", "#98D8C8", "#F7DC6F", "#BB8FCE", "#85C1E9"
            };
            return colors[new Random().Next(colors.Length)];
        }
    }
}