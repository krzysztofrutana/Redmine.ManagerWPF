using ModernWpf.Controls;
using Redmine.ManagerWPF.Abstraction.Interfaces;
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
    /// Interaction logic for EditTimeIntervalTime.xaml
    /// </summary>
    public partial class EditTimeIntervalTime : ContentDialog, ICloseable
    {
        public EditTimeIntervalTime()
        {
            InitializeComponent();
        }

        public void Close()
        {
            Hide();
        }
    }
}