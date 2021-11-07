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
        // TODO: add documentation
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
        /// Show a text dialog with a button to close it
        /// </summary>
        /// <param name="text">Text to show</param>
        /// <param name="title">(Optional) Title of dialog</param>
        /// <param name="buttonLabel">(Optional) Custom label for button, default is "OK"</param>
        /// <param name="scaleFactor">Scale factor to scale the dialog</param>
        public async Task ShowTextDialog(string text, string title = null, string buttonLabel = "OK", double scaleFactor = 1)
        {
            textDialog.Text = text;
            textDialog.Title= title;
            textDialog.SetScaling(scaleFactor);
            textDialog.ButtonLabel = buttonLabel;
            _ = await dialogHost.ShowDialog(textDialog);
        }

        public async Task ShowConfirmDialog(string text)
        {

        }

        /// <summary>
        /// Open a dialog with loading animation
        /// </summary>
        public void ShowLoadingDialog()
        {
            if (isOpen) CloseDialog();
            _ = dialogHost.ShowDialog(loadingDialog);
            isOpen = true;
        }

        /// <summary>
        /// Close dialog
        /// </summary>
        public void CloseDialog()
        {
            if (!isOpen) return;
            DialogHost.Close(dialogHost.Identifier);
            isOpen = false;
        }
    }
}
