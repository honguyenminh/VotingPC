using System.Windows;

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