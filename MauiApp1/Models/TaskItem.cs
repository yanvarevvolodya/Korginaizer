using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MauiApp1.Services;

namespace MauiApp1.Models
{
    public class TaskItem : INotifyPropertyChanged
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

        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set => SetProperty(ref _isCompleted, value);
        }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
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
        public int TaskListId { get; set; }
        public int Priority { get; set; } = 1;

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

        public int CategoryId { get; set; } = 0;

        [Ignore]
        public string CategoryName { get; set; } = "";

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, value))
                return false;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
