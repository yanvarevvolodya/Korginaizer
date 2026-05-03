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

            // Новая команда для меню списка
            ShowListMenuCommand = new Command<TaskList>(async (list) => await ShowListMenuAsync(list));
            SetDueDateCommand = new Command<TaskItem>(async (task) => await SetDueDateForTaskAsync(task));
            ToggleTaskStatusCommand = new Command<TaskItem>(async (task) => await ToggleTaskStatusAsync(task));
            ShowTaskMenuCommand = new Command<TaskItem>(async (task) => await ShowTaskMenuAsync(task));

            // Загружаем данные
            LoadTaskLists();
        }

        // СВОЙСТВА (оставляем без изменений)
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
        public ICommand ShowListMenuCommand { get; }
        public ICommand SetDueDateCommand { get; }
        public ICommand ShowTaskMenuCommand { get; }

        public ICommand ToggleTaskStatusCommand { get; }

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
            if (task == null) return;

            // Формируем детальную информацию
            var details = new StringBuilder();
            details.AppendLine($"**{task.Title}**");
            details.AppendLine();

            if (!string.IsNullOrWhiteSpace(task.Description))
            {
                details.AppendLine($"Описание:");
                details.AppendLine($"{task.Description}");
                details.AppendLine();
            }

            details.AppendLine($"Дата создания:");
            details.AppendLine($"{task.CreatedDate:dd.MM.yyyy HH:mm}");
            details.AppendLine();

            if (task.DueDate.HasValue)
            {
                details.AppendLine($"Срок выполнения:");
                details.AppendLine($"{task.FormattedDueDateTime}");
                details.AppendLine();
            }

            details.AppendLine($"Статус:");
            details.AppendLine($"{(task.IsCompleted ? "✓ Выполнена" : "◯ В процессе")}");

            await Application.Current.MainPage.DisplayAlert(
                "Подробная информация о задаче",
                details.ToString(),
                "Закрыть");
        }

        // Метод для редактирования задачи
        private async Task EditTaskAsync(TaskItem task)
        {
            if (task == null) return;

            // 1. Редактирование названия
            var newTitle = await Application.Current.MainPage.DisplayPromptAsync(
                "Редактирование задачи",
                "Введите новое название:",
                "Сохранить",
                "Отмена",
                initialValue: task.Title,
                maxLength: 100);

            if (string.IsNullOrWhiteSpace(newTitle) || newTitle == task.Title)
                return;

            // 2. Редактирование описания
            var newDescription = await Application.Current.MainPage.DisplayPromptAsync(
                "Описание задачи",
                "Введите описание (оставьте пустым, чтобы удалить):",
                "Сохранить",
                "Отмена",
                initialValue: task.Description,
                maxLength: 500);

            // Сохраняем изменения
            try
            {
                task.Title = newTitle.Trim();
                task.Description = newDescription?.Trim() ?? "";

                await _databaseService.SaveTaskAsync(task);

                // Обновляем отображение
                var index = Tasks.IndexOf(task);
                if (index != -1)
                {
                    Tasks[index] = task;
                }

                await Application.Current.MainPage.DisplayAlert(
                    "Успешно",
                    "Задача обновлена",
                    "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка",
                    $"Не удалось обновить задачу: {ex.Message}",
                    "OK");
            }
        }

        // Вспомогательный метод для получения текста приоритета
        private string GetPriorityText(int priority)
        {
            return priority switch
            {
                1 => "Низкий",
                2 => "Средний",
                3 => "Высокий",
                _ => "Средний"
            };
        }

        private async Task ToggleTaskStatusAsync(TaskItem task)
        {
            if (task == null) return;

            try
            {
                // НЕ инвертируем статус! Он уже изменен через привязку
                // Просто сохраняем текущий статус в базу

                Debug.WriteLine($"Сохранение задачи: ID={task.Id}, IsCompleted={task.IsCompleted}");
                await _databaseService.SaveTaskAsync(task);

                // НЕ обновляем элемент в коллекции - он уже обновлен через привязку
                // Только пересортируем задачи
                await Task.Delay(100); // Даем время UI на обновление
                await ReorderTasks();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось обновить статус задачи: {ex.Message}", "OK");

                // Восстанавливаем исходный статус в UI
                task.IsCompleted = !task.IsCompleted;
            }
        }

        public async Task SaveTaskStatusAsync(TaskItem task)
        {
            try
            {
                await _databaseService.SaveTaskAsync(task);
                Debug.WriteLine($"Статус сохранен: ID={task.Id}, IsCompleted={task.IsCompleted}");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось сохранить статус: {ex.Message}", "OK");
            }
        }

        // Метод для пересортировки задач
        private async Task ReorderTasks()
        {
            var currentListId = SelectedList?.Id;
            if (currentListId.HasValue)
            {
                await Task.Delay(10); // Небольшая задержка для стабильности
                LoadTasks(currentListId.Value);
            }
        }

        private async Task SetDueDateForTaskAsync(TaskItem task)
        {
            if (task == null) return;

            // 1. Запрашиваем дату
            var currentDate = task.DueDate ?? DateTime.Today;
            var dateResult = await Application.Current.MainPage.DisplayPromptAsync(
                "Дата выполнения",
                "Введите дату (дд.мм.гггг):",
                "Далее",
                "Отмена",
                keyboard: Keyboard.Numeric,
                initialValue: currentDate.ToString("dd.MM.yyyy"));

            if (string.IsNullOrWhiteSpace(dateResult)) return;

            if (!DateTime.TryParseExact(dateResult, "dd.MM.yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime selectedDate))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    "Некорректный формат даты. Используйте дд.мм.гггг", "OK");
                return;
            }

            // 2. Запрашиваем время (опционально)
            var timeOptions = new[] { "Без времени", "Указать время", "Отмена" };
            var timeChoice = await Application.Current.MainPage.DisplayActionSheet(
                "Время выполнения",
                "Отмена",
                null,
                timeOptions);

            TimeSpan? selectedTime = null;

            if (timeChoice == "Указать время")
            {
                var currentTime = task.DueTime ?? new TimeSpan(12, 0, 0);
                var timeResult = await Application.Current.MainPage.DisplayPromptAsync(
                    "Время выполнения",
                    "Введите время (чч:мм):",
                    "Сохранить",
                    "Отмена",
                    keyboard: Keyboard.Numeric,
                    initialValue: currentTime.ToString(@"hh\:mm"));

                if (!string.IsNullOrWhiteSpace(timeResult))
                {
                    if (TimeSpan.TryParseExact(timeResult, @"hh\:mm",
                        System.Globalization.CultureInfo.InvariantCulture,
                        out TimeSpan parsedTime))
                    {
                        selectedTime = parsedTime;
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Ошибка",
                            "Некорректный формат времени. Используйте чч:мм", "OK");
                        return;
                    }
                }
            }
            else if (timeChoice == "Без времени")
            {
                selectedTime = null;
            }
            else
            {
                return; // Пользователь нажал Отмена
            }

            // 3. Сохраняем изменения
            try
            {
                task.SetDueDateTime(selectedDate, selectedTime);
                await _databaseService.SaveTaskAsync(task);

                // Обновляем отображение
                var index = Tasks.IndexOf(task);
                if (index != -1)
                {
                    Tasks[index] = task;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось установить дату: {ex.Message}", "OK");
            }
        }

        // МЕТОД ДЛЯ МЕНЮ СПИСКА
        private async Task ShowListMenuAsync(TaskList list)
        {
            if (list == null) return;

            // Закрываем боковую панель
            if (IsSidePanelVisible)
            {
                await ToggleSidePanelWithAnimation();
            }

            var action = await Application.Current.MainPage.DisplayActionSheet(
                $"Список: {list.Name}",
                "Отмена",
                null,
                "Переименовать",
                "Удалить");

            switch (action)
            {
                case "Переименовать":
                    await RenameListAsync(list);
                    break;
                case "Удалить":
                    await DeleteListAsync(list);
                    break;
            }
        }

        // ПЕРЕИМЕНОВАНИЕ СПИСКА
        private async Task RenameListAsync(TaskList list)
        {
            if (list == null) return;

            string newName = await Application.Current.MainPage.DisplayPromptAsync(
                "Переименование",
                "Введите новое название списка:",
                "Сохранить",
                "Отмена",
                list.Name,
                maxLength: 50);

            if (!string.IsNullOrWhiteSpace(newName) && newName != list.Name)
            {
                try
                {
                    list.Name = newName.Trim();
                    await _databaseService.SaveTaskListAsync(list);

                    // Обновляем коллекцию
                    var index = TaskLists.IndexOf(list);
                    if (index != -1)
                    {
                        TaskLists[index] = list;
                    }
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка",
                        $"Не удалось переименовать список: {ex.Message}", "OK");
                }
            }
        }

        // УДАЛЕНИЕ СПИСКА
        private async Task DeleteListAsync(TaskList list)
        {
            if (list == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Удаление списка",
                $"Вы уверены, что хотите удалить список '{list.Name}' и все задачи в нём?",
                "Да, удалить",
                "Отмена");

            if (!confirm) return;

            try
            {
                // Удаляем из базы данных
                await _databaseService.DeleteTaskListAsync(list);

                // Удаляем из коллекции
                TaskLists.Remove(list);

                // Если удаляем выбранный список
                if (SelectedList?.Id == list.Id)
                {
                    SelectedList = null;
                    Tasks.Clear();

                    // Выбираем следующий доступный список
                    if (TaskLists.Any())
                    {
                        SelectedList = TaskLists.First();
                    }
                }

                await Application.Current.MainPage.DisplayAlert("Успешно",
                    $"Список '{list.Name}' удалён", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось удалить список: {ex.Message}", "OK");
            }
        }

        // МОДАЛЬНОЕ ОКНО ДЛЯ СОЗДАНИЯ СПИСКА (немного улучшаем)
        private async Task ShowAddListModal()
        {
            // Закрываем панель после создания
            if (IsSidePanelVisible)
            {
                await ToggleSidePanelWithAnimation();
            }

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

        // ОСТАЛЬНЫЕ МЕТОДЫ (без изменений)
        private async Task ToggleSidePanelWithAnimation()
        {
            if (IsSidePanelVisible)
            {
                // Скрываем панель с анимацией
                await AnimateSidePanelWidth(250, 0, 250);
                IsSidePanelVisible = false;
            }
            else
            {
                // Показываем панель с анимацией
                IsSidePanelVisible = true;
                await AnimateSidePanelWidth(0, 250, 250);
            }
        }

        private async Task AnimateSidePanelWidth(double start, double end, uint length = 250)
        {
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

                // Сортируем: сначала невыполненные с ближайшей датой, потом без даты, потом выполненные
                var sortedTasks = tasks
                    .OrderBy(t => t.IsCompleted) // Сначала невыполненные
                    .ThenBy(t => t.DueDate.HasValue ? 0 : 1) // С датой - вперёд
                    .ThenBy(t => t.DueDate) // По возрастанию даты
                    .ThenBy(t => t.CreatedDate); // Если даты одинаковые - по времени создания

                foreach (var task in sortedTasks)
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
                    Color = "#3A2C6B"
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
                    Description = "",
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
    }
}