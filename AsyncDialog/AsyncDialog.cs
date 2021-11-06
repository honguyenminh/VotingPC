using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncDialog
{
    public class AsyncDialog
    {
        // TODO: add documentation
        private readonly DialogHost dialogHost;

        private bool isOpen;
        private readonly TextDialog textDialog = new();
        private readonly LoadingDialog loadingDialog = new();
        public AsyncDialog(DialogHost dialogHost)
        {
            this.dialogHost = dialogHost;
        }

        public async Task ShowTextDialog(string text, string buttonLabel = "OK")
        {
            textDialog.Text = text;
            textDialog.ButtonLabel = buttonLabel;
            _ = await dialogHost.ShowDialog(textDialog);
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
