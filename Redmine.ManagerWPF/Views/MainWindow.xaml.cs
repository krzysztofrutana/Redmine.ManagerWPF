using CommunityToolkit.Mvvm.DependencyInjection;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Redmine.ManagerWPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ITrayable
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public async void CloseFromTray()
        {
            var _timeIntervalService = Ioc.Default.GetRequiredService<TimeIntervalsService>();

            if (await _timeIntervalService.CheckIfAnyStartedTimeIntervalExistAsync())
            {
                var _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();
                _messageBoxService.ShowWarningInfoBox("Proszę zakończyć wszystkie zadania!", "Uwaga");
                return;
            }

            this.Close();
        }

        public void OpenFromTray()
        {
            this.WindowState = WindowState.Normal;
            this.ShowInTaskbar = true;
        }



        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var _timeIntervalService = Ioc.Default.GetRequiredService<TimeIntervalsService>();

            if (_timeIntervalService.CheckIfAnyStartedTimeIntervalExist())
            {
                var _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();
                _messageBoxService.ShowWarningInfoBox("Proszę zakończyć wszystkie zadania!", "Uwaga");
                e.Cancel = true;
            }
        }

        private void MainWindowWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
            }
        }
    }
}