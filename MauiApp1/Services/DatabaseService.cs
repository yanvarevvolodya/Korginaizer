using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using MauiApp1.Models;
using System.Diagnostics;

namespace MauiApp1.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;

        private string _databasePath;

        public DatabaseService()
        {
            InitializeDatabase();
        }

        public string GetDatabasePath()
        {
            if (string.IsNullOrEmpty(_databasePath))
            {
                _databasePath = Path.Combine(FileSystem.AppDataDirectory, "tasks.db3");
            }
            return _databasePath;
        }

        private async void InitializeDatabase()
        {
            try
            {
                if (_database == null)
                {
                    string dbPath = Path.Combine(FileSystem.AppDataDirectory, "tasks.db3");
                    _database = new SQLiteAsyncConnection(dbPath);

                    // Создаем таблицы
                    await _database.CreateTableAsync<TaskList>();
                    await _database.CreateTableAsync<TaskItem>();

                    // Добавляем тестовые данные, если база пустая
                    await SeedDataAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка инициализации БД: {ex.Message}");
            }
        }

        private async Task SeedDataAsync()
        {
            var lists = await GetTaskListsAsync();
            if (!lists.Any())
            {
                // Создаем несколько списков по умолчанию
                var defaultLists = new[]
                {
                    new TaskList { Name = "Личные", Description = "Личные задачи", Color = "#FF6B6B" },
                    new TaskList { Name = "Работа", Description = "Рабочие задачи", Color = "#4ECDC4" },
                    new TaskList { Name = "Покупки", Description = "Список покупок", Color = "#45B7D1" }
                };

                foreach (var list in defaultLists)
                {
                    await SaveTaskListAsync(list);
                }
            }
        }

        // Методы для TaskList
        public async Task<List<TaskList>> GetTaskListsAsync()
        {
            try
            {
                return await _database.Table<TaskList>().ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка получения списков: {ex.Message}");
                return new List<TaskList>();
            }
        }

        public async Task<int> SaveTaskListAsync(TaskList taskList)
        {
            try
            {
                if (taskList.Id != 0)
                    return await _database.UpdateAsync(taskList);
                else
                    return await _database.InsertAsync(taskList);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка сохранения списка: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> DeleteTaskListAsync(TaskList taskList)
        {
            try
            {
                // Сначала удаляем все задачи этого списка
                var tasks = await GetTasksByListIdAsync(taskList.Id);
                foreach (var task in tasks)
                {
                    await _database.DeleteAsync(task);
                }

                return await _database.DeleteAsync(taskList);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка удаления списка: {ex.Message}");
                return 0;
            }
        }

        // Методы для TaskItem
        public async Task<List<TaskItem>> GetTasksByListIdAsync(int listId)
        {
            try
            {
                return await _database.Table<TaskItem>()
                    .Where(t => t.TaskListId == listId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка получения задач: {ex.Message}");
                return new List<TaskItem>();
            }
        }

        public async Task<int> SaveTaskAsync(TaskItem task)
        {
            try
            {
                if (task.Id != 0)
                    return await _database.UpdateAsync(task);
                else
                    return await _database.InsertAsync(task);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка сохранения задачи: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> DeleteTaskAsync(TaskItem task)
        {
            try
            {
                return await _database.DeleteAsync(task);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка удаления задачи: {ex.Message}");
                return 0;
            }
        }
    }
}
