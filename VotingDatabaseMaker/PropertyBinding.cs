using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace VotingDatabaseMaker
{
    public class PropertyBinding : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Fields
        private string title = "";
        private string year = "";
        private string max = "";
        private string color = "FFFFFF";
        // Public properties to bind to
        public string Title { get => title; set { title = value; OnPropertyChanged(); } }
        public string Year { get => year; set { year = value; OnPropertyChanged(); } }
        public string Max { get => max; set { max = value; OnPropertyChanged(); } }
        /// <summary>
        /// NEVER SET ANYTHING TO THIS. USE ChangeUIColorProperty() instead. I beg you.
        /// </summary>
        public string Color { get => color; set { color = value; OnPropertyChanged(); } }

        public PropertyBinding() { }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
