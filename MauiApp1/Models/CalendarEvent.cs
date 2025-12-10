using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

public class CalendarEvent
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Title { get; set; }
    public string Description { get; set; }

    // Явно указываем тип для SQLite
    [Column("EventDate")]
    public DateTime EventDate { get; set; }

    [Column("CreatedDate")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public string Color { get; set; } = "#007ACC";
    public int Priority { get; set; } = 1;

    // Время храним как строку для простоты
    public string StartTimeString { get; set; }
    public string EndTimeString { get; set; }
    public bool IsAllDay { get; set; } = true;

    [Ignore]
    public TimeSpan? StartTime
    {
        get => string.IsNullOrEmpty(StartTimeString) ? null : TimeSpan.Parse(StartTimeString);
        set => StartTimeString = value?.ToString(@"hh\:mm");
    }

    [Ignore]
    public TimeSpan? EndTime
    {
        get => string.IsNullOrEmpty(EndTimeString) ? null : TimeSpan.Parse(EndTimeString);
        set => EndTimeString = value?.ToString(@"hh\:mm");
    }

    [Ignore]
    public string TimeText
    {
        get
        {
            if (IsAllDay) return "Весь день";
            if (StartTime.HasValue && EndTime.HasValue)
                return $"{StartTime.Value:hh\\:mm} - {EndTime.Value:hh\\:mm}";
            if (StartTime.HasValue)
                return $"{StartTime.Value:hh\\:mm}";
            return string.Empty;
        }
    }

    [Ignore]
    public bool HasTime => !IsAllDay || StartTime.HasValue;
}
