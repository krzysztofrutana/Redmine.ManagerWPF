using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Desktop.Extensions;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class SynchronizeProjectsViewModel : ObservableRecipient
    {
        #region Properties
        private int _totalProjectsCount;

        public int TotalProjectsCount
        {
            get => _totalProjectsCount;
            private set => SetProperty(ref _totalProjectsCount, value);
        }

        private int _value;

        public int Value
        {
            get => _value;
            private set => SetProperty(ref _value, value);
        }

        private decimal _progressBarValue;

        public decimal ProgressBarValue
        {
            get => _progressBarValue;
            set => SetProperty(ref _progressBarValue, value);
        }

        private bool _showOk;

        public bool ShowOk
        {
            get => _showOk;
            set => SetProperty(ref _showOk, value);
        }

        private string _primaryButtonText;

        public string PrimaryButtonText
        {
            get => _primaryButtonText;
            private set => SetProperty(ref _primaryButtonText, value);
        }

        private string _cancelButtonText;

        public string CancelButtonText
        {
            get => _cancelButtonText;
            private set => SetProperty(ref _cancelButtonText, value);
        }
        #endregion

        #region Injections
        private readonly ProjectService _projectService;
        private readonly Integration.Services.ProjectService _integrationProjectService;
        private readonly IMessageBoxHelper _messageBoxHelper;
        private readonly ILogger<SynchronizeProjectsViewModel> _logger;
        #endregion

        #region Commands
        public IAsyncRelayCommand SynchronizeProjectsCommand { get; }
        public IRelayCommand<ICloseable> CancelCommand { get; }
        #endregion

        public SynchronizeProjectsViewModel()
        {
            _projectService = Ioc.Default.GetRequiredService<ProjectService>();
            _integrationProjectService = Ioc.Default.GetRequiredService<Integration.Services.ProjectService>();
            _messageBoxHelper = Ioc.Default.GetRequiredService<IMessageBoxHelper>();
            _logger = Ioc.Default.GetLoggerForType<SynchronizeProjectsViewModel>();

            CancelButtonText = "Zamknij";
            PrimaryButtonText = "Rozpocznij";

            SynchronizeProjectsCommand = new AsyncRelayCommand(SynchronizeProjectAsync);
            CancelCommand = new RelayCommand<ICloseable>(Cancel);
        }

        private async Task SynchronizeProjectAsync(CancellationToken token)
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
            catch (ArgumentException ex)
            {
                _logger.LogError("{0} {1}", nameof(SynchronizeProjectAsync), ex.Message);
                CancelButtonText = "Kliknij by zamknąć";
                _messageBoxHelper.ShowWarningInfoBox("Nie skonfigurowano połączenia do bazy danych", "Błąd");
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(SynchronizeProjectAsync), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy synchronizacji projektów");
            }
        }

        private void Cancel(ICloseable dialog)
        {
            if (CancelButtonText == "Przerwij")
            {
                SynchronizeProjectsCommand.Cancel();
                CancelButtonText = "Kliknij by zamknąć";
            }
            else
            {
                dialog?.Close();
            }
        }
    }
}