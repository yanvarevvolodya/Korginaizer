using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MauiApp1.Models
{
    public class CalendarDay : INotifyPropertyChanged
    {
        private DateTime _date;
        private bool _isCurrentMonth;
        private bool _isToday;
        private bool _isSelected;
        private int _eventCount;

        public DateTime Date
        {
            get => _date;
            set => SetField(ref _date, value);
        }

        public bool IsCurrentMonth
        {
            get => _isCurrentMonth;
            set => SetField(ref _isCurrentMonth, value);
        }

        public bool IsToday
        {
            get => _isToday;
            set => SetField(ref _isToday, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetField(ref _isSelected, value))
                {
                    OnPropertyChanged(nameof(TextColor));
                    OnPropertyChanged(nameof(BackgroundColor));
                    OnPropertyChanged(nameof(BorderColor));
                }
            }
        }

        public int EventCount
        {
            get => _eventCount;
            set => SetField(ref _eventCount, value);
        }

        public string DayNumber => Date.Day.ToString();
        public bool IsWeekend => Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday;
        public bool HasEvents => EventCount > 0;

        public Color TextColor
        {
            get
            {
                if (!IsCurrentMonth)
                    return Application.Current.RequestedTheme == AppTheme.Dark
                        ? Color.FromArgb("#666666")
                        : Color.FromArgb("#AAAAAA");

                if (IsToday)
                    return Colors.White;

                if (IsWeekend)
                    return Color.FromArgb("#DC3545");

                return Application.Current.RequestedTheme == AppTheme.Dark
                    ? Colors.White
                    : Colors.Black;
            }
        }

        public Color BackgroundColor
        {
            get
            {
                if (IsToday)
                    return Color.FromArgb("#007ACC");

                if (IsSelected)
                    return Application.Current.RequestedTheme == AppTheme.Dark
                        ? Color.FromArgb("#2A2A2A")     
                        : Color.FromArgb("#E3F2FD");    

                return Colors.Transparent;
            }
        }

        public Color BorderColor => IsSelected ? Color.FromArgb("#007ACC") : Colors.Transparent;

        public Color EventIndicatorColor
        {
            get
            {
                if (EventCount == 0) return Colors.Transparent;
                if (EventCount <= 2) return Color.FromArgb("#28A745");
                if (EventCount <= 5) return Color.FromArgb("#FFC107");
                return Color.FromArgb("#DC3545");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}