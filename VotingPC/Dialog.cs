using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VotingPC
{
    /// <summary>
    /// Dialog methods. Should be run from an async method.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Show a text dialog with a button. Should be run from an async method.
        /// </summary>
        /// <param name="text">Text to show</param>
        /// <param name="buttonContent">Content of button</param>
        /// <param name="clickMethod">Method to execute on button click</param>
        private void ShowTextDialog(string text, string buttonContent, Action clickMethod)
        {
            StackPanel stackPanel = new() { Margin = new Thickness(16) };
            TextBlock textBlock = new() { Text = text };
            Button button = new() { Content = buttonContent };
            button.Click += (sender, e) =>
            {
                CloseDialog();
                clickMethod();
            };
            button.Style = (Style)Application.Current.Resources["MaterialDesignFlatButton"];

            _ = stackPanel.Children.Add(textBlock);
            _ = stackPanel.Children.Add(button);
            _ = dialogHost.ShowDialog(stackPanel);
        }
        /// <summary>
        /// Show text method, close on button click. Should be run from an async method.
        /// </summary>
        /// <param name="text">Text to show</param>
        /// <param name="buttonContent">Content of button</param>
        private void ShowTextDialog(string text, string buttonContent)
        {
            StackPanel stackPanel = new() { Margin = new Thickness(16) };
            TextBlock textBlock = new() { Text = text, TextWrapping = TextWrapping.Wrap };
            Button button = new() { Content = buttonContent };
            button.Click += (sender, e) => CloseDialog();
            button.Style = (Style)Application.Current.Resources["MaterialDesignFlatButton"];

            _ = stackPanel.Children.Add(textBlock);
            _ = stackPanel.Children.Add(button);
            _ = dialogHost.ShowDialog(stackPanel);
        }
        /// <summary>
        /// Show a circular loading dialog. Should be run from an async method.
        /// </summary>
        private void ShowLoadingDialog()
        {
            ProgressBar progressBar = new()
            {
                Style = (Style)Application.Current.Resources["MaterialDesignCircularProgressBar"],
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(32),
                IsIndeterminate = true,
                Value = 0,
                RenderTransform = new ScaleTransform(2, 2),
                RenderTransformOrigin = new Point(0.5, 0.5)
            };
            //dialogHost.DialogContent = progressBar;

            _ = dialogHost.ShowDialog(progressBar);
        }
        /// <summary>
        /// Close whatever dialog is currently showing. Even if there's none.
        /// </summary>
        private void CloseDialog()
        {
            if (dialogHost.IsOpen)
            {
                DialogHost.Close(dialogHost.Identifier);
            }
        }
    }
}
