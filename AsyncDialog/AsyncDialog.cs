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
        private double scaleFactor = 1;
        private readonly TextDialog textDialog = new();
        private readonly PasswordDialog passwordDialog = new();
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
        /// Change the factor used to scale the dialogs
        /// </summary>
        public double ScaleFactor
        {
            get => scaleFactor;
            set
            {
                scaleFactor = value;
                textDialog.SetScaling(value);
                passwordDialog.SetScaling(value);
                loadingDialog.SetScaling(value);
            }
        }

        /// <summary>
        /// Show a text dialog with a button to close it and optional title
        /// </summary>
        /// <param name="text">Text to show</param>
        /// <param name="title">(Optional) Title of the dialog</param>
        /// <param name="buttonLabel">(Optional) Custom label for button, default is "OK"</param>
        public async Task ShowTextDialog(string text, string title = null, string buttonLabel = "OK")
        {
            textDialog.Text = text;
            textDialog.Title = title;
            textDialog.ButtonLabel = buttonLabel;
            textDialog.EnableLeftButton = false;
            _ = await dialogHost.ShowDialog(textDialog);
        }
        /// <summary>
        /// Show a text dialog with an optional title and two button which CLOSES DIALOG ON CLICK
        /// </summary>
        /// <param name="text">Text to show</param>
        /// <param name="title">Title of the dialog</param>
        /// <param name="leftButtonLabel">Custom label for left button, default is "CANCEL"</param>
        /// <param name="rightButtonLabel">Custom label for right button, default is "OK"</param>
        /// <returns><see langword="true"/> if user clicked right button, <see langword="false"/> otherwise</returns>
        public async Task<bool> ShowConfirmTextDialog(string text, string title = null, string leftButtonLabel = "CANCEL", string rightButtonLabel = "OK")
        {
            textDialog.Text = text;
            textDialog.Title = title;
            textDialog.EnableLeftButton = true;
            textDialog.LeftButtonLabel = leftButtonLabel;
            textDialog.ButtonLabel = rightButtonLabel;
            return (bool)await dialogHost.ShowDialog(textDialog);
        }

        /// <summary>
        /// Show a password dialog
        /// </summary>
        /// <param name="title">Title of the dialog</param>
        /// <param name="passwordBoxLabel">Label of the password box</param>
        /// <param name="passwordBoxHelperText">Helper text under password box</param>
        /// <param name="confirmButtonLabel">Custom label for confirm button, default is "OK"</param>
        /// <param name="cancelButtonLabel">Custom label for cancel button, default is "CANCEL"</param>
        /// <returns><see langword="null"/> if user cancelled. Else return the password.</returns>
        public async Task<string> ShowPasswordDialog(string title, string passwordBoxLabel,
            string passwordBoxHelperText = null, string confirmButtonLabel = "OK", string cancelButtonLabel = "CANCEL")
        {
            passwordDialog.Title = title;
            passwordDialog.PasswordBoxLabel = passwordBoxLabel;
            passwordDialog.PasswordBoxHelperText = passwordBoxHelperText;
            passwordDialog.ConfirmButtonLabel = confirmButtonLabel;
            passwordDialog.CancelButtonLabel = cancelButtonLabel;
            return (string)await dialogHost.ShowDialog(passwordDialog);
        }

        /// <summary>
        /// Open a dialog with loading animation
        /// This does not await until the dialog stops, please call <see cref="CloseDialog()">CloseDialog()</see> to close it
        /// </summary>
        /// 
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
