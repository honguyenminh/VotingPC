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
    public class Dialogs
    {
        private readonly DialogHost dialogHost;
        
        public Dialogs(DialogHost dialogHost)
        {
            this.dialogHost = dialogHost;
        }

        /// <summary>
        /// Show a text dialog with a button, close then run a custom method on button click. Should be run from an async method.
        /// </summary>
        /// <param name="text">Text to show</param>
        /// <param name="buttonContent">Content of button</param>
        /// <param name="clickMethod">Method to execute on button click</param>
        public void ShowTextDialog(string text, string buttonContent, Action clickMethod)
        {
            StackPanel stackPanel = new() { Margin = new Thickness(32) };
            TextBlock textBlock = new() { Text = text, TextWrapping = TextWrapping.Wrap };
            Button button = new()
            {
                Content = buttonContent,
                Margin = new Thickness(0, 8, 0, 0)
            };
            button.Click += (sender, e) =>
            {
                CloseDialog();
                clickMethod();
            };
            button.Style = (Style)Application.Current.Resources["MaterialDesignFlatButton"];

            _ = stackPanel.Children.Add(textBlock);
            _ = stackPanel.Children.Add(button);
            stackPanel.LayoutTransform = new ScaleTransform(2, 2);
            _ = dialogHost.ShowDialog(stackPanel);
        }
        /// <summary>
        /// Show text method, close on button click. Should be run from an async method.
        /// </summary>
        /// <param name="text">Text to show</param>
        /// <param name="buttonContent">Content of button</param>
        public void ShowTextDialog(string text, string buttonContent)
        {
            StackPanel stackPanel = new() { Margin = new Thickness(32) };
            TextBlock textBlock = new() { Text = text, TextWrapping = TextWrapping.Wrap };
            Button button = new() {
                Content = buttonContent,
                Margin = new Thickness(0, 8, 0, 0)
            };
            button.Click += (sender, e) => CloseDialog();
            button.Style = (Style)Application.Current.Resources["MaterialDesignFlatButton"];

            _ = stackPanel.Children.Add(textBlock);
            _ = stackPanel.Children.Add(button);
            stackPanel.LayoutTransform = new ScaleTransform(2, 2);
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
                Value = 0,
                LayoutTransform = new ScaleTransform(2, 2)
                //RenderTransform = new ScaleTransform(2, 2),
                //RenderTransformOrigin = new Point(0.5, 0.5)
            };
            _ = dialogHost.ShowDialog(progressBar);
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
