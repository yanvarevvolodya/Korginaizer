using MauiApp1.Models;
using MauiApp1.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;

namespace MauiApp1.ViewModels
{
    public class TasksViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly NotificationService _notificationService;
        private string _newTaskTitle;
        private ObservableCollection<TaskItem> _tasks;
        private ObservableCollection<TaskCategory> _taskCategories;
        private TaskCategory _selectedCategory;
        private List<TaskItem> _allTasks = new List<TaskItem>();
        private Dictionary<int, string> _categoryDict = new();

        public ObservableCollection<TaskItem> Tasks
        {
            get => _tasks;
            set => SetProperty(ref _tasks, value);
        }

        public ObservableCollection<TaskCategory> TaskCategories
        {
            get => _taskCategories;
            set { _taskCategories = value; OnPropertyChanged(); }
        }

        public TaskCategory SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    OnPropertyChanged();
                    FilterTasks();
                }
            }
        }
        public string NewTaskTitle
        {
            get => _newTaskTitle;
            set => SetProperty(ref _newTaskTitle, value);
        }

        public ICommand AddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand SetDueDateCommand { get; }
        public ICommand ShowTaskMenuCommand { get; }
        public ICommand ToggleTaskStatusCommand { get; }
        public ICommand AddCategoryCommand { get; }
        public ICommand ManageCategoriesCommand { get; }

        public TasksViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _notificationService = new NotificationService();
            Tasks = new ObservableCollection<TaskItem>();

            AddTaskCommand = new Command(async () => await AddTaskAsync());
            DeleteTaskCommand = new Command<TaskItem>(async (task) => await DeleteTaskAsync(task));
            SetDueDateCommand = new Command<TaskItem>(async (task) => await SetDueDateForTaskAsync(task));
            ShowTaskMenuCommand = new Command<TaskItem>(async (task) => await ShowTaskMenuAsync(task));
            ToggleTaskStatusCommand = new Command<TaskItem>(async (task) => await ToggleTaskStatusAsync(task));
            AddCategoryCommand = new Command(async () => await AddCategoryAsync());
            ManageCategoriesCommand = new Command(async () => await ManageCategoriesAsync());

            _ = LoadTasks();
            _ = LoadCategories();
        }
        public async Task LoadTasks()
        {
            var allTasks = await _databaseService.GetAllTasksAsync();

            var categories = await _databaseService.GetCategoriesAsync();
            _categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

            foreach (var task in allTasks)
            {
                task.CategoryName = _categoryDict.ContainsKey(task.CategoryId) ? _categoryDict[task.CategoryId] : "";
            }

            _allTasks = allTasks;
            FilterTasks();
        }

        private async Task LoadCategories()
        {
            var cats = await _databaseService.GetCategoriesAsync();
            var allCats = new List<TaskCategory> { new TaskCategory { Id = 0, Name = "Все категории" } };
            allCats.AddRange(cats);
            TaskCategories = new ObservableCollection<TaskCategory>(allCats);
            if (SelectedCategory == null)
                SelectedCategory = TaskCategories.FirstOrDefault();
        }
        private void FilterTasks()
        {
            if (_allTasks == null) return;

            IEnumerable<TaskItem> filteredTasks;
            if (SelectedCategory == null || SelectedCategory.Id == 0)
                filteredTasks = _allTasks;
            else
                filteredTasks = _allTasks.Where(t => t.CategoryId == SelectedCategory.Id);

            var sorted = filteredTasks
                .OrderBy(t => t.IsCompleted)
                .ThenBy(t => t.DueDate ?? DateTime.MaxValue)
                .ThenBy(t => t.CreatedDate)
                .ThenBy(t => t.CategoryName);

            Tasks.Clear();
            foreach (var task in sorted)
                Tasks.Add(task);
        }
        private async Task AddTaskAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle))
                return;

            var newTask = new TaskItem
            {
                Title = NewTaskTitle.Trim(),
                Description = "",
                TaskListId = await _databaseService.GetOrCreateDefaultListIdAsync(), // создаст или вернёт id дефолтного списка
                CreatedDate = DateTime.Now,
                CategoryId = 0
            };

            await _databaseService.SaveTaskAsync(newTask);
            NewTaskTitle = "";
            await LoadTasks();
        }

        private async Task AddCategoryAsync()
        {
            string categoryName = await Application.Current.MainPage.DisplayPromptAsync(
                "Новая категория",
                "Введите название категории:",
                "Создать",
                "Отмена");

            if (string.IsNullOrWhiteSpace(categoryName))
                return;

            var newCategory = new TaskCategory { Name = categoryName.Trim() };
            await _databaseService.SaveCategoryAsync(newCategory);
            await LoadCategories();
            await LoadTasks();
        }

        private async Task ManageCategoriesAsync()
        {
            var categories = await _databaseService.GetCategoriesAsync();
            if (!categories.Any())
            {
                await Application.Current.MainPage.DisplayAlert("Нет категорий", "Создайте категорию через кнопку '+'", "OK");
                return;
            }

            var categoryNames = categories.Select(c => c.Name).ToArray();
            var selectedName = await Application.Current.MainPage.DisplayActionSheet(
                "Управление категориями",
                "Отмена",
                null,
                categoryNames);

            if (string.IsNullOrEmpty(selectedName) || selectedName == "Отмена")
                return;

            var selectedCat = categories.FirstOrDefault(c => c.Name == selectedName);
            if (selectedCat == null) return;

            var action = await Application.Current.MainPage.DisplayActionSheet(
                $"Категория: {selectedCat.Name}",
                "Отмена",
                null,
                "Переименовать",
                "Удалить");

            switch (action)
            {
                case "Переименовать":
                    await RenameCategoryAsync(selectedCat);
                    break;
                case "Удалить":
                    await DeleteCategoryAsync(selectedCat);
                    break;
            }
        }

        private async Task RenameCategoryAsync(TaskCategory category)
        {
            var newName = await Application.Current.MainPage.DisplayPromptAsync(
                "Переименование",
                "Новое название категории:",
                "Сохранить",
                "Отмена",
                initialValue: category.Name);
            if (string.IsNullOrEmpty(newName) || newName == category.Name)
                return;

            category.Name = newName.Trim();
            await _databaseService.SaveCategoryAsync(category);

            await LoadCategories();
            await LoadTasks();
        }

        private async Task DeleteCategoryAsync(TaskCategory category)
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Удаление категории",
                $"Удалить категорию '{category.Name}'? Задачи останутся, но будут без категории.",
                "Да",
                "Нет");
            if (!confirm) return;

            await _databaseService.DeleteCategoryAsync(category);
            await LoadCategories();
            await LoadTasks();
        }

        private async Task DeleteTaskAsync(TaskItem task)
        {
            if (task == null) return;
            bool confirm = await Application.Current.MainPage.DisplayAlert("Удаление", $"Удалить задачу '{task.Title}'?", "Да", "Нет");
            if (!confirm) return;
            await _databaseService.DeleteTaskAsync(task);
            LoadTasks();
        }

        private async Task SetDueDateForTaskAsync(TaskItem task)
        {
            if (task == null) return;

            var currentDate = task.DueDate ?? DateTime.Today;
            var dateResult = await Application.Current.MainPage.DisplayPromptAsync(
                "Дата выполнения",
                "Введите дату (дд.мм.гггг):",
                "Далее",
                "Отмена",
                initialValue: currentDate.ToString("dd.MM.yyyy"));

            if (string.IsNullOrWhiteSpace(dateResult)) return;

            if (!DateTime.TryParseExact(dateResult, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime selectedDate))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Некорректный формат даты.", "OK");
                return;
            }

            var currentTime = task.DueTime ?? new TimeSpan(12, 0, 0);
            var timeResult = await Application.Current.MainPage.DisplayPromptAsync(
                "Время выполнения",
                "Введите время (чч:мм):",
                "Сохранить",
                "Отмена",
                initialValue: currentTime.ToString(@"hh\:mm"));

            if (string.IsNullOrWhiteSpace(timeResult))
                return;

            if (!TimeSpan.TryParseExact(timeResult, @"hh\:mm", null, out TimeSpan selectedTime))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Некорректный формат времени. Используйте hh:mm", "OK");
                return;
            }

            task.SetDueDateTime(selectedDate, selectedTime);
            await _databaseService.SaveTaskAsync(task);
            _notificationService.CancelNotification(task.Id);
            var notifyDateTime = selectedDate.Date + selectedTime;
            //var notifyDateTimeUtc = notifyDateTime.ToUniversalTime();
            Debug.WriteLine($"УВЕДОМЛЕНИЕ Вызов планирования: задача {task.Id}, время {notifyDateTime}");
            Debug.WriteLine($"Текущее время (локальное): {DateTime.Now}");
            Debug.WriteLine($"Текущее время (UTC): {DateTime.UtcNow}");
            Debug.WriteLine($"Запланированное время (локальное): {notifyDateTime}");
            _notificationService.ScheduleNotification(task.Id, task.Title, notifyDateTime);
            //Debug.WriteLine($"Запланированное время (UTC): {notifyDateTimeUtc}");
            //_notificationService.ScheduleNotification(task.Id, task.Title, notifyDateTimeUtc);
            //_notificationService.ScheduleNotification(task.Id, task.Title, DateTime.Now.AddSeconds(5));

            LoadTasks();
        }

        private async Task ShowTaskMenuAsync(TaskItem task)
        {
            if (task == null) return;
            var action = await Application.Current.MainPage.DisplayActionSheet(
                $"Задача: {task.Title}",
                "Отмена",
                null,
                "Подробности",
                "Редактировать",
                "Назначить категорию"
                );

            switch (action)
            {
                case "Подробности":
                    await ShowTaskDetailsAsync(task);
                    break;
                case "Редактировать":
                    await EditTaskAsync(task);
                    break;
                case "Удалить":
                    await DeleteTaskAsync(task);
                    break;
                case "Назначить категорию":
                    await AssignCategoryToTaskAsync(task);
                    break;
            }
        }
        private async Task AssignCategoryToTaskAsync(TaskItem task)
        {
            var categories = await _databaseService.GetCategoriesAsync();
            var categoryNames = new List<string> { "Без категории" };
            categoryNames.AddRange(categories.Select(c => c.Name));

            var selectedCategoryName = await Application.Current.MainPage.DisplayActionSheet(
                "Выберите категорию",
                "Отмена",
                null,
                categoryNames.ToArray());

            if (string.IsNullOrEmpty(selectedCategoryName) || selectedCategoryName == "Отмена")
                return;

            if (selectedCategoryName == "Без категории")
            {
                task.CategoryId = 0;
            }
            else
            {
                var selectedCat = categories.FirstOrDefault(c => c.Name == selectedCategoryName);
                task.CategoryId = selectedCat?.Id ?? 0;
            }

            await _databaseService.SaveTaskAsync(task);
            await LoadTasks();
        }

        private async Task ShowTaskDetailsAsync(TaskItem task)
        {
            var details = new StringBuilder();
            details.AppendLine(task.Title);
            if (!string.IsNullOrWhiteSpace(task.Description))
                details.AppendLine($"\nОписание: {task.Description}");
            if (task.DueDate.HasValue)
                details.AppendLine($"\nСрок: {task.FormattedDueDateTime}");
            //details.AppendLine($"\nСтатус: {(task.IsCompleted ? "Выполнена" : "Активна")}");
            details.AppendLine($"\nДата создания: {task.CreatedDate:dd.MM.yyyy HH:mm}");
            await Application.Current.MainPage.DisplayAlert("Подробности", details.ToString(), "Закрыть");
        }

        private async Task EditTaskAsync(TaskItem task)
        {
            var newTitle = await Application.Current.MainPage.DisplayPromptAsync("Редактирование", "Новое название:", "Сохранить", "Отмена", initialValue: task.Title);
            if (string.IsNullOrWhiteSpace(newTitle) || newTitle == task.Title) return;

            var newDescription = await Application.Current.MainPage.DisplayPromptAsync("Описание", "Введите описание:", "Сохранить", "Отмена", initialValue: task.Description);
            task.Title = newTitle.Trim();
            task.Description = newDescription?.Trim() ?? "";
            await _databaseService.SaveTaskAsync(task);
            await LoadTasks();
        }

        private async Task ToggleTaskStatusAsync(TaskItem task)
        {
            if (task == null) return;
            await _databaseService.SaveTaskAsync(task);
            // Перезагружаем для правильной сортировки
            //LoadTasks();
        }
    }
}