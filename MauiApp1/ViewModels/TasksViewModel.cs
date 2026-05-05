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

        public ObservableCollection<TaskItem> Tasks
        {
            get => _tasks;
            set => SetProperty(ref _tasks, value);
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

            LoadTasks();
        }

        private async void LoadTasks()
        {
            var allTasks = await _databaseService.GetAllTasksAsync();
            Tasks.Clear();
            var sorted = allTasks
                .OrderBy(t => t.IsCompleted)
                .ThenBy(t => t.DueDate ?? DateTime.MaxValue)
                .ThenBy(t => t.CreatedDate);
            foreach (var t in sorted)
                Tasks.Add(t);
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
                CreatedDate = DateTime.Now
            };
            await _databaseService.SaveTaskAsync(newTask);
            NewTaskTitle = "";
            LoadTasks();
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
                "Удалить");

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
            }
        }

        private async Task ShowTaskDetailsAsync(TaskItem task)
        {
            var details = new StringBuilder();
            details.AppendLine(task.Title);
            if (!string.IsNullOrWhiteSpace(task.Description))
                details.AppendLine($"\nОписание: {task.Description}");
            if (task.DueDate.HasValue)
                details.AppendLine($"\nСрок: {task.FormattedDueDateTime}");
            details.AppendLine($"\nСтатус: {(task.IsCompleted ? "Выполнена" : "Активна")}");
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
            LoadTasks();
        }

        private async Task ToggleTaskStatusAsync(TaskItem task)
        {
            if (task == null) return;
            // Статус уже изменён в UI через привязку, просто сохраняем
            await _databaseService.SaveTaskAsync(task);
            // Перезагружаем для правильной сортировки
            //LoadTasks();
        }
    }
}