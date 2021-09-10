using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Redmine.ManagerWPF.Desktop.Views.ContentDialogs
{
    /// <summary>
    /// Logika interakcji dla klasy SynchronizeProjects.xaml
    /// </summary>
    public partial class SynchronizeIssues : ContentDialog
    {
        public SynchronizeIssues()
        {
            InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            sender.Hide();
        }
    }
}
