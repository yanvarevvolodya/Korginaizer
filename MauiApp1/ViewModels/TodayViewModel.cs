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

        public TodayViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            TasksForDate = new ObservableCollection<TaskItem>();
            SelectedDate = DateTime.Today;
        }

        public async Task LoadDataForDate()
        {
            var tasks = await _databaseService.GetTasksForDateAsync(SelectedDate);
            TasksForDate.Clear();
            foreach (var t in tasks) 
                TasksForDate.Add(t);
            HasTasks = TasksForDate.Any();
            NoTasks = !HasTasks;
        }
    }
}
