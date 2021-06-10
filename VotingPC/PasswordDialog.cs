using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VotingPC
{
    internal class PasswordDialog
    {
        private readonly RoutedEventHandler routedEventHandler;
        public bool falsePassword;
        private readonly string wrongPasswordText;
        private readonly StackPanel stackPanel;
        private readonly TextBlock title;
        private readonly PasswordBox passwordBox;
        private readonly Button button;
        public string Password => passwordBox.Password;
        public PasswordDialog(string titleText, string wrongPasswordTitleText, string buttonText, RoutedEventHandler eventHandler)
        {
            routedEventHandler = eventHandler;
            wrongPasswordText = wrongPasswordTitleText;
            stackPanel = new()
            {
                Margin = new Thickness(32),
                LayoutTransform = new ScaleTransform(2, 2),
                Orientation = Orientation.Vertical
            };
            title = new()
            {
                Text = titleText,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            passwordBox = new()
            {
                MaxLength = 32,
                Margin = new Thickness(10)
            };
            passwordBox.KeyDown += (sender, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    routedEventHandler(sender, null);
                }
            };
            // Add top text on password box
            HintAssist.SetHelperText(passwordBox, "Mật khẩu");
            button = new()
            {
                Content = buttonText,
                HorizontalAlignment = HorizontalAlignment.Right,
                Style = (Style)Application.Current.Resources["MaterialDesignFlatButton"]
            };
            button.Click += routedEventHandler;

            if (falsePassword)
            {
                title.Text = wrongPasswordTitleText;
                title.Foreground = Brushes.Red;
            }

            _ = stackPanel.Children.Add(title);
            _ = stackPanel.Children.Add(passwordBox);
            _ = stackPanel.Children.Add(button);
        }

        public StackPanel Dialog
        {
            get
            {
                if (falsePassword)
                {
                    title.Text = wrongPasswordText;
                    title.Foreground = Brushes.Red;
                }
                return stackPanel;
            }
        }
    }
}
