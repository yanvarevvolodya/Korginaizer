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
                    await _database.CreateTableAsync<CalendarEvent>();
                    await _database.CreateTableAsync<TaskCategory>();

                    Debug.WriteLine($"Таблица CalendarEvent создана. Путь к БД: {dbPath}");

                    // Добавляем тестовые данные, если база пустая
                    await SeedDataAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка инициализации БД: {ex.Message}");
            }
        }

        // Test data methods
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
                //Debug.WriteLine($"=== СОХРАНЕНИЕ ЗАДАЧИ ===");
                //Debug.WriteLine($"ID: {task.Id}, Title: '{task.Title}'");
                //Debug.WriteLine($"IsCompleted до сохранения: {task.IsCompleted}");
                //Debug.WriteLine($"Stack trace: {Environment.StackTrace}");

                if (task.Id != 0)
                {
                    var result = await _database.UpdateAsync(task);
                    Debug.WriteLine($"Задача ОБНОВЛЕНА: ID={task.Id}, IsCompleted={task.IsCompleted}");
                    return result;
                }
                else
                {
                    var result = await _database.InsertAsync(task);
                    Debug.WriteLine($"Задача СОЗДАНА: ID={task.Id}, IsCompleted={task.IsCompleted}");
                    return result;
                }
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

        // TaskCategory
        public async Task<List<TaskCategory>> GetCategoriesAsync()
        {
            return await _database.Table<TaskCategory>().ToListAsync();
        }

        public async Task<int> SaveCategoryAsync(TaskCategory taskCategory)
        {
            if (taskCategory.Id != 0)
                return await _database.UpdateAsync(taskCategory);
            else
                return await _database.InsertAsync(taskCategory);
        }

        public async Task<int> DeleteCategoryAsync(TaskCategory taskCategory)
        {
            var tasks = await _database.Table<TaskItem>().Where(t => t.CategoryId == taskCategory.Id).ToListAsync();
            foreach (var task in tasks)
            {
                task.CategoryId = 0;
                await _database.UpdateAsync(task);
            }
            return await _database.DeleteAsync(taskCategory);
        }

        // Методы для CalendarEvent
        public async Task<List<CalendarEvent>> GetCalendarEventsAsync(DateTime date)
        {
            try
            {
                Debug.WriteLine($"Запрос событий на дату: {date:yyyy-MM-dd}");

                // Получаем ВСЕ события
                var allEvents = await _database.Table<CalendarEvent>().ToListAsync();
                Debug.WriteLine($"Всего событий в БД: {allEvents.Count}");

                // Фильтруем вручную
                var eventsForDate = new List<CalendarEvent>();
                foreach (var ev in allEvents)
                {
                    Debug.WriteLine($"Событие: ID={ev.Id}, Title='{ev.Title}', Date={ev.EventDate:yyyy-MM-dd}, Ticks={ev.EventDate.Ticks}");

                    // Сравниваем даты (только день, месяц, год)
                    if (ev.EventDate.Date == date.Date)
                    {
                        eventsForDate.Add(ev);
                        Debug.WriteLine($"  -> Подходит для {date:yyyy-MM-dd}");
                    }
                }

                Debug.WriteLine($"Найдено событий для {date:yyyy-MM-dd}: {eventsForDate.Count}");
                return eventsForDate;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка получения событий: {ex.Message}");
                return new List<CalendarEvent>();
            }
        }

        public async Task<List<CalendarEvent>> GetCalendarEventsByMonthAsync(int year, int month)
        {
            try
            {
                Debug.WriteLine($"Запрос событий за месяц: {year}-{month}");

                var allEvents = await _database.Table<CalendarEvent>().ToListAsync();
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                Debug.WriteLine($"Диапазон: {startDate:yyyy-MM-dd} - {endDate:yyyy-MM-dd}");

                var monthEvents = new List<CalendarEvent>();
                foreach (var ev in allEvents)
                {
                    if (ev.EventDate >= startDate && ev.EventDate <= endDate)
                    {
                        monthEvents.Add(ev);
                        Debug.WriteLine($"  Событие: {ev.Title} на {ev.EventDate:yyyy-MM-dd}");
                    }
                }

                Debug.WriteLine($"Всего событий за месяц: {monthEvents.Count}");
                return monthEvents;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка получения событий месяца: {ex.Message}");
                return new List<CalendarEvent>();
            }
        }

        public async Task<int> SaveCalendarEventAsync(CalendarEvent calendarEvent)
        {
            try
            {
                Debug.WriteLine($"Сохранение события: {calendarEvent.Title}");
                Debug.WriteLine($"  Дата события: {calendarEvent.EventDate:yyyy-MM-dd HH:mm:ss}");
                Debug.WriteLine($"  Ticks: {calendarEvent.EventDate.Ticks}");
                Debug.WriteLine($"  CreatedDate: {calendarEvent.CreatedDate:yyyy-MM-dd HH:mm:ss}");

                if (calendarEvent.Id != 0)
                {
                    var result = await _database.UpdateAsync(calendarEvent);
                    Debug.WriteLine($"  Обновлено, результат: {result}");
                    return result;
                }
                else
                {
                    var result = await _database.InsertAsync(calendarEvent);
                    Debug.WriteLine($"  Добавлено, ID: {calendarEvent.Id}, результат: {result}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка сохранения события: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> DeleteCalendarEventAsync(CalendarEvent calendarEvent)
        {
            try
            {
                return await _database.DeleteAsync(calendarEvent);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка удаления события: {ex.Message}");
                return 0;
            }
        }

        public async Task DebugCalendarEventsAsync()
        {
            try
            {
                var events = await _database.Table<CalendarEvent>().ToListAsync();
                Debug.WriteLine($"Всего событий в БД: {events.Count}");

                foreach (var ev in events)
                {
                    Debug.WriteLine($"Событие: ID={ev.Id}, Title='{ev.Title}', Date={ev.EventDate:dd.MM.yyyy}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка отладки: {ex.Message}");
            }
        }

        public async Task<List<TaskItem>> GetTasksForDateAsync(DateTime date)
        {
            try
            {
                var startOfDay = date.Date;
                var endOfday = startOfDay.AddDays(1).AddTicks(-1);

                return await _database.Table<TaskItem>()
                    .Where(t => t.DueDate >= startOfDay && t.DueDate <= endOfday)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка получения задач на дату: {ex.Message}");
                return new List<TaskItem>();
            }
        }

        public async Task<List<CalendarEvent>> GetEventsForDateAsync(DateTime date)
        {
            try
            {
                var start = date.Date;
                var end = start.AddDays(1).AddTicks(-1);
                return await _database.Table<CalendarEvent>()
                    .Where(e => e.EventDate >= start && e.EventDate <= end)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка получения событий: {ex.Message}");
                return new List<CalendarEvent>();
            }
        }


        public async Task<List<TaskItem>> GetAllTasksAsync()
        {
            //await Init();
            return await _database.Table<TaskItem>().ToListAsync();
        }

        public async Task<int> GetOrCreateDefaultListIdAsync()
        {
           // await Init();
            var lists = await _database.Table<TaskList>().ToListAsync();
            if (lists.Any())
                return lists.First().Id;
            // Создаём дефолтный список
            var defaultList = new TaskList { Name = "Все задачи", Description = "", CreatedDate = DateTime.Now };
            await _database.InsertAsync(defaultList);
            return defaultList.Id;
        }

        


    }
}
