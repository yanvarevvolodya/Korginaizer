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

        public KanbanViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            // Команды
            AddBoardCommand = new Command(async () => await AddBoardAsync());
            DeleteBoardCommand = new Command<KanbanBoard>(async (board) => await DeleteBoardAsync(board));
            AddColumnCommand = new Command(async () => await AddColumnAsync());
            DeleteColumnCommand = new Command<KanbanColumn>(async (column) => await DeleteColumnAsync(column));

            // Команды для управления порядком
            MoveColumnLeftCommand = new Command<KanbanColumn>(async (column) => await MoveColumnLeftAsync(column));
            MoveColumnRightCommand = new Command<KanbanColumn>(async (column) => await MoveColumnRightAsync(column));

            // Команды для карточек
            AddCardCommand = new Command<KanbanColumn>(async (column) => await AddCardAsync(column));
            DeleteCardCommand = new Command<KanbanCard>(async (card) => await DeleteCardAsync(card));
            MoveCardUpCommand = new Command<KanbanCard>(async (card) => await MoveCardUpAsync(card));
            MoveCardDownCommand = new Command<KanbanCard>(async (card) => await MoveCardDownAsync(card));
            MoveCardToColumnCommand = new Command<KanbanCard>(async (card) => await MoveCardToColumnAsync(card));

            LoadBoards();
        }

        public KanbanBoard SelectedBoard
        {
            get => _selectedBoard;
            set
            {
                if (SetProperty(ref _selectedBoard, value) && value != null)
                {
                    LoadColumns(value.Id);
                }
            }
        }

        public ICommand AddBoardCommand { get; }
        public ICommand DeleteBoardCommand { get; }
        public ICommand AddColumnCommand { get; }
        public ICommand DeleteColumnCommand { get; }
        public ICommand MoveColumnLeftCommand { get; }
        public ICommand MoveColumnRightCommand { get; }
        public ICommand AddCardCommand { get; }
        public ICommand DeleteCardCommand { get; }
        public ICommand MoveCardUpCommand { get; }
        public ICommand MoveCardDownCommand { get; }
        public ICommand MoveCardToColumnCommand { get; }

        #region Загрузка данных

        private async void LoadBoards()
        {
            try
            {
                var boards = await _databaseService.GetKanbanBoardsAsync();

                Boards.Clear();
                foreach (var board in boards)
                {
                    Boards.Add(board);
                }

                if (boards.Any())
                {
                    SelectedBoard = boards.First();
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось загрузить доски: {ex.Message}", "OK");
            }
        }

        private async void LoadColumns(int boardId)
        {
            try
            {
                // ВАЖНО: Всегда очищаем столбцы при загрузке новой доски
                Columns.Clear();

                var columns = await _databaseService.GetColumnsByBoardIdAsync(boardId);

                // Если в базе есть столбцы для этой доски
                if (columns.Any())
                {
                    foreach (var column in columns.OrderBy(c => c.Position))
                    {
                        Columns.Add(column);
                        await LoadCardsForColumn(column);
                    }
                }
                else
                {
                    // Если столбцов нет - создаем стандартные
                    await CreateDefaultColumns(boardId);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось загрузить столбцы: {ex.Message}", "OK");
            }
        }

        private async Task LoadCardsForColumn(KanbanColumn column)
        {
            try
            {
                var cards = await _databaseService.GetCardsByColumnIdAsync(column.Id);

                column.Cards.Clear();
                foreach (var card in cards.OrderBy(c => c.Position))
                {
                    column.Cards.Add(card);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки карточек: {ex.Message}");
            }
        }

        #endregion

        #region Методы перемещения (ОПТИМИЗИРОВАННЫЕ)

        private async Task MoveColumnLeftAsync(KanbanColumn column)
        {
            if (column == null || column.Position <= 0) return;

            try
            {
                var leftColumn = Columns.FirstOrDefault(c => c.Position == column.Position - 1);
                if (leftColumn == null) return;

                column.Position--;
                leftColumn.Position++;

                // ИСПРАВЛЕНО: Используем Move вместо Clear/Add
                var indexFrom = Columns.IndexOf(column);
                var indexTo = Columns.IndexOf(leftColumn);
                Columns.Move(indexFrom, indexTo);

                await _databaseService.SaveKanbanColumnAsync(column);
                await _databaseService.SaveKanbanColumnAsync(leftColumn);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось переместить столбец: {ex.Message}", "OK");
            }
        }

        private async Task MoveColumnRightAsync(KanbanColumn column)
        {
            if (column == null) return;

            try
            {
                var rightColumn = Columns.FirstOrDefault(c => c.Position == column.Position + 1);
                if (rightColumn == null) return;

                column.Position++;
                rightColumn.Position--;

                // ИСПРАВЛЕНО: Используем Move вместо Clear/Add
                var indexFrom = Columns.IndexOf(column);
                var indexTo = Columns.IndexOf(rightColumn);
                Columns.Move(indexFrom, indexTo);

                await _databaseService.SaveKanbanColumnAsync(column);
                await _databaseService.SaveKanbanColumnAsync(rightColumn);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось переместить столбец: {ex.Message}", "OK");
            }
        }

        private async Task MoveCardUpAsync(KanbanCard card)
        {
            if (card == null) return;

            try
            {
                var column = Columns.FirstOrDefault(c => c.Id == card.ColumnId);
                if (column == null || card.Position <= 0) return;

                var cardAbove = column.Cards.FirstOrDefault(c => c.Position == card.Position - 1);
                if (cardAbove == null) return;

                // ИЗМЕНЕНИЕ 1: Делаем обновление на фоновом потоке
                await Task.Run(() =>
                {
                    // Меняем позиции
                    card.Position--;
                    cardAbove.Position++;

                    // Перемещаем в коллекции
                    var indexFrom = column.Cards.IndexOf(card);
                    var indexTo = column.Cards.IndexOf(cardAbove);

                    // ВАЖНО: На Android Move() может блокировать UI
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        column.Cards.Move(indexFrom, indexTo);
                    });
                });

                // ИЗМЕНЕНИЕ 2: Сохраняем в БД на фоновом потоке
                await Task.WhenAll(
                    Task.Run(() => _databaseService.SaveKanbanCardAsync(card)),
                    Task.Run(() => _databaseService.SaveKanbanCardAsync(cardAbove))
                );

                Console.WriteLine("=== MoveCardUpAsync УСПЕШНО ЗАВЕРШЕНО ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА в MoveCardUpAsync: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось переместить карточку: {ex.Message}", "OK");
            }
        }

        private async Task MoveCardDownAsync(KanbanCard card)
        {
            if (card == null) return;

            try
            {
                var column = Columns.FirstOrDefault(c => c.Id == card.ColumnId);
                if (column == null || column.Cards.Count <= 1) return;

                var maxPosition = column.Cards.Count - 1;
                if (card.Position >= maxPosition) return;

                var cardBelow = column.Cards.FirstOrDefault(c => c.Position == card.Position + 1);
                if (cardBelow == null) return;

                // Делаем на фоновом потоке
                await Task.Run(() =>
                {
                    card.Position++;
                    cardBelow.Position--;

                    var indexFrom = column.Cards.IndexOf(card);
                    var indexTo = column.Cards.IndexOf(cardBelow);

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        column.Cards.Move(indexFrom, indexTo);
                    });
                });

                // Сохраняем асинхронно
                await Task.WhenAll(
                    Task.Run(() => _databaseService.SaveKanbanCardAsync(card)),
                    Task.Run(() => _databaseService.SaveKanbanCardAsync(cardBelow))
                );
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось переместить карточку: {ex.Message}", "OK");
            }
        }

        private async Task MoveCardToColumnAsync(KanbanCard card)
        {
            if (card == null || SelectedBoard == null) return;

            try
            {
                var columnNames = Columns.Select(c => c.Name).ToList();
                var action = await Application.Current.MainPage.DisplayActionSheet(
                    $"Переместить карточку '{card.Title}' в:",
                    "Отмена",
                    null,
                    columnNames.ToArray());

                if (string.IsNullOrEmpty(action) || action == "Отмена") return;

                var targetColumn = Columns.FirstOrDefault(c => c.Name == action);
                if (targetColumn == null || targetColumn.Id == card.ColumnId) return;

                var sourceColumn = Columns.FirstOrDefault(c => c.Id == card.ColumnId);
                if (sourceColumn == null) return;

                // Удаляем из текущего
                sourceColumn.Cards.Remove(card);

                // Обновляем позиции в исходном столбце
                for (int i = 0; i < sourceColumn.Cards.Count; i++)
                {
                    sourceColumn.Cards[i].Position = i;
                    await _databaseService.SaveKanbanCardAsync(sourceColumn.Cards[i]);
                }

                // Обновляем ID столбца
                card.ColumnId = targetColumn.Id;
                card.Position = targetColumn.Cards.Count;

                // Добавляем в новый
                targetColumn.Cards.Add(card);

                // Обновляем позиции в целевом столбце
                for (int i = 0; i < targetColumn.Cards.Count; i++)
                {
                    targetColumn.Cards[i].Position = i;
                    await _databaseService.SaveKanbanCardAsync(targetColumn.Cards[i]);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось переместить карточку: {ex.Message}", "OK");
            }
        }

        #endregion

        #region Остальные методы

        private async Task CreateDefaultColumns(int boardId)
        {
            var defaultColumns = new[]
            {
                new KanbanColumn { Name = "📋 Запланировано", BoardId = boardId, Position = 0 },
                new KanbanColumn { Name = "⚡ В работе", BoardId = boardId, Position = 1 },
                new KanbanColumn { Name = "✅ Готово", BoardId = boardId, Position = 2 }
            };

            foreach (var column in defaultColumns)
            {
                await _databaseService.SaveKanbanColumnAsync(column);
                Columns.Add(column);
                // ВАЖНО: Загружаем карточки (их нет, но это инициализирует коллекцию)
                await LoadCardsForColumn(column);
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
                Boards.Add(board);

                // ВАЖНО: Устанавливаем SelectedBoard, что вызовет LoadColumns
                SelectedBoard = board;

                // Отладка
                Console.WriteLine($"Создана новая доска: {board.Name}, ID={board.Id}");
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
                Columns.Add(column);
            }
        }

        private async Task AddCardAsync(KanbanColumn column)
        {
            if (SelectedBoard == null || column == null) return;

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
                    Color = GetRandomColor()
                };

                await _databaseService.SaveKanbanCardAsync(card);
                column.Cards.Add(card);
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

                var column = Columns.FirstOrDefault(c => c.Id == card.ColumnId);
                if (column != null)
                {
                    var cardToRemove = column.Cards.FirstOrDefault(c => c.Id == card.Id);
                    if (cardToRemove != null)
                    {
                        column.Cards.Remove(cardToRemove);

                        // Обновляем позиции
                        for (int i = 0; i < column.Cards.Count; i++)
                        {
                            column.Cards[i].Position = i;
                            await _databaseService.SaveKanbanCardAsync(column.Cards[i]);
                        }
                    }
                }
            }
        }

        private async Task DeleteBoardAsync(KanbanBoard board)
        {
            if (board == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Удаление доски",
                $"Удалить доску '{board.Name}' и все её столбцы с карточками?",
                "Да, удалить",
                "Отмена");

            if (confirm)
            {
                try
                {
                    var columns = await _databaseService.GetColumnsByBoardIdAsync(board.Id);
                    foreach (var column in columns)
                    {
                        var cards = await _databaseService.GetCardsByColumnIdAsync(column.Id);
                        foreach (var card in cards)
                        {
                            await _databaseService.DeleteKanbanCardAsync(card);
                        }
                        await _databaseService.DeleteKanbanColumnAsync(column);
                    }

                    await _databaseService.DeleteKanbanBoardAsync(board);

                    Boards.Remove(board);

                    if (SelectedBoard?.Id == board.Id)
                    {
                        SelectedBoard = Boards.FirstOrDefault();
                        if (SelectedBoard == null)
                        {
                            Columns.Clear();
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка",
                        $"Не удалось удалить доску: {ex.Message}", "OK");
                }
            }
        }

        private async Task DeleteColumnAsync(KanbanColumn column)
        {
            if (column == null || SelectedBoard == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Удаление столбца",
                $"Удалить столбец '{column.Name}' и все карточки в нём?",
                "Да, удалить",
                "Отмена");

            if (confirm)
            {
                try
                {
                    var cards = await _databaseService.GetCardsByColumnIdAsync(column.Id);
                    foreach (var card in cards)
                    {
                        await _databaseService.DeleteKanbanCardAsync(card);
                    }

                    await _databaseService.DeleteKanbanColumnAsync(column);

                    Columns.Remove(column);

                    // Обновляем позиции оставшихся столбцов
                    for (int i = 0; i < Columns.Count; i++)
                    {
                        if (Columns[i].Position != i)
                        {
                            Columns[i].Position = i;
                            await _databaseService.SaveKanbanColumnAsync(Columns[i]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка",
                        $"Не удалось удалить столбец: {ex.Message}", "OK");
                }
            }
        }

        private string GetRandomColor()
        {
            var colors = new[]
            {
                "#FF6B6B", "#4ECDC4", "#45B7D1", "#96CEB4",
                "#FFEAA7", "#DDA0DD", "#FFA07A"
            };

            return colors[new Random().Next(colors.Length)];
        }

        #endregion
    }
}