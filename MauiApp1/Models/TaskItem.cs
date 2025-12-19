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
            set
            {
                if (_isCompleted != value)
                {
                    _isCompleted = value;
                    OnPropertyChanged();

                    // Автоматически сохраняем в БД при изменении
                    SaveToDatabaseAsync();
                }
            }
        }

        private async void SaveToDatabaseAsync()
        {
            try
            {
                // Нужен доступ к DatabaseService
                // Используем DependencyService или статический метод
                await Task.Run(async () =>
                {
                    // Создаем временный DatabaseService
                    var databaseService = new DatabaseService();
                    await databaseService.SaveTaskAsync(this);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения задачи: {ex.Message}");
            }
        }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; }
        public int TaskListId { get; set; }
        public int Priority { get; set; } = 1;

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, value))
                return false;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
