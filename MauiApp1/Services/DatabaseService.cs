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
                    await _database.CreateTableAsync<KanbanBoard>();
                    await _database.CreateTableAsync<KanbanColumn>();
                    await _database.CreateTableAsync<KanbanCard>();

                    Debug.WriteLine($"Таблица CalendarEvent создана. Путь к БД: {dbPath}");

                    // Добавляем тестовые данные, если база пустая
                    await SeedDataAsync();
                    await SeedKanbanDataAsync();
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

        private async Task SeedKanbanDataAsync()
        {
            // Тестовые данные для канбана
            var boards = await GetKanbanBoardsAsync();
            if (!boards.Any())
            {
                var board = new KanbanBoard
                {
                    Name = "Проект: Разработка",
                    Description = "Моя первая канбан-доска",
                    Color = "#007ACC"
                };

                await SaveKanbanBoardAsync(board);

                // Создаем стандартные столбцы
                var columns = new[]
                {
                new KanbanColumn { Name = "📋 Запланировано", BoardId = board.Id, Position = 0 },
                new KanbanColumn { Name = "⚡ В работе", BoardId = board.Id, Position = 1 },
                new KanbanColumn { Name = "✅ Готово", BoardId = board.Id, Position = 2 }
            };

                foreach (var column in columns)
                {
                    await SaveKanbanColumnAsync(column);

                    // Добавляем тестовые карточки
                    if (column.Name == "📋 Запланировано")
                    {
                        var card1 = new KanbanCard
                        {
                            Title = "Спроектировать базу данных",
                            Description = "Создать ER-диаграмму",
                            ColumnId = column.Id,
                            BoardId = board.Id,
                            Position = 0,
                            Color = "#FF6B6B"
                        };

                        var card2 = new KanbanCard
                        {
                            Title = "Создать UI дизайн",
                            Description = "Нарисовать макеты в Figma",
                            ColumnId = column.Id,
                            BoardId = board.Id,
                            Position = 1,
                            Color = "#4ECDC4"
                        };

                        await SaveKanbanCardAsync(card1);
                        await SaveKanbanCardAsync(card2);
                    }
                    else if (column.Name == "⚡ В работе")
                    {
                        var card = new KanbanCard
                        {
                            Title = "Реализовать TasksPage",
                            Description = "Создать список задач",
                            ColumnId = column.Id,
                            BoardId = board.Id,
                            Position = 0,
                            Color = "#FFEAA7"
                        };

                        await SaveKanbanCardAsync(card);
                    }
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

        // Методы для Kanban
        // Доски
        public async Task<List<KanbanBoard>> GetKanbanBoardsAsync()
        {
            try
            {
                return await _database.Table<KanbanBoard>().ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка получения досок: {ex.Message}");
                return new List<KanbanBoard>();
            }
        }

        public async Task<KanbanBoard> GetKanbanBoardAsync(int id)
        {
            try
            {
                return await _database.Table<KanbanBoard>()
                    .FirstOrDefaultAsync(b => b.Id == id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка получения доски: {ex.Message}");
                return null;
            }
        }

        public async Task<int> SaveKanbanBoardAsync(KanbanBoard board)
        {
            try
            {
                if (board.Id != 0)
                    return await _database.UpdateAsync(board);
                else
                    return await _database.InsertAsync(board);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка сохранения доски: {ex.Message}");
                return 0;
            }
        }

        // Метод для удаления доски
        public async Task<int> DeleteKanbanBoardAsync(KanbanBoard board)
        {
            try
            {
                return await _database.DeleteAsync(board);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка удаления доски: {ex.Message}");
                return 0;
            }
        }

        // Столбцы
        public async Task<List<KanbanColumn>> GetColumnsByBoardIdAsync(int boardId)
        {
            try
            {
                return await _database.Table<KanbanColumn>()
                    .Where(c => c.BoardId == boardId)
                    .OrderBy(c => c.Position)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка получения столбцов: {ex.Message}");
                return new List<KanbanColumn>();
            }
        }

        public async Task<KanbanColumn> GetKanbanColumnAsync(int id)
        {
            try
            {
                return await _database.Table<KanbanColumn>()
                    .FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка получения столбца: {ex.Message}");
                return null;
            }
        }

        public async Task<int> SaveKanbanColumnAsync(KanbanColumn column)
        {
            try
            {
                if (column.Id != 0)
                    return await _database.UpdateAsync(column);
                else
                    return await _database.InsertAsync(column);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка сохранения столбца: {ex.Message}");
                return 0;
            }
        }

        // Метод для удаления столбца
        public async Task<int> DeleteKanbanColumnAsync(KanbanColumn column)
        {
            try
            {
                return await _database.DeleteAsync(column);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка удаления столбца: {ex.Message}");
                return 0;
            }
        }

        // Карточки
        public async Task<List<KanbanCard>> GetCardsByColumnIdAsync(int columnId)
        {
            try
            {
                // Для Android используем более надежный подход
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    // Явно указываем порядок и проверяем данные
                    var cards = await _database.Table<KanbanCard>()
                        .Where(c => c.ColumnId == columnId)
                        .ToListAsync();

                    // Сортируем локально для надежности
                    return cards.OrderBy(c => c.Position).ToList();
                }
                else
                {
                    return await _database.Table<KanbanCard>()
                        .Where(c => c.ColumnId == columnId)
                        .OrderBy(c => c.Position)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка получения карточек для столбца {columnId}: {ex.Message}");
                return new List<KanbanCard>();
            }
        }

        public async Task<List<KanbanCard>> GetCardsByBoardIdAsync(int boardId)
        {
            try
            {
                return await _database.Table<KanbanCard>()
                    .Where(c => c.BoardId == boardId)
                    .OrderBy(c => c.Position)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка получения карточек доски: {ex.Message}");
                return new List<KanbanCard>();
            }
        }

        public async Task<KanbanCard> GetKanbanCardAsync(int id)
        {
            try
            {
                return await _database.Table<KanbanCard>()
                    .FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка получения карточки: {ex.Message}");
                return null;
            }
        }

        public async Task<int> SaveKanbanCardAsync(KanbanCard card, bool immediate = false)
        {
            try
            {
                // Для Android используем транзакции для надежности
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    await _database.RunInTransactionAsync(conn =>
                    {
                        if (card.Id != 0)
                            conn.Update(card);
                        else
                            conn.Insert(card);
                    });

                    Debug.WriteLine($"Сохранена карточка: ID={card.Id}, Title='{card.Title}', ColumnId={card.ColumnId}, Position={card.Position}");
                    return 1;
                }
                else
                {
                    if (card.Id != 0)
                        return await _database.UpdateAsync(card);
                    else
                        return await _database.InsertAsync(card);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"КРИТИЧЕСКАЯ ошибка сохранения карточки: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> DeleteKanbanCardAsync(KanbanCard card)
        {
            try
            {
                return await _database.DeleteAsync(card);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка удаления карточки: {ex.Message}");
                return 0;
            }
        }
    }
}
