using MaterialDesignThemes.Wpf;
using System.Windows;

namespace AsyncDialog;

/// <summary>
/// A password dialog with title, two button and a password field with optional helper text
/// </summary>
public partial class PasswordDialog
{
    public string Title
    {
        get => titleBox.Text;
        set => titleBox.Text = value;
    }
    public string ConfirmButtonLabel {
        get => (string)confirmButton.Content;
        set => confirmButton.Content = value; 
    }
    public string CancelButtonLabel
    {
        get => (string)cancelButton.Content; 
        set => cancelButton.Content = value;
    }
    public string PasswordBoxLabel
    {
        get => (string)HintAssist.GetHint(passwordBox); 
        set => HintAssist.SetHint(passwordBox, value);
    }
    public string PasswordBoxHelperText
    {
        get => (string)HintAssist.GetHelperText(passwordBox);
        set => HintAssist.SetHelperText(passwordBox, value);
    }
    
    public PasswordDialog()
    {
        InitializeComponent();
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        DialogHost.CloseDialogCommand.Execute(passwordBox.Password, null);
        passwordBox.Password = string.Empty;
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