using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VotingDatabaseMaker
{
    public class PropertyBinding : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Fields
        private string _title = "";
        private string _year = "";
        private string _max = "";
        private string _color = "FFFFFF";
        // Public properties to bind to
        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }
        public string Year { get => _year; set { _year = value; OnPropertyChanged(); } }
        public string Max { get => _max; set { _max = value; OnPropertyChanged(); } }
        /// <summary>
        /// NEVER SET ANYTHING TO THIS. USE ChangeUIColorProperty() instead. I beg you.
        /// </summary>
        public string Color { get => _color; set { _color = value; OnPropertyChanged(); } }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
