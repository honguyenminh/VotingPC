using MaterialDesignThemes.Wpf;
using System;
using System.Threading.Tasks;

namespace AsyncDialog
{
    /// <summary>
    /// Helper class to manage and show frequently used dialog
    /// in MaterialDesignInXaml DialogHost
    /// </summary>
    public class AsyncDialogManager
    {
        private readonly DialogHost _dialogHost;

        private bool _isOpen;
        private double _scaleFactor = 1;
        private readonly TextDialog _textDialog = new();
        private readonly PasswordDialog _passwordDialog = new();
        private readonly LoadingDialog _loadingDialog = new();

        /// <summary>
        /// Create a new AsyncDialog instance
        /// </summary>
        /// <param name="dialogHost">The DialogHost instance from MDIX inside your UI that you want dialogs to open in</param>
        public AsyncDialogManager(DialogHost dialogHost)
        {
            _dialogHost = dialogHost;
        }

        /// <summary>
        /// Change the factor used to scale the dialogs
        /// </summary>
        public double ScaleFactor
        {
            get => _scaleFactor;
            set
            {
                _scaleFactor = value;
                _textDialog.SetScaling(value);
                _passwordDialog.SetScaling(value);
                _loadingDialog.SetScaling(value);
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
            _textDialog.Text = text;
            _textDialog.Title = title;
            _textDialog.ButtonLabel = buttonLabel;
            _textDialog.EnableLeftButton = false;
            _ = await _dialogHost.ShowDialog(_textDialog);
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
            _textDialog.Text = text;
            _textDialog.Title = title;
            _textDialog.EnableLeftButton = true;
            _textDialog.LeftButtonLabel = leftButtonLabel;
            _textDialog.ButtonLabel = rightButtonLabel;
            return (bool)(await _dialogHost.ShowDialog(_textDialog))!;
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
            _passwordDialog.Title = title;
            _passwordDialog.PasswordBoxLabel = passwordBoxLabel;
            _passwordDialog.PasswordBoxHelperText = passwordBoxHelperText;
            _passwordDialog.ConfirmButtonLabel = confirmButtonLabel;
            _passwordDialog.CancelButtonLabel = cancelButtonLabel;
            return (string)await _dialogHost.ShowDialog(_passwordDialog);
        }

        /// <summary>
        /// Open a dialog with loading animation
        /// This does not await until the dialog stops, please call <see cref="CloseDialog()"/> to close it
        /// </summary>
        /// 
        public void ShowLoadingDialog()
        {
            if (_isOpen) CloseDialog();
            _ = _dialogHost.ShowDialog(_loadingDialog);
            _isOpen = true;
        }

        /// <summary>
        /// Close dialog
        /// </summary>
        /// <remarks>
        /// Intended to be used with <see cref="ShowLoadingDialog()"/>
        /// </remarks>
        /// <exception cref="NullReferenceException">Thrown if Identifier of dialog host is null</exception>
        public void CloseDialog()
        {
            if (!_isOpen) return;
            DialogHost.Close(_dialogHost.Identifier!);
            _isOpen = false;
        }
    }
}
