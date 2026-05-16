using MauiApp1.Models;
using MauiApp1.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace MauiApp1.ViewModels
{
    public class CalendarViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly NotificationService _notificationService;
        private DateTime _currentMonth;
        private DateTime _selectedDate;
        private ObservableCollection<TaskItem> _eventsForSelectedDate;
        private ObservableCollection<CalendarDay> _calendarDays;
        private List<TaskItem> _monthEvents;

        public CalendarViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _notificationService = new NotificationService();

            _currentMonth = DateTime.Now;
            _selectedDate = DateTime.Now.Date;
            _eventsForSelectedDate = new ObservableCollection<TaskItem>();
            _calendarDays = new ObservableCollection<CalendarDay>();
            _monthEvents = new List<TaskItem>();

            PreviousMonthCommand = new Command(async () => await ChangeMonth(-1));
            NextMonthCommand = new Command(async () => await ChangeMonth(1));
            AddEventCommand = new Command(async () => await AddEventAsync());
            DeleteEventCommand = new Command<TaskItem>(async (e) => await DeleteEventAsync(e));
            SelectDateCommand = new Command<DateTime>((date) => SelectDate(date));
            SelectDayCommand = new Command<CalendarDay>((day) => SelectDay(day));

            // Инициализация
            Task.Run(async () =>
            {
                await LoadMonthEventsAsync();
                GenerateCalendarDays();
                await LoadEventsForSelectedDateAsync();
            });
        }

        // СВОЙСТВА
        public DateTime CurrentMonth
        {
            get => _currentMonth;
            private set => SetProperty(ref _currentMonth, value);
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            private set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    OnPropertyChanged(nameof(SelectedDateText));
                    _ = LoadEventsForSelectedDateAsync();
                }
            }
        }

        public ObservableCollection<TaskItem> EventsForSelectedDate
        {
            get => _eventsForSelectedDate;
            set => SetProperty(ref _eventsForSelectedDate, value);
        }

        public ObservableCollection<CalendarDay> CalendarDays
        {
            get => _calendarDays;
            set => SetProperty(ref _calendarDays, value);
        }

        public string CurrentMonthYear => CurrentMonth.ToString("MMMM yyyy");
        public string SelectedDateText => SelectedDate.ToString("dd MMMM yyyy");

        // КОМАНДЫ
        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }
        public ICommand AddEventCommand { get; }
        public ICommand DeleteEventCommand { get; }
        public ICommand SelectDateCommand { get; }
        public ICommand SelectDayCommand { get; }

        // МЕТОДЫ
        public async Task InitializeCalendarAsync()
        {
            await LoadMonthEventsAsync();
            GenerateCalendarDays();
            await LoadEventsForSelectedDateAsync();
        }

        public async Task LoadMonthEventsAsync()
        {
            try
            {
                _monthEvents = await _databaseService.GetTasksByMonthAsync(
                    CurrentMonth.Year, CurrentMonth.Month);
                Debug.WriteLine($"Загружено задач за {CurrentMonth:MMMM yyyy}: {_monthEvents.Count}");
                foreach (var task in _monthEvents)
                {
                    Debug.WriteLine($" - {task.Title} на {task.DueDate:dd.MM.yyyy}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка: {ex.Message}");
                _monthEvents = new List<TaskItem>();
            }
        }

        public void GenerateCalendarDays()
        {
            var newDays = new List<CalendarDay>();

            DateTime firstDayOfMonth = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            int firstDayWeekday = (int)firstDayOfMonth.DayOfWeek;
            firstDayWeekday = firstDayWeekday == 0 ? 6 : firstDayWeekday - 1;

            DateTime currentDisplayDate = firstDayOfMonth.AddDays(-firstDayWeekday);

            for (int i = 0; i < 42; i++)
            {
                var day = new CalendarDay
                {
                    Date = currentDisplayDate,
                    IsCurrentMonth = currentDisplayDate.Month == CurrentMonth.Month,
                    IsToday = currentDisplayDate.Date == DateTime.Now.Date,
                    IsSelected = currentDisplayDate.Date == SelectedDate.Date,
                    EventCount = CountEventsForDate(currentDisplayDate)
                };

                newDays.Add(day);
                currentDisplayDate = currentDisplayDate.AddDays(1);
            }

            CalendarDays.Clear();

            foreach (var day in newDays)
            {
                CalendarDays.Add(day);
            }

            Debug.WriteLine($"Сгенерировано {CalendarDays.Count} дней для {CurrentMonth:MMMM yyyy}");

            OnPropertyChanged(nameof(CalendarDays));
        }

        private int CountEventsForDate(DateTime date)
        {
            return _monthEvents?.Count(t => t.DueDate.HasValue && t.DueDate.Value.Date == date.Date) ?? 0;
        }

        private void SelectDay(CalendarDay day)
        {
            // Обновляем выделение
            foreach (var d in CalendarDays)
            {
                d.IsSelected = d.Date.Date == day.Date.Date;
            }

            // Обновляем выбранную дату
            SelectedDate = day.Date;

            Debug.WriteLine($"Выбрана дата: {SelectedDate}");
        }

        private async Task ChangeMonth(int delta)
        {
            CurrentMonth = CurrentMonth.AddMonths(delta);
            OnPropertyChanged(nameof(CurrentMonthYear));

            await LoadMonthEventsAsync();
            GenerateCalendarDays();
        }

        private void SelectDate(DateTime date)
        {
            SelectedDate = date;
        }

        private async Task LoadEventsForSelectedDateAsync()
        {
            try
            {
                EventsForSelectedDate.Clear();
                Debug.WriteLine($"Загрузка событий для: {SelectedDate:dd.MM.yyyy}");

                var events = await _databaseService.GetTasksForDateAsync(SelectedDate);
                Debug.WriteLine($"Найдено в БД: {events.Count} событий");

                foreach (var ev in events)
                {
                    Debug.WriteLine($"Добавляем: {ev.Title} ({ev.DueDate:dd.MM.yyyy})");
                    EventsForSelectedDate.Add(ev);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка загрузки событий: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось загрузить события: {ex.Message}", "OK");
            }
        }

        private async Task AddEventAsync()
        {
            DateTime eventDate = SelectedDate;

            string title = await Application.Current.MainPage.DisplayPromptAsync(
                "Новое событие",
                $"Введите название задачи для {SelectedDate:dd.MM.yyyy}:",
                "Добавить",
                "Отмена",
                "Например: Купить продукты");

            if (string.IsNullOrWhiteSpace(title))
                return;

            string time = await Application.Current.MainPage.DisplayPromptAsync(
                "Время события",
                "Введите время (например: чч:мм) или оставьте пустым:",
                "Добавить",
                "Без времени",
                "");

            TimeSpan? dueTime = null;

            if (!string.IsNullOrWhiteSpace(time) && TimeSpan.TryParse(time, out var parsedTime))
                dueTime = parsedTime;

            try
            {
                var newTask = new TaskItem
                {
                    Title = title.Trim(),
                    DueDate = eventDate,
                    DueTime = dueTime,
                    CreatedDate = DateTime.Now,
                    IsCompleted = false,
                    TaskListId = await _databaseService.GetOrCreateDefaultListIdAsync()
                };

                int result = await _databaseService.SaveTaskAsync(newTask);

                if (result > 0)
                {
                    if (newTask.DueDate.HasValue && newTask.DueTime.HasValue)
                    {
                        var notifyDateTime = newTask.DueDate.Value.Date + newTask.DueTime.Value;
                        _notificationService.CancelNotification(newTask.Id);
                        _notificationService.ScheduleNotification(newTask.Id, newTask.Title, notifyDateTime);
                    }

                    await LoadEventsForSelectedDateAsync();
                    await LoadMonthEventsAsync();
                    GenerateCalendarDays();
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    ex.Message, "OK");
            }
        }

        private async Task DeleteEventAsync(TaskItem task)
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Удаление",
                $"Удалить задачу '{task.Title}'?",
                "Да", "Нет");

            if (confirm)
            {
                try
                {
                    _notificationService.CancelNotification(task.Id);
                    await _databaseService.DeleteTaskAsync(task);
                    EventsForSelectedDate.Remove(task);
                    await LoadMonthEventsAsync();
                    GenerateCalendarDays();
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка",
                         ex.Message, "OK");
                }
            }
        }
    }
}