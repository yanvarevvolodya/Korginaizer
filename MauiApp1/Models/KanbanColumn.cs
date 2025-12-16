using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1.Models
{
    public class KanbanColumn
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
        public int BoardId { get; set; } // К какой доске относится
        public int Position { get; set; } // Порядок столбцов
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        private ObservableCollection<KanbanCard> _cards = new();

        [Ignore]
        public ObservableCollection<KanbanCard> Cards
        {
            get => _cards;
            set
            {
                if (_cards != value)
                {
                    _cards = value;
                    OnPropertyChanged();
                }
            }
        }

        [Ignore]
        public int CardCount => Cards?.Count ?? 0;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
