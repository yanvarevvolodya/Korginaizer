using MauiApp1.Models;
using MauiApp1.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MauiApp1.ViewModels
{
    class TodayViewModel : BindableObject
    {
        private readonly DatabaseService _databaseService;
        private DateTime _selectedDate;
        private ObservableCollection<TaskItem> _todaysTasks;
        private ObservableCollection<CalendarEvent> _todayEvents;
        private bool _hasTasks, _noTasks, _hasEvents, _noEvents;

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged();
                    LoadDataForDate();
                }
            }
        }

        public ObservableCollection<TaskItem> TodayTasks
        {
            get => _todaysTasks;
            set { _todaysTasks = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CalendarEvent> TodayEvents { get; set; }

        public bool HasTasks { get => _hasTasks; set { _hasTasks = value; OnPropertyChanged(); } }
        public bool NoTasks { get => _noTasks; set { _noTasks = value; OnPropertyChanged(); } }
        public bool HasEvents { get => _hasEvents; set { _hasEvents = value; OnPropertyChanged(); } }
        public bool NoEvents { get => _noEvents; set { _noEvents = value; OnPropertyChanged(); } }

        public ICommand LoadTasksCommand { get; }

        public TodayViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            TodayTasks = new ObservableCollection<TaskItem>();
            TodayEvents = new ObservableCollection<CalendarEvent>();
            SelectedDate = DateTime.Today;
            LoadTasksCommand = new Command(async () => await LoadTasks());

            LoadTasksCommand.Execute(null);
        }

        private async void LoadDataForDate()
        {
            var tasks = await _databaseService.GetTasksForDateAsync(SelectedDate);
            TodayTasks.Clear();
            foreach (var t in tasks) TodayTasks.Add(t);
            HasTasks = TodayTasks.Any();
            NoTasks = !HasTasks;

            var events = await _databaseService.GetEventsForDateAsync(SelectedDate);
            TodayEvents.Clear();
            foreach (var e in events) TodayEvents.Add(e);
            HasEvents = TodayEvents.Any();
            NoEvents = !HasEvents;
        }

        private async Task LoadTasks()
        {
            var tasks = await _databaseService.GetTasksForDateAsync(DateTime.Today);

            TodayTasks.Clear();
            foreach (var task in tasks)
                TodayTasks.Add(task);
        }
    }
}
