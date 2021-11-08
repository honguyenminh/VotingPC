using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncDialog
{
    /// <summary>
    /// Main driver class to use AsyncDialog
    /// </summary>
    public class AsyncDialog
    {
        private readonly DialogHost dialogHost;

        private bool isOpen;
        private readonly TextDialog textDialog = new();
        private readonly LoadingDialog loadingDialog = new();

        /// <summary>
        /// Create a new AsyncDialog instance
        /// </summary>
        /// <param name="dialogHost">The DialogHost instance from MDIX inside your UI that you want dialogs to open in</param>
        public AsyncDialog(DialogHost dialogHost)
        {
            this.dialogHost = dialogHost;
        }

        /// <summary>
        /// Show a text dialog with a button to close it and optional title
        /// </summary>
        /// <param name="text">Text to show</param>
        /// <param name="title">(Optional) Title of dialog</param>
        /// <param name="buttonLabel">(Optional) Custom label for button, default is "OK"</param>
        /// <param name="scaleFactor">(Optional) Scale factor to scale the dialog</param>
        public async Task ShowTextDialog(string text, string title = null, string buttonLabel = "OK",  double scaleFactor = 1)
        {
            textDialog.Text = text;
            textDialog.Title= title;
            textDialog.SetScaling(scaleFactor);
            textDialog.ButtonLabel = buttonLabel;
            textDialog.EnableLeftButton = false;
            _ = await dialogHost.ShowDialog(textDialog);
        }
        /// <summary>
        /// Show a text dialog with an optional title and two button which CLOSES DIALOG ON CLICK
        /// </summary>
        /// <param name="text">Text to show</param>
        /// <param name="title"></param>
        /// <param name="leftButtonLabel"></param>
        /// <param name="rightButtonLabel"></param>
        /// <param name="scaleFactor"></param>
        /// <returns><see langword="true"/> if user clicked right button, <see langword="false"/> otherwise</returns>
        public async Task<bool> ShowConfirmTextDialog(string text, string title = null, string leftButtonLabel = "Cancel", string rightButtonLabel = "OK", double scaleFactor = 1)
        {
            textDialog.Text = text;
            textDialog.Title = title;
            textDialog.SetScaling(scaleFactor);
            textDialog.EnableLeftButton = true;
            textDialog.LeftButtonLabel = leftButtonLabel;
            textDialog.ButtonLabel = rightButtonLabel;
            return (bool)await dialogHost.ShowDialog(textDialog);
        }

        /// <summary>
        /// Open a dialog with loading animation
        /// This does not await until the dialog stops, please call <see cref="CloseDialog()">CloseDialog()</see> to close it
        /// </summary>
        public void ShowLoadingDialog()
        {
            if (isOpen) CloseDialog();
            _ = dialogHost.ShowDialog(loadingDialog);
            isOpen = true;
        }

        /// <summary>
        /// Close dialog, to be used with <see cref="ShowLoadingDialog()">ShowLoadingDialog()</see>
        /// </summary>
        public void CloseDialog()
        {
            if (!isOpen) return;
            DialogHost.Close(dialogHost.Identifier);
            isOpen = false;
        }
    }
}
