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
        private DateTime _currentMonth;
        private DateTime _selectedDate;
        private ObservableCollection<CalendarEvent> _eventsForSelectedDate;
        private ObservableCollection<CalendarDay> _calendarDays;
        private List<CalendarEvent> _monthEvents;

        public CalendarViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            _currentMonth = DateTime.Now;
            _selectedDate = DateTime.Now.Date;
            _eventsForSelectedDate = new ObservableCollection<CalendarEvent>();
            _calendarDays = new ObservableCollection<CalendarDay>();
            _monthEvents = new List<CalendarEvent>();

            PreviousMonthCommand = new Command(async () => await ChangeMonth(-1));
            NextMonthCommand = new Command(async () => await ChangeMonth(1));
            AddEventCommand = new Command(async () => await AddEventAsync());
            DeleteEventCommand = new Command<CalendarEvent>(async (e) => await DeleteEventAsync(e));
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

        public ObservableCollection<CalendarEvent> EventsForSelectedDate
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
                _monthEvents = await _databaseService.GetCalendarEventsByMonthAsync(
                    CurrentMonth.Year, CurrentMonth.Month);
                Debug.WriteLine($"Загружено {_monthEvents.Count} событий за месяц");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка загрузки событий месяца: {ex.Message}");
                _monthEvents = new List<CalendarEvent>();
            }
        }

        public void GenerateCalendarDays()
        {
            // Временная коллекция для новых дней
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

            // ВАЖНО: Полностью заменяем коллекцию
            CalendarDays.Clear();

            foreach (var day in newDays)
            {
                CalendarDays.Add(day);
            }

            Debug.WriteLine($"Сгенерировано {CalendarDays.Count} дней для {CurrentMonth:MMMM yyyy}");

            // Явно уведомляем об изменении коллекции
            OnPropertyChanged(nameof(CalendarDays));
        }

        private int CountEventsForDate(DateTime date)
        {
            return _monthEvents?.Count(e => e.EventDate.Date == date.Date) ?? 0;
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

                // Загружаем реальные события из БД
                var events = await _databaseService.GetCalendarEventsAsync(SelectedDate);
                Debug.WriteLine($"Найдено в БД: {events.Count} событий");

                foreach (var ev in events)
                {
                    Debug.WriteLine($"Добавляем: {ev.Title} ({ev.EventDate:dd.MM.yyyy})");
                    EventsForSelectedDate.Add(ev);
                }

                // НИКАКИХ ТЕСТОВЫХ ДАННЫХ!
                // Если событий нет - список будет пустым
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
            // Просто используем выбранную дату в календаре
            DateTime eventDate = SelectedDate;

            string title = await Application.Current.MainPage.DisplayPromptAsync(
                "Новое событие",
                $"Введите название события для {SelectedDate:dd.MM.yyyy}:",
                "Добавить",
                "Отмена",
                "Например: Встреча с клиентом");

            if (string.IsNullOrWhiteSpace(title))
                return;

            // Спрашиваем только время
            string time = await Application.Current.MainPage.DisplayPromptAsync(
                "Время события",
                "Введите время (например: 14:30) или оставьте пустым:",
                "Добавить",
                "Без времени",
                "");

            TimeSpan? startTime = null;
            DateTime finalEventDate = eventDate;

            if (!string.IsNullOrWhiteSpace(time) && TimeSpan.TryParse(time, out var parsedTime))
            {
                startTime = parsedTime;
                finalEventDate = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day,
                                             parsedTime.Hours, parsedTime.Minutes, 0);
            }

            try
            {
                var newEvent = new CalendarEvent
                {
                    Title = title.Trim(),
                    EventDate = finalEventDate,
                    StartTime = startTime,
                    CreatedDate = DateTime.Now,
                    Color = GetRandomColor(),
                    IsAllDay = string.IsNullOrWhiteSpace(time)
                };

                Debug.WriteLine($"Создаем событие: {newEvent.Title} на {newEvent.EventDate:dd.MM.yyyy HH:mm}");

                // Сохраняем в БД
                int result = await _databaseService.SaveCalendarEventAsync(newEvent);

                if (result > 0)
                {
                    // Обновляем UI
                    await LoadEventsForSelectedDateAsync();
                    await LoadMonthEventsAsync();
                    GenerateCalendarDays();

                    Debug.WriteLine($"Событие успешно добавлено на {finalEventDate:dd.MM.yyyy}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось добавить событие: {ex.Message}", "OK");
            }
        }

        private async Task DeleteEventAsync(CalendarEvent calendarEvent)
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Удаление",
                $"Удалить событие '{calendarEvent.Title}'?",
                "Да", "Нет");

            if (confirm)
            {
                try
                {
                    // Удаляем из БД
                    await _databaseService.DeleteCalendarEventAsync(calendarEvent);

                    // Удаляем из списка
                    EventsForSelectedDate.Remove(calendarEvent);

                    // Обновляем индикаторы на календаре
                    await LoadMonthEventsAsync();
                    GenerateCalendarDays();

                    // Обновляем UI
                    OnPropertyChanged(nameof(CalendarDays));
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка",
                        $"Не удалось удалить событие: {ex.Message}", "OK");
                }
            }
        }

        private string GetRandomColor()
        {
            var colors = new[]
            {
                "#007ACC", "#28A745", "#DC3545", "#FFC107",
                "#6F42C1", "#17A2B8", "#FD7E14"
            };
            return colors[new Random().Next(colors.Length)];
        }
    }
}