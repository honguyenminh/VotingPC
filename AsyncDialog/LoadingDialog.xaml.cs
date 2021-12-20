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

namespace AsyncDialog;

/// <summary>
/// Dialog with a loading icon and optional progress text
/// </summary>
public partial class LoadingDialog
{
    public LoadingDialog()
    {
        InitializeComponent();
    }
    
    public double ScaleFactor
    {
        set
        {
            scale.ScaleX = value;
            scale.ScaleY = value;
        }
    }

    public string Text {
        set
        {
            title.Text = value;
            Thickness margin = title.Margin;
            margin.Left = value is null ? 0 : 32;
            title.Margin = margin;    
        }
    }
}