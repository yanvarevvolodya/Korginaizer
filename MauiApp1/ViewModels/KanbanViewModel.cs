using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1.ViewModels
{
    public class KanbanViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private KanbanBoard _selectedBoard;

        public ObservableCollection<KanbanBoard> Boards { get; } = new();
        public ObservableCollection<KanbanColumn> Columns { get; } = new();
        // Убираем ObservableCollection<KanbanCard> Cards - карточки хранятся внутри столбцов

        public KanbanViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            // Команды
            AddBoardCommand = new Command(async () => await AddBoardAsync());
            AddColumnCommand = new Command(async () => await AddColumnAsync());
            AddCardCommand = new Command<KanbanColumn>(async (column) => await AddCardAsync(column));
            DeleteCardCommand = new Command<KanbanCard>(async (card) => await DeleteCardAsync(card));

            // Загружаем данные
            Task.Run(async () => await LoadBoardsAsync());
        }

        public KanbanBoard SelectedBoard
        {
            get => _selectedBoard;
            set
            {
                if (SetProperty(ref _selectedBoard, value) && value != null)
                {
                    // Загружаем столбцы для выбранной доски
                    Task.Run(async () => await LoadColumnsAsync(value.Id));
                }
            }
        }

        public ICommand AddBoardCommand { get; }
        public ICommand AddColumnCommand { get; }
        public ICommand AddCardCommand { get; }
        public ICommand DeleteCardCommand { get; }

        private async Task LoadBoardsAsync()
        {
            try
            {
                var boards = await _databaseService.GetKanbanBoardsAsync();

                // Используем MainThread для обновления UI
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Boards.Clear();
                    foreach (var board in boards)
                    {
                        Boards.Add(board);
                    }

                    if (boards.Any() && SelectedBoard == null)
                    {
                        SelectedBoard = boards.First();
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось загрузить доски: {ex.Message}", "OK");
            }
        }

        private async Task LoadColumnsAsync(int boardId)
        {
            try
            {
                var columns = await _databaseService.GetColumnsByBoardIdAsync(boardId);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Columns.Clear();

                    foreach (var column in columns.OrderBy(c => c.Position))
                    {
                        // Загружаем карточки для каждого столбца
                        Task.Run(async () => await LoadCardsForColumnAsync(column));
                        Columns.Add(column);
                    }

                    // Если столбцов нет, создаём стандартные
                    if (!columns.Any())
                    {
                        Task.Run(async () => await CreateDefaultColumnsAsync(boardId));
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось загрузить столбцы: {ex.Message}", "OK");
            }
        }

        private async Task LoadCardsForColumnAsync(KanbanColumn column)
        {
            try
            {
                var cards = await _databaseService.GetCardsByColumnIdAsync(column.Id);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    column.Cards.Clear();
                    foreach (var card in cards.OrderBy(c => c.Position))
                    {
                        column.Cards.Add(card);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки карточек для столбца {column.Name}: {ex.Message}");
            }
        }

        private async Task CreateDefaultColumnsAsync(int boardId)
        {
            var defaultColumns = new[]
            {
                new KanbanColumn {
                    Name = "📋 Запланировано",
                    BoardId = boardId,
                    Position = 0
                },
                new KanbanColumn {
                    Name = "⚡ В работе",
                    BoardId = boardId,
                    Position = 1
                },
                new KanbanColumn {
                    Name = "✅ Готово",
                    BoardId = boardId,
                    Position = 2
                }
            };

            foreach (var column in defaultColumns)
            {
                await _databaseService.SaveKanbanColumnAsync(column);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Columns.Add(column);
                });
            }
        }

        private async Task AddBoardAsync()
        {
            string name = await Application.Current.MainPage.DisplayPromptAsync(
                "Новая доска",
                "Введите название доски:",
                "Создать",
                "Отмена",
                "Например: Личные задачи");

            if (!string.IsNullOrWhiteSpace(name))
            {
                var board = new KanbanBoard
                {
                    Name = name.Trim(),
                    Description = "Новая канбан-доска"
                };

                await _databaseService.SaveKanbanBoardAsync(board);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Boards.Add(board);
                    SelectedBoard = board;
                });
            }
        }

        private async Task AddColumnAsync()
        {
            if (SelectedBoard == null) return;

            string name = await Application.Current.MainPage.DisplayPromptAsync(
                "Новый столбец",
                "Введите название столбца:",
                "Добавить",
                "Отмена",
                "Например: Тестирование");

            if (!string.IsNullOrWhiteSpace(name))
            {
                var column = new KanbanColumn
                {
                    Name = name.Trim(),
                    BoardId = SelectedBoard.Id,
                    Position = Columns.Count
                };

                await _databaseService.SaveKanbanColumnAsync(column);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Columns.Add(column);
                });
            }
        }

        private async Task AddCardAsync(KanbanColumn column)
        {
            if (SelectedBoard == null) return;

            string title = await Application.Current.MainPage.DisplayPromptAsync(
                "Новая карточка",
                $"Введите название для '{column.Name}':",
                "Добавить",
                "Отмена",
                "Например: Фикс бага");

            if (!string.IsNullOrWhiteSpace(title))
            {
                string description = await Application.Current.MainPage.DisplayPromptAsync(
                    "Описание",
                    "Введите описание (необязательно):",
                    "Добавить",
                    "Пропустить",
                    "");

                var card = new KanbanCard
                {
                    Title = title.Trim(),
                    Description = description?.Trim(),
                    ColumnId = column.Id,
                    BoardId = SelectedBoard.Id,
                    Position = column.Cards.Count,
                    Color = GetRandomColor() // Добавляем цвет
                };

                await _databaseService.SaveKanbanCardAsync(card);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    column.Cards.Add(card);
                });
            }
        }

        private async Task DeleteCardAsync(KanbanCard card)
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Удаление",
                $"Удалить карточку '{card.Title}'?",
                "Да", "Нет");

            if (confirm)
            {
                await _databaseService.DeleteKanbanCardAsync(card);

                // Находим столбец, содержащий эту карточку
                var column = Columns.FirstOrDefault(c => c.Id == card.ColumnId);
                if (column != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        var cardToRemove = column.Cards.FirstOrDefault(c => c.Id == card.Id);
                        if (cardToRemove != null)
                        {
                            column.Cards.Remove(cardToRemove);
                        }
                    });
                }
            }
        }

        // Метод для получения случайного цвета
        private string GetRandomColor()
        {
            var colors = new[]
            {
                "#FF6B6B", // Красный
                "#4ECDC4", // Бирюзовый
                "#45B7D1", // Голубой
                "#96CEB4", // Зеленый
                "#FFEAA7", // Желтый
                "#DDA0DD", // Фиолетовый
                "#FFA07A"  // Оранжевый
            };

            var random = new Random();
            return colors[random.Next(colors.Length)];
        }
    }
}