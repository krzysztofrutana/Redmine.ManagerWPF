using ModernWpf.Controls;
using Redmine.ManagerWPF.Abstraction.Interfaces;

namespace Redmine.ManagerWPF.Desktop.Views.ContentDialogs
{
    /// <summary>
    /// Logika interakcji dla klasy BreakReason.xaml
    /// </summary>
    public partial class BreakReason : ContentDialog, ICloseable
    {
        public BreakReason()
        {
            InitializeComponent();
        }

        public void Close()
        {
            this.Hide();
        }
    }
}