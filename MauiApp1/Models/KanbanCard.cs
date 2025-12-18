using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1.Models
{
    public class KanbanCard : INotifyPropertyChanged
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

        public int ColumnId { get; set; }
        public int BoardId { get; set; }

        // ВАЖНО: Position должно быть свойством с уведомлением!
        private int _position;
        public int Position
        {
            get => _position;
            set => SetProperty(ref _position, value); // Используем SetProperty
        }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        private string _color = "#4ECDC4";
        public string Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

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