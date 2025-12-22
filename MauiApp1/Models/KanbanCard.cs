using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1.Models
{
    public class KanbanCard : INotifyPropertyChanged
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public int ColumnId { get; set; }
        public int BoardId { get; set; }

        private int _position;
        public int Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        private string _color = "#4ECDC4";
        public string Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        // Добавляем свойства для даты и времени (аналогично TaskItem)
        private DateTime? _dueDate;
        public DateTime? DueDate
        {
            get => _dueDate;
            set => SetProperty(ref _dueDate, value);
        }

        private string _dueTimeString;
        public string DueTimeString
        {
            get => _dueTimeString;
            set => SetProperty(ref _dueTimeString, value);
        }

        [Ignore]
        public TimeSpan? DueTime
        {
            get => string.IsNullOrEmpty(DueTimeString) ? null : TimeSpan.Parse(DueTimeString);
            set => DueTimeString = value?.ToString(@"hh\:mm");
        }

        [Ignore]
        public string FormattedDueDateTime
        {
            get
            {
                if (!DueDate.HasValue) return null;

                var datePart = DueDate.Value.ToString("ddd, d MMMM yyyy",
                    new System.Globalization.CultureInfo("ru-RU"));

                if (DueTime.HasValue)
                {
                    return $"{datePart}, {DueTime.Value:hh\\:mm}";
                }
                return datePart;
            }
        }

        [Ignore]
        public bool HasDueDateTime => DueDate.HasValue;

        public void SetDueDateTime(DateTime? date, TimeSpan? time)
        {
            DueDate = date;
            DueTime = time;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, value))
                return false;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // Уведомляем об изменении вычисляемых свойств
            if (propertyName == nameof(DueDate) || propertyName == nameof(DueTimeString))
            {
                OnPropertyChanged(nameof(FormattedDueDateTime));
                OnPropertyChanged(nameof(HasDueDateTime));
            }

            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}