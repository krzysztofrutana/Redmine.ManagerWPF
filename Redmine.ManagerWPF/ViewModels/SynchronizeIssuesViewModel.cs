using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Redmine.ManagerWPF.Data.Enums;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class SynchronizeIssuesViewModel : ObservableRecipient
    {
        private readonly IssueService _issueService;
        private readonly Integration.Services.IssueService _integrationIssueService;

        private int _totalIssuesCount;

        public int TotalIssuesCount
        {
            get { return _totalIssuesCount; }
            set { SetProperty(ref _totalIssuesCount, value); }
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

        private bool _showDownloadIssues;

        public bool ShowDownloadIssues
        {
            get { return _showDownloadIssues; }
            set { SetProperty(ref _showDownloadIssues, value); }
        }

        private bool _showProgressText;

        public bool ShowProgressText
        {
            get { return _showProgressText; }
            set { SetProperty(ref _showProgressText, value); }
        }

        private bool _showTreeUpdateText;

        public bool ShowTreeUpdateText
        {
            get { return _showTreeUpdateText; }
            set { SetProperty(ref _showTreeUpdateText, value); }
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

        private readonly IMessageBoxService _messageBoxService;

        public IAsyncRelayCommand SynchronizeIssuesCommand { get; }
        public IAsyncRelayCommand CancelCommand { get; }

        public SynchronizeIssuesViewModel()
        {
            _issueService = Ioc.Default.GetRequiredService<IssueService>();
            _integrationIssueService = Ioc.Default.GetRequiredService<Integration.Services.IssueService>();
            _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();

            CancelButtonText = "Anuluj";
            PrimaryButtonText = "Rozpocznij";

            SynchronizeIssuesCommand = new AsyncRelayCommand(SynchronizeIssues);
            CancelCommand = new AsyncRelayCommand(Cancel);
        }

        public async Task SynchronizeIssues(CancellationToken token)
        {
            try
            {
                SetStatus(SynchronizeIssueStatusType.DownloadIssues);
                var redmineIssues = await _integrationIssueService.GetIssues();
                TotalIssuesCount = redmineIssues.Count;
                Value = 0;
                ProgressBarValue = 0;
                var step = 100 / TotalIssuesCount;
                foreach (var redmineIssue in redmineIssues)
                {
                    await _integrationIssueService.GetIssueJournals(redmineIssue);
                    Value++;
                    ProgressBarValue = step * Value;
                }

                SetStatus(SynchronizeIssueStatusType.SynchronizeIssues);
                Value = 0;
                ProgressBarValue = 0;
                foreach (var redmineProject in redmineIssues)
                {
                    await _issueService.SynchronizeIssues(redmineProject);
                    Value++;
                    ProgressBarValue = step * Value;
                }

                SetStatus(SynchronizeIssueStatusType.BuildTree);
                var allIssues = await _issueService.GetAllIssueAsync();
                TotalIssuesCount = allIssues.Count;
                Value = 0;
                ProgressBarValue = 0;
                step = 100 / TotalIssuesCount;
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
            catch (Exception ex)
            {
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy synchronizacji zadań");
            }
        }

        public Task Cancel()
        {
            SynchronizeIssuesCommand.Cancel();

            return Task.CompletedTask;
        }

        private void SetStatus(SynchronizeIssueStatusType status)
        {
            switch (status)
            {
                case SynchronizeIssueStatusType.DownloadIssues:
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