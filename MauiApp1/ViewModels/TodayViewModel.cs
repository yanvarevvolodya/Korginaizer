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
        private ObservableCollection<TaskItem> _tasksForDate;
        private bool _hasTasks, _noTasks;

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

        public ObservableCollection<TaskItem> TasksForDate
        {
            get => _tasksForDate;
            set { _tasksForDate = value; OnPropertyChanged(); }
        }

        public bool HasTasks { get => _hasTasks; set { _hasTasks = value; OnPropertyChanged(); } }
        public bool NoTasks { get => _noTasks; set { _noTasks = value; OnPropertyChanged(); } }

        public ICommand OpenScheduleCommand { get; }
        public ICommand PreviousDayCommand { get; }
        public ICommand NextDayCommand { get; }
        public TodayViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            TasksForDate = new ObservableCollection<TaskItem>();
            SelectedDate = DateTime.Today;
            OpenScheduleCommand = new Command(async () => await OpenSchedule());
            PreviousDayCommand = new Command(PreviousDay);
            NextDayCommand = new Command(NextDay);
            LoadDataForDate();
        }

        public async Task LoadDataForDate()
        {
            var tasks = await _databaseService.GetTasksForDateAsync(SelectedDate);
            var sorted = tasks
                .OrderBy(t => t.IsCompleted)
                .ThenBy(t => t.DueTime ?? TimeSpan.MaxValue)
                .ThenBy(t => t.CreatedDate)
                .ToList();

            TasksForDate.Clear();
            foreach (var t in sorted) 
                TasksForDate.Add(t);
            HasTasks = TasksForDate.Any();
            NoTasks = !HasTasks;
        }

        public async Task OpenSchedule()
        {
            await Shell.Current.GoToAsync("SchedulePage");
        }

        private void PreviousDay()
        {
            SelectedDate = SelectedDate.AddDays(-1);
        }

        private void NextDay()
        {
            SelectedDate = SelectedDate.AddDays(1);
        }
    }
}
