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
        private bool _isSidePanelVisible = false;
        private double _sidePanelWidth = 0;
        private string _newTaskTitle;

        public ObservableCollection<TaskList> TaskLists { get; } = new();
        public ObservableCollection<TaskItem> Tasks { get; } = new();

        public TasksViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            // Команда для панели
            ToggleSidePanelCommand = new Command(async () => await ToggleSidePanelWithAnimation());
            ShowAddListModalCommand = new Command(async () => await ShowAddListModal());

            // Команды для задач
            AddTaskCommand = new Command(async () => await AddTaskAsync());
            DeleteTaskCommand = new Command<TaskItem>(async (task) => await DeleteTaskAsync(task));
            ToggleTaskCommand = new Command<TaskItem>(async (task) => await ToggleTaskAsync(task));

            // Загружаем данные
            LoadTaskLists();
        }

        // СВОЙСТВА
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

        public string NewTaskTitle
        {
            get => _newTaskTitle;
            set => SetProperty(ref _newTaskTitle, value);
        }

        // КОМАНДЫ
        public ICommand ToggleSidePanelCommand { get; }
        public ICommand ShowAddListModalCommand { get; }
        public ICommand AddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand ToggleTaskCommand { get; }

        // МЕТОД ДЛЯ ПАНЕЛИ
        private async Task ToggleSidePanelWithAnimation()
        {
            if (IsSidePanelVisible)
            {
                // Скрываем панель с анимацией
                await AnimateSidePanelWidth(250, 0, 250); // 250ms анимация
                IsSidePanelVisible = false;
            }
            else
            {
                // Показываем панель с анимацией
                IsSidePanelVisible = true;
                await AnimateSidePanelWidth(0, 250, 250); // 250ms анимация
            }
        }

        private async Task AnimateSidePanelWidth(double start, double end, uint length = 250)
        {
            // Плавно меняем ширину
            var animation = new Animation(
                callback: (value) => SidePanelWidth = value,
                start: start,
                end: end,
                easing: Easing.CubicOut
            );

            animation.Commit(
                owner: Application.Current.MainPage,
                name: "SidePanelAnimation",
                length: length,
                finished: (v, c) => SidePanelWidth = end
            );

            await Task.Delay((int)length);
        }

        // МОДАЛЬНОЕ ОКНО ДЛЯ СОЗДАНИЯ СПИСКА
        private async Task ShowAddListModal()
        {
            // Закрываем панель после создания
            await ToggleSidePanelWithAnimation();

            string listName = await Application.Current.MainPage.DisplayPromptAsync(
                "Новый список",
                "Введите название списка:",
                "Создать",
                "Отмена",
                "Например: Работа",
                maxLength: 50);

            if (!string.IsNullOrWhiteSpace(listName))
            {
                await AddNewList(listName);
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

                foreach (var task in tasks.OrderBy(t => t.IsCompleted))
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
                    Color = "#3A2C6B" // Фиксированный цвет
                };

                await _databaseService.SaveTaskListAsync(newList);
                TaskLists.Add(newList);
                SelectedList = newList; // Автоматически выбираем новый список
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось создать список: {ex.Message}", "OK");
            }
        }

        private async Task AddTaskAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle) || SelectedList == null)
            {
                await Application.Current.MainPage.DisplayAlert("Внимание",
                    "Сначала выберите список задач", "OK");
                return;
            }

            try
            {
                var newTask = new TaskItem
                {
                    Title = NewTaskTitle.Trim(),
                    TaskListId = SelectedList.Id,
                    CreatedDate = DateTime.Now
                };

                await _databaseService.SaveTaskAsync(newTask);
                NewTaskTitle = string.Empty;
                LoadTasks(SelectedList.Id);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось добавить задачу: {ex.Message}", "OK");
            }
        }

        private async Task DeleteTaskAsync(TaskItem task)
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
                    Tasks.Remove(task);
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка",
                        $"Не удалось удалить задачу: {ex.Message}", "OK");
                }
            }
        }

        private async Task ToggleTaskAsync(TaskItem task)
        {
            try
            {
                task.IsCompleted = !task.IsCompleted;
                await _databaseService.SaveTaskAsync(task);

                // Обновляем отображение
                var index = Tasks.IndexOf(task);
                Tasks.RemoveAt(index);
                Tasks.Insert(0, task); // Перемещаем в начало
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось обновить задачу: {ex.Message}", "OK");
            }
        }
    }
}