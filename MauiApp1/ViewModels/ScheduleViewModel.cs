using MauiApp1.Services;
using MauiApp1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;

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

        public ScheduleViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            GroupedTasks = new ObservableCollection<GroupedTasks>();
            RefreshCommand = new Command(async () => await LoadTasksAsync());
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
