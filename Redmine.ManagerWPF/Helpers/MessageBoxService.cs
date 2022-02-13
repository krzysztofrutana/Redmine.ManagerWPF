using System.Windows;
using Redmine.ManagerWPF.Helpers.Interfaces;

namespace Redmine.ManagerWPF.Desktop.Helpers
{
    public class MessageBoxService : IMessageBoxService
    {
        public void ShowWarningInfoBox(string text, string caption)
        {
            ModernWpf.MessageBox.Show(text, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public void ShowInformationBox(string text, string caption)
        {
            ModernWpf.MessageBox.Show(text, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public bool ShowConfirmationBox(string text, string caption)
        {
            var result = ModernWpf.MessageBox.Show(text, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}