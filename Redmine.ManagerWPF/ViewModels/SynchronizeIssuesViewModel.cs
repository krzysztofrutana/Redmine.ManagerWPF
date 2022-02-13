using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Data.Enums;
using Redmine.ManagerWPF.Desktop.Extensions;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class SynchronizeIssuesViewModel : ObservableRecipient
    {
        private string _totalIssuesCount;

        public string TotalIssuesCount
        {
            get => _totalIssuesCount;
            set => SetProperty(ref _totalIssuesCount, value, nameof(TotalIssuesCount));
        }

        private int _value;

        public int Value
        {
            get => _value;
            set => SetProperty(ref _value, value, nameof(Value));
        }

        private decimal _progressBarValue;

        public decimal ProgressBarValue
        {
            get => _progressBarValue;
            set => SetProperty(ref _progressBarValue, value, nameof(ProgressBarValue));
        }

        private bool _showOk;

        public bool ShowOk
        {
            get => _showOk;
            set => SetProperty(ref _showOk, value);
        }

        private bool _showDownloadIssues;

        public bool ShowDownloadIssues
        {
            get => _showDownloadIssues;
            set => SetProperty(ref _showDownloadIssues, value);
        }

        private bool _showProgressText;

        public bool ShowProgressText
        {
            get => _showProgressText;
            set => SetProperty(ref _showProgressText, value);
        }

        private bool _showTreeUpdateText;

        public bool ShowTreeUpdateText
        {
            get => _showTreeUpdateText;
            set => SetProperty(ref _showTreeUpdateText, value);
        }

        private string _primaryButtonText;

        public string PrimaryButtonText
        {
            get => _primaryButtonText;
            set { SetProperty(ref _primaryButtonText, value); }
        }

        private string _cancelButtonText;

        public string CancelButtonText
        {
            get { return _cancelButtonText; }
            set { SetProperty(ref _cancelButtonText, value); }
        }

        private readonly IMessageBoxService _messageBoxService;
        private readonly ILogger<SynchronizeIssuesViewModel> _logger;
        private readonly IssueService _issueService;
        private readonly Integration.Services.IssueService _integrationIssueService;
        private readonly Integration.Services.JournalService _integrationJournalService;

        public IAsyncRelayCommand SynchronizeIssuesCommand { get; }
        public IRelayCommand<ICloseable> CancelCommand { get; }

        public SynchronizeIssuesViewModel()
        {
            _issueService = Ioc.Default.GetRequiredService<IssueService>();
            _integrationIssueService = Ioc.Default.GetRequiredService<Integration.Services.IssueService>();
            _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();
            _logger = Ioc.Default.GetLoggerForType<SynchronizeIssuesViewModel>();
            _integrationJournalService = Ioc.Default.GetRequiredService<Integration.Services.JournalService>();

            CancelButtonText = "Zamknij";
            PrimaryButtonText = "Rozpocznij";

            SynchronizeIssuesCommand = new AsyncRelayCommand(SynchronizeIssues);
            CancelCommand = new RelayCommand<ICloseable>(Cancel);
        }

        public async Task SynchronizeIssues(CancellationToken token)
        {
            try
            {
                decimal fullBarValue = 100;

                SetStatus(SynchronizeIssueStatusType.DownloadIssues);
                var redmineIssues = await _integrationIssueService.GetIssues();
                TotalIssuesCount = redmineIssues.Count.ToString();
                Value = 0;
                ProgressBarValue = 0;
                decimal step = fullBarValue / redmineIssues.Count;
                foreach (var redmineIssue in redmineIssues)
                {
                    redmineIssue.Comments = await _integrationJournalService.GetIssueJournals(redmineIssue);
                    Value++;
                    ProgressBarValue = step * Value;
                }

                SetStatus(SynchronizeIssueStatusType.SynchronizeIssues);
                Value = 0;
                ProgressBarValue = 0;
                foreach (var issue in redmineIssues)
                {
                    await _issueService.SynchronizeIssues(issue);
                    Value++;
                    ProgressBarValue = step * Value;
                }

                SetStatus(SynchronizeIssueStatusType.BuildTree);
                var allIssues = await _issueService.GetAllIssueAsync();
                TotalIssuesCount = allIssues.Count().ToString();
                Value = 0;
                ProgressBarValue = 0;
                step = fullBarValue / allIssues.Count();
                foreach (var issue in allIssues)
                {
                    var redmineIssue = redmineIssues.FirstOrDefault(x => x.Id == issue.SourceId);
                    if (redmineIssue != null && redmineIssue.ParentIssueId != null)
                    {
                        await _issueService.UpdateTreeStructure(redmineIssue, issue);
                    }
                    Value++;
                    ProgressBarValue = step * Value;
                }

                SetStatus(SynchronizeIssueStatusType.AllDone);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("{0} {1}", nameof(SynchronizeIssues), ex.Message);
                CancelButtonText = "Kliknij by zamknąć";
                _messageBoxService.ShowWarningInfoBox("Nie skonfigurowano połączenia do bazy danych", "Błąd");
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(SynchronizeIssues), ex.Message);
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy synchronizacji zadań");
            }
        }

        public void Cancel(ICloseable dialog)
        {
            if (CancelButtonText == "Przerwij")
            {
                SynchronizeIssuesCommand.Cancel();
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

        private void SetStatus(SynchronizeIssueStatusType status)
        {
            switch (status)
            {
                case SynchronizeIssueStatusType.DownloadIssues:
                    TotalIssuesCount = "-";
                    CancelButtonText = "Przerwij";
                    ShowProgressText = false;
                    ShowDownloadIssues = true;
                    ShowTreeUpdateText = false;
                    break;

                case SynchronizeIssueStatusType.SynchronizeIssues:
                    ShowProgressText = true;
                    ShowDownloadIssues = false;
                    ShowTreeUpdateText = false;
                    break;

                case SynchronizeIssueStatusType.BuildTree:
                    ShowProgressText = false;
                    ShowDownloadIssues = false;
                    ShowTreeUpdateText = true;
                    break;

                case SynchronizeIssueStatusType.AllDone:
                    ShowOk = true;
                    CancelButtonText = String.Empty;
                    CancelButtonText = "Kliknij by zamknąć";
                    ProgressBarValue = 100;
                    break;
            }
        }
    }
}