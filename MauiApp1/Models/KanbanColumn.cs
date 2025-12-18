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
    public class KanbanColumn : INotifyPropertyChanged
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public int BoardId { get; set; }

        private int _position;
        public int Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        private ObservableCollection<KanbanCard> _cards = new();
        [Ignore]
        public ObservableCollection<KanbanCard> Cards
        {
            get => _cards;
            set => SetProperty(ref _cards, value);
        }

        [Ignore]
        public int CardCount => Cards?.Count ?? 0;

        [Ignore]
        public int MaxCardPosition => CardCount > 0 ? CardCount - 1 : 0;

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, value))
                return false;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}