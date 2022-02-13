using System;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using ModernWpf;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;

namespace Redmine.ManagerWPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ITrayable
    {
        public MainWindow()
        {
            // Force light theme in app
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;

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