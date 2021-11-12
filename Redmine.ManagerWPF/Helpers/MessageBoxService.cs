using Redmine.ManagerWPF.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Redmine.ManagerWPF.Desktop.Helpers
{
    public class MessageBoxService : IMessageBoxService
    {
        public void ShowWarningInfoBox(string text, string caption)
        {
            MessageBox.Show(text, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
