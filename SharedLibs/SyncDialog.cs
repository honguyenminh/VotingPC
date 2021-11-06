using MaterialDesignThemes.Wpf;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VotingPC
{
    /// <summary>
    /// [DEPRECATED] Old synchronous dialog methods. Moved to AsyncDialog.
    /// </summary>
    public class SyncDialog
    {
        private readonly DialogHost dialogHost;

        public SyncDialog(DialogHost dialogHost)
        {
            this.dialogHost = dialogHost;
        }

        /// <summary>
        /// Show a text dialog with a button, which run a custom method on button click. Should be run from an async method.
        /// The dialog WILL auto-close on button click
        /// </summary>
        /// <param name="text">Text to show</param>
        /// <param name="buttonContent">Content of button</param>
        /// <param name="clickMethod">Method to execute on button click</param>
        public void ShowTextDialog(string text, string buttonContent, Action clickMethod = null, double customScaleFactor = 2)
        {
            StackPanel stackPanel = new() { Margin = new Thickness(32) };
            TextBlock textBlock = new() { Text = text, TextWrapping = TextWrapping.Wrap };
            Button button = new()
            {
                Content = buttonContent,
                Margin = new Thickness(0, 8, 0, 0),
                FocusVisualStyle = null
            };
            button.Click += (sender, e) =>
            {
                CloseDialog();
                clickMethod?.Invoke();
            };
            button.Style = (Style)Application.Current.Resources["MaterialDesignFlatButton"];

            _ = stackPanel.Children.Add(textBlock);
            _ = stackPanel.Children.Add(button);
            stackPanel.LayoutTransform = new ScaleTransform(customScaleFactor, customScaleFactor);
            _ = dialogHost.ShowDialog(stackPanel);
        }
        /// <summary>
        /// Show a circular loading dialog. Should be run from an async method.
        /// </summary>
        public void ShowLoadingDialog()
        {
            ProgressBar progressBar = new()
            {
                Style = (Style)Application.Current.Resources["MaterialDesignCircularProgressBar"],
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(32),
                IsIndeterminate = true,
                LayoutTransform = new ScaleTransform(2, 2),
                IsTabStop = false,
                FocusVisualStyle = null
                //RenderTransform = new ScaleTransform(2, 2),
                //RenderTransformOrigin = new Point(0.5, 0.5)
            };
            _ = dialogHost.ShowDialog(progressBar);
        }
        /// <summary>
        /// Show a text dialog with 2 button, which run the provided click method on click.
        /// DOES NOT auto-close the dialog after clicked, must manually call CloseDialog in click method
        /// </summary>
        public void Show2ChoiceDialog(string text, string leftButtonContent, string rightButtonContent,
            RoutedEventHandler leftClickHandler = null, RoutedEventHandler rightClickHandler = null)
        {
            StackPanel stackPanel = new() { Margin = new Thickness(32) };
            TextBlock textBlock = new() { Text = text, TextWrapping = TextWrapping.Wrap };

            Button leftButton = new()
            {
                Content = leftButtonContent,
                Margin = new Thickness(0, 0, 4, 0),
                FocusVisualStyle = null
            };
            leftButton.Click += leftClickHandler;
            leftButton.Style = (Style)Application.Current.Resources["MaterialDesignFlatButton"];
            Button rightButton = new()
            {
                Content = rightButtonContent,
                Margin = new Thickness(4, 0, 0, 0),
                FocusVisualStyle = null
            };
            rightButton.Click += rightClickHandler;
            rightButton.Style = (Style)Application.Current.Resources["MaterialDesignFlatButton"];

            StackPanel buttonStack = new()
            {
                Margin = new(0, 8, 0, 0),
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            _ = buttonStack.Children.Add(leftButton);
            _ = buttonStack.Children.Add(rightButton);
            _ = stackPanel.Children.Add(textBlock);
            _ = stackPanel.Children.Add(buttonStack);
            stackPanel.LayoutTransform = new ScaleTransform(2, 2);
            _ = dialogHost.ShowDialog(stackPanel);
        }

        /// <summary>
        /// Close whatever dialog is currently showing. Even if there's none.
        /// </summary>
        public void CloseDialog()
        {
            if (dialogHost.IsOpen)
            {
                DialogHost.Close(dialogHost.Identifier);
            }
        }
    }
}
