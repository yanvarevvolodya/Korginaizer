using MauiApp1.Services;
using MauiApp1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Ical.Net;
using Ical.Net.CalendarComponents;

namespace MauiApp1.ViewModels
{
    public class ScheduleViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private ObservableCollection<GroupedTasks> _groupedTasks;

        public ObservableCollection<GroupedTasks> GroupedTasks
        {
            get => _groupedTasks;
            set => SetProperty(ref _groupedTasks, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand ExportToCalendarCommand { get; }

        public ScheduleViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            GroupedTasks = new ObservableCollection<GroupedTasks>();
            RefreshCommand = new Command(async () => await LoadTasksAsync());
            ExportToCalendarCommand = new Command(async () => await ExportToCalendarAsync());
            Task.Run(LoadTasksAsync);
        }

        public async Task LoadTasksAsync()
        {
            var allTasks = await _databaseService.GetAllTasksWithDueDateAsync();
            var grouped = allTasks
                .OrderBy(t => t.DueDate)
                .GroupBy(t => t.DueDate.Value.Date)
                .Select(g => new GroupedTasks(g.Key, g.OrderBy(t => t.DueTime ?? TimeSpan.Zero).ToList()));

            GroupedTasks.Clear();
            foreach (var group in grouped)
                GroupedTasks.Add(group);
        }

        private async Task ExportToCalendarAsync()
        {
            var tasks = await _databaseService.GetAllTasksWithDueDateAsync();
            if (!tasks.Any())
            {
                await Application.Current.MainPage.DisplayAlert("Нет данных", "Нет задач с датой для экспорта.", "OK");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:-//Korginaizer//RU");
            sb.AppendLine("CALSCALE:GREGORIAN");

            foreach (var task in tasks)
            {
                if (!task.DueDate.HasValue) continue;
                var dueDate = task.DueDate.Value;
                var dueTime = task.DueTime;

                DateTime start, end;
                bool isAllDay;
                if (dueTime.HasValue)
                {
                    start = dueDate.Date + dueTime.Value;
                    end = start.AddHours(1);
                    isAllDay = false;
                }
                else
                {
                    start = dueDate.Date;
                    end = dueDate.Date.AddDays(1).AddSeconds(-1);
                    isAllDay = true;
                }

                sb.AppendLine("BEGIN:VEVENT");
                sb.AppendLine($"UID:{Guid.NewGuid():N}");
                sb.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");
                if (isAllDay)
                {
                    sb.AppendLine($"DTSTART;VALUE=DATE:{start:yyyyMMdd}");
                    sb.AppendLine($"DTEND;VALUE=DATE:{end:yyyyMMdd}");
                }
                else
                {
                    var startUtc = start.ToUniversalTime();
                    var endUtc = end.ToUniversalTime();
                    sb.AppendLine($"DTSTART:{startUtc:yyyyMMddTHHmmssZ}");
                    sb.AppendLine($"DTEND:{endUtc:yyyyMMddTHHmmssZ}");
                }
                sb.AppendLine($"SUMMARY:{EscapeIcsText(task.Title)}");
                if (!string.IsNullOrWhiteSpace(task.Description))
                    sb.AppendLine($"DESCRIPTION:{EscapeIcsText(task.Description)}");
                if (!isAllDay)
                {
                    sb.AppendLine("BEGIN:VALARM");
                    sb.AppendLine("TRIGGER:-PT15M");
                    sb.AppendLine("ACTION:DISPLAY");
                    sb.AppendLine($"DESCRIPTION:Напоминание: {EscapeIcsText(task.Title)}");
                    sb.AppendLine("END:VALARM");
                }
                sb.AppendLine("END:VEVENT");
            }

            sb.AppendLine("END:VCALENDAR");

            var fileName = $"tasks_{DateTime.Now:yyyyMMdd_HHmmss}.ics";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            File.WriteAllText(filePath, sb.ToString());

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Экспорт задач в календарь",
                File = new ShareFile(filePath)
            });
        }

        private string EscapeIcsText(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Replace("\\", "\\\\")
                       .Replace(";", "\\;")
                       .Replace(",", "\\,")
                       .Replace("\n", "\\n");
        }
    }

    public class GroupedTasks : ObservableCollection<TaskItem>
    {
        public DateTime Date { get; }
        public string Key => Date.ToString("dddd, d MMMM yyyy");

        public GroupedTasks(DateTime date, IEnumerable<TaskItem> tasks) : base(tasks)
        {
            Date = date;
        }
    }
}
