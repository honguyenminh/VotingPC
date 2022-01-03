using System.Windows;

namespace AsyncDialog;

/// <summary>
/// Text dialog with an optional title and a button to close dialog.
/// </summary>
public partial class TextDialog
{
    // Fake Bindings
    public string Text { get => textBox.Text; set => textBox.Text = value; }
    public string ButtonLabel { get => (string)button.Content; set => button.Content = value; }
    public string LeftButtonLabel { get => (string)leftButton.Content; set => leftButton.Content = value; }
    public string Title
    {
        get => titleBox.Text;
        set
        {
            if (value is null) titleBox.Visibility = Visibility.Collapsed;
            else
            {
                titleBox.Visibility = Visibility.Visible;
                titleBox.Text = value;
            }
        }
    }
    public bool EnableLeftButton
    {
        get => leftButton.Visibility == Visibility.Visible;
        // Enable button if true, else disable it
        set => leftButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }

    public TextDialog()
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
}