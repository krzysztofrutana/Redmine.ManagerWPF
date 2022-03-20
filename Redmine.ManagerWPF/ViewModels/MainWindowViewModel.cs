using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Desktop.Extensions;
using Redmine.ManagerWPF.Desktop.Helpers;
using Redmine.ManagerWPF.Desktop.Messages.BreakReason;
using Redmine.ManagerWPF.Desktop.Messages.MainWindowTreeView;
using Redmine.ManagerWPF.Desktop.Messages.ProjectCombobox;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Desktop.Views.ContentDialogs;
using Redmine.ManagerWPF.Desktop.Views.Windows;
using Redmine.ManagerWPF.Helpers.Interfaces;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class MainWindowViewModel : ObservableRecipient
    {
        #region Properties
        public ObservableCollection<Models.Projects.ListItemModel> Projects { get; private set; } = new ObservableCollection<Models.Projects.ListItemModel>();
        public ObservableCollection<Models.Tree.TreeModel> Issues { get; private set; } = new ExtendedObservableCollection<Models.Tree.TreeModel>();

        private Models.Projects.ListItemModel _selectedProject;

        public Models.Projects.ListItemModel SelectedProject
        {
            get => _selectedProject;
            set
            {
                SetProperty(ref _selectedProject, value);

                if (value != null)
                {
                    WeakReferenceMessenger.Default.Send(new ProjectChangeMessage(value));
                }

                ViewProjectDetails = value != null;
                ViewIssuesList = value != null;
            }
        }

        private bool _viewProjectDetails;

        public bool ViewProjectDetails
        {
            get => _viewProjectDetails;
            set => SetProperty(ref _viewProjectDetails, value);
        }

        private bool _viewIssuesList;
        public bool ViewIssuesList
        {
            get => _viewIssuesList;
            set => SetProperty(ref _viewIssuesList, value);
        }


        private bool _viewIssueDetails;

        public bool ViewIssueDetails
        {
            get => _viewIssueDetails;
            set => SetProperty(ref _viewIssueDetails, value);
        }

        private bool _viewCommentDetails;

        public bool ViewCommentDetails
        {
            get => _viewCommentDetails;
            set => SetProperty(ref _viewCommentDetails, value);
        }

        private bool _viewTimeIntervalList;

        public bool ViewTimeIntervalList
        {
            get => _viewTimeIntervalList;
            set => SetProperty(ref _viewTimeIntervalList, value);
        }
        #endregion

        #region Private fields
        private Stopwatch _timer;
        #endregion

        #region Injections
        private readonly IMapper _mapper;
        private readonly ProjectService _projectService;
        private readonly IssueService _issueService;
        private readonly TimeIntervalsService _timeIntervalsService;
        private readonly IMessageBoxHelper _messageBoxHelper;
        private readonly ILogger<MainWindowViewModel> _logger;
        private readonly Integration.Services.IssueService _integrationIssueService;
        private readonly Integration.Services.JournalService _integrationJournalService;
        private readonly CommentService _commentService;
        #endregion

        #region Commands
        public IRelayCommand SynchronizeProjectsCommand { get; }
        public IRelayCommand SynchronizeIssuesCommand { get; }
        public IAsyncRelayCommand LoadProjectsAsyncCommand { get; }
        public IRelayCommand LoadIssuesForProjectCommand { get; }
        public IRelayCommand OpenSettingsDialogCommand { get; }
        public IRelayCommand OpenDailyRaportDialogCommand { get; }
        public IRelayCommand OpenSearchWindowCommand { get; }
        public IRelayCommand OpenCreateBackupDialogCommand { get; }
        public IRelayCommand OpenRestoreBackupDialogCommand { get; }
        public IRelayCommand<ITrayable> ShowMainWindowCommand { get; }
        #endregion

        public MainWindowViewModel()
        {
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _projectService = Ioc.Default.GetRequiredService<ProjectService>();
            _issueService = Ioc.Default.GetRequiredService<IssueService>();
            _timeIntervalsService = Ioc.Default.GetRequiredService<TimeIntervalsService>();
            _messageBoxHelper = Ioc.Default.GetRequiredService<IMessageBoxHelper>();
            _logger = Ioc.Default.GetLoggerForType<MainWindowViewModel>();
            _integrationIssueService = Ioc.Default.GetRequiredService<Integration.Services.IssueService>();
            _integrationJournalService = Ioc.Default.GetRequiredService<Integration.Services.JournalService>();
            _commentService = Ioc.Default.GetRequiredService<CommentService>();

            SynchronizeProjectsCommand = new RelayCommand(ShowSynchronizeProjectDialog);
            SynchronizeIssuesCommand = new RelayCommand(SynchronizeIssuesDialog);
            LoadProjectsAsyncCommand = new AsyncRelayCommand(LoadProjectsAsync);
            OpenSettingsDialogCommand = new RelayCommand(OpenSettingsDialog);
            OpenDailyRaportDialogCommand = new RelayCommand(OpenDailyRaportDialog);
            OpenSearchWindowCommand = new RelayCommand(OpenSearchWindow);
            OpenCreateBackupDialogCommand = new RelayCommand(OpenCreateBackupDialog);
            OpenRestoreBackupDialogCommand = new RelayCommand(OpenRestoreBackupDialog);
            ShowMainWindowCommand = new RelayCommand<ITrayable>(ShowFromTray);

            WeakReferenceMessenger.Default.Register<NodeChangeMessage>(this, (r, m) =>
            {
                SetVisibility(m.Value);
            });

            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
        }

        private async void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                if (SelectedProject != null || await _timeIntervalsService.CheckIfAnyStartedTimeIntervalExistAsync())
                {
                    _timer = new Stopwatch();
                    _timer.Start();
                }
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    if (_timer.ElapsedMilliseconds > 1000 * 60 * 5 && (SelectedProject != null || await _timeIntervalsService.CheckIfAnyStartedTimeIntervalExistAsync()))
                    {
                        var dialog = new BreakReason();
                        dialog.ShowAsync();
                        WeakReferenceMessenger.Default.Send(new BreakReasonTimeMessage(_timer.Elapsed));
                        _timer.Reset();
                    }
                }
            }
        }

        private void SetVisibility(TreeModel value)
        {
            switch (value.Type)
            {
                case nameof(Data.Models.Issue):
                    ViewIssueDetails = true;
                    ViewCommentDetails = false;
                    ViewTimeIntervalList = true;
                    break;
                case nameof(Data.Models.Comment):
                    ViewIssueDetails = false;
                    ViewCommentDetails = true;
                    ViewTimeIntervalList = true;
                    break;
            }
        }

        private void OpenCreateBackupDialog()
        {
            var dialog = new CreateDatabaseBackup();
            dialog.ShowAsync();
        }

        private void OpenRestoreBackupDialog()
        {
            var dialog = new RestoreDatabaseBackup();
            dialog.ShowAsync();
        }

        public void ShowFromTray(ITrayable window)
        {
            window?.OpenFromTray();
        }


        private async Task LoadProjectsAsync()
        {
            try
            {
                Projects.Clear();
                var result = await _projectService.GetProjectsAsync();
                foreach (var item in _mapper.Map<IEnumerable<Models.Projects.ListItemModel>>(result))
                {
                    Projects.Add(item);
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("{0} {1}", nameof(LoadProjectsAsync), ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(LoadProjectsAsync), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy pobieraniu projektów");
            }
        }

        private void ShowSynchronizeProjectDialog()
        {
            var dialog = new SynchronizeProjects();
            dialog.ShowAsync();
        }

        private void SynchronizeIssuesDialog()
        {
            var dialog = new SynchronizeIssues();
            dialog.ShowAsync();
        }

        private void LoadIssuesForProjectAsync()
        {
            WeakReferenceMessenger.Default.Send<ProjectChangeMessage>(new ProjectChangeMessage(SelectedProject));
        }

        private void OpenSettingsDialog()
        {
            var dialog = new Settings();
            dialog.ShowAsync();
        }

        private void OpenDailyRaportDialog()
        {
            var dialog = new DailyRaport();
            dialog.ShowAsync();
        }

        private void OpenSearchWindow()
        {
            if (SelectedProject != null)
            {
                var dialog = new SearchWindow();
                dialog.Show();
                WeakReferenceMessenger.Default.Send(new SetSelectedProjectMessage(SelectedProject));
            }
            else
            {
                _messageBoxHelper.ShowWarningInfoBox("Najpierw należy wybrać projekt w którym nastąpi wyszukiwanie", "Brak wybranego projektu");
            }
        }
    }
}