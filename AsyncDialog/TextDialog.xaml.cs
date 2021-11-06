using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AsyncDialog
{
    /// <summary>
    /// Text dialog class
    /// </summary>
    public partial class TextDialog : UserControl
    {
        // Fake Bindings
        public string Text { get => textBox.Text; set => textBox.Text = value; }
        public string ButtonLabel { get => (string)button.Content; set => button.Content = value; }

        public TextDialog()
        {
            InitializeComponent();
        }

        public void SetScaling(double scaleFactor)
        {
            scale.ScaleX = scaleFactor;
            scale.ScaleY = scaleFactor;
        }
    }
}