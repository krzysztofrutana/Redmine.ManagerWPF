using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using ModernWpf.Controls;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;
using System;
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
        public IRelayCommand<ICloseable> CancelCommand { get; }

        private readonly IMessageBoxService _messageBoxService;

        public SynchronizeProjectsViewModel()
        {
            _projectService = Ioc.Default.GetRequiredService<ProjectService>();
            _integrationProjectService = Ioc.Default.GetRequiredService<Integration.Services.ProjectService>();
            _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();

            CancelButtonText = "Zamknij";
            PrimaryButtonText = "Rozpocznij";

            SynchronizeProjectsCommand = new AsyncRelayCommand(SynchronizeProject);
            CancelCommand = new RelayCommand<ICloseable>(Cancel);
        }

        public async Task SynchronizeProject(CancellationToken token)
        {
            try
            {
                CancelButtonText = "Przerwij";
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
                CancelButtonText = "Kliknij by zamknąć";
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy synchronizacji projektów");
            }
        }

        public void Cancel(ICloseable dialog)
        {
            if (CancelButtonText == "Przerwij")
            {
                SynchronizeProjectsCommand.Cancel();
                CancelButtonText = "Kliknij by zamknąć";
            }
            else
            {
                if (dialog != null)
                {
                    dialog.Close();
                }
            }
        }
    }
}