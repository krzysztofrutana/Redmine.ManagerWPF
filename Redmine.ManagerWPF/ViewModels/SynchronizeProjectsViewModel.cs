using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Redmine.ManagerWPF.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class SynchronizeProjectsViewModel : ObservableRecipient
    {
        private readonly ProjectService _projectService;
        private readonly Integration.Services.ProjectService _integrationProjectService;

        private int _totalProjectsCount;

        public int TotalProjectsCount
        {
            get { return _totalProjectsCount; }
            set { SetProperty(ref _totalProjectsCount, value); }
        }

        private int _value;

        public int Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        private decimal _progressBarValue;

        public decimal ProgressBarValue
        {
            get { return _progressBarValue; }
            set { SetProperty(ref _progressBarValue, value); }
        }

        private bool _showOk;

        public bool ShowOk
        {
            get { return _showOk; }
            set { SetProperty(ref _showOk, value); }
        }

        private string _primaryButtonText;

        public string PrimaryButtonText
        {
            get { return _primaryButtonText; }
            set { SetProperty(ref _primaryButtonText, value); }
        }

        private string _cancelButtonText;

        public string CancelButtonText
        {
            get { return _cancelButtonText; }
            set { SetProperty(ref _cancelButtonText, value); }
        }

        public IAsyncRelayCommand SynchronizeProjectsCommand { get; }
        public IRelayCommand CancelCommand { get; }

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public SynchronizeProjectsViewModel()
        {
            _projectService = Ioc.Default.GetRequiredService<ProjectService>();
            _integrationProjectService = Ioc.Default.GetRequiredService<Integration.Services.ProjectService>();

            CancelButtonText = "Anuluj";

            SynchronizeProjectsCommand = new AsyncRelayCommand(SynchronizeProject);
            CancelCommand = new RelayCommand(Cancel);
        }

        public Task SynchronizeProject()
        {
            var token = cancellationTokenSource.Token;
            return Task.Run(async () =>
            {
                try
                {
                    var redmineProjects = await _integrationProjectService.GetProjects();
                    TotalProjectsCount = redmineProjects.Count;
                    Value = 0;
                    ProgressBarValue = 0;
                    var step = 100 / TotalProjectsCount;
                    foreach (var redmineProject in redmineProjects)
                    {
                        await _projectService.SynchronizeProjects(redmineProject);
                        Value++;
                        ProgressBarValue = step * Value;
                    }

                    ShowOk = true;
                    CancelButtonText = String.Empty;
                    PrimaryButtonText = "Kliknij by zamknąć";
                }
                catch 
                {

                    throw;
                }

            }, token);
        }

        public void Cancel()
        {
            cancellationTokenSource.Cancel();
        }
    }
}
