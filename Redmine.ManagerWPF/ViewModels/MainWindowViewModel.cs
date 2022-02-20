﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Data.Enums;
using Redmine.ManagerWPF.Desktop.Extensions;
using Redmine.ManagerWPF.Desktop.Helpers;
using Redmine.ManagerWPF.Desktop.Messages;
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

        private TreeModel _selectedNode;

        public TreeModel SelectedNode
        {
            get => _selectedNode;
            set
            {
                SetProperty(ref _selectedNode, value);
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
        public IAsyncRelayCommand LoadIssuesForProjectAsyncCommand { get; }
        public IRelayCommand OpenSettingsDialogCommand { get; }
        public IRelayCommand OpenDailyRaportDialogCommand { get; }
        public IRelayCommand AddIssueForProjectCommand { get; }
        public IRelayCommand OpenSearchWindowCommand { get; }
        public IRelayCommand OpenCreateBackupDialogCommand { get; }
        public IRelayCommand OpenRestoreBackupDialogCommand { get; }
        public IAsyncRelayCommand DeleteIssueFromProjectCommand { get; }
        public IAsyncRelayCommand<TreeModel> SynchronizeNodeCommand { get; }
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
            LoadProjectsAsyncCommand = new AsyncRelayCommand(LoadProjects);
            LoadIssuesForProjectAsyncCommand = new AsyncRelayCommand(LoadIssuesForProject);
            OpenSettingsDialogCommand = new RelayCommand(OpenSettingsDialog);
            OpenDailyRaportDialogCommand = new RelayCommand(OpenDailyRaportDialog);
            AddIssueForProjectCommand = new RelayCommand(OpenAddIsuueToProjectDialog);
            OpenSearchWindowCommand = new RelayCommand(OpenSearchWindow);
            OpenCreateBackupDialogCommand = new RelayCommand(OpenCreateBackupDialog);
            OpenRestoreBackupDialogCommand = new RelayCommand(OpenRestoreBackupDialog);
            DeleteIssueFromProjectCommand = new AsyncRelayCommand(DeleteIsuueFromProject);
            ShowMainWindowCommand = new RelayCommand<ITrayable>(ShowFromTray);
            SynchronizeNodeCommand = new AsyncRelayCommand<TreeModel>(SynchronizeNode);

            WeakReferenceMessenger.Default.Register<ChangeSelectedIssueDoneStatus>(this, (r, m) =>
            {
                ChangeSelectedAsDone(m.Value);
            });

            WeakReferenceMessenger.Default.Register<ChangeSelectedCommentDoneStatus>(this, (r, m) =>
            {
                ChangeSelectedAsDone(m.Value);
            });

            WeakReferenceMessenger.Default.Register<AddIssueToProjectMessage>(this, (r, m) =>
            {
                AddToTreeView(m.Value);
            });

            WeakReferenceMessenger.Default.Register<NodeChangeMessage>(this, (r, m) =>
            {
                SetVisibility(m.Value);
            });
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
            SelectedNode = value;
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

        private void AddToTreeView(Data.Models.Issue value)
        {
            if (value == null) return;
            var issue = _mapper.Map<Models.Tree.TreeModel>(value);
            Issues.Add(issue);
        }

        private void ChangeSelectedAsDone(TreeModel value)
        {
            if (SelectedNode.Id == value.Id && SelectedNode.Type == value.Type)
            {
                SelectedNode.Done = value.Done;
            }
        }

        private async Task LoadProjects()
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
                _logger.LogError("{0} {1}", nameof(LoadProjects), ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(LoadProjects), ex.Message);
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

        private async Task LoadIssuesForProject()
        {
            try
            {
                if (SelectedProject != null && SelectedProject.Id > 0)
                {
                    Issues.Clear();
                    var result = await _issueService.GetIssuesByProjectIdAsync(SelectedProject.Id);
                    foreach (var item in _mapper.Map<IEnumerable<Models.Tree.TreeModel>>(result))
                    {
                        Issues.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(LoadIssuesForProject), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy pobieraniu listy zadań");
            }
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
                WeakReferenceMessenger.Default.Send(new SelectedProjectMessage(SelectedProject));
            }
            else
            {
                _messageBoxHelper.ShowWarningInfoBox("Najpierw należy wybrać projekt w którym nastąpi wyszukiwanie", "Brak wybranego projektu");
            }
        }

        private void OpenAddIsuueToProjectDialog()
        {
            if (SelectedProject != null)
            {
                var dialog = new AddIssueToProject();
                dialog.ShowAsync();
                WeakReferenceMessenger.Default.Send(new SelectedProjectMessage(SelectedProject));
            }
            else
            {
                _messageBoxHelper.ShowWarningInfoBox("By dodać zadanie pierwsze wybierz projekt", "Brak wybranego projektu");
            }
        }

        private async Task DeleteIsuueFromProject()
        {
            try
            {
                if (SelectedNode != null && SelectedNode.Type == nameof(ObjectType.Issue))
                {
                    var result = _messageBoxHelper.ShowConfirmationBox("Czy na pewno chcesz usunąć zaznaczone zadanie?", "Uwaga");
                    if (result)
                    {
                        var timeIntervalsForSelectedIssue = await _timeIntervalsService.GetTimeIntervalsForIssueAsync(SelectedNode.Id);
                        foreach (var timeInterval in timeIntervalsForSelectedIssue)
                        {
                            await _timeIntervalsService.DeleteAsync(timeInterval);
                        }

                        var issue = await _issueService.GetIssueAsync(SelectedNode.Id);
                        if (issue != null)
                        {
                            await _issueService.Delete(issue);
                        }

                        var nodeToSearch = SelectedNode;
                        SelectedNode = null;

                        var isDeleted = DeleteNodeFromTree(nodeToSearch, Issues);
                        if (isDeleted)
                        {
                            _messageBoxHelper.ShowInformationBox("Pomyślnie usunięto zadanie", "Usuwanie zakończone powodzeniem");
                        }
                    }
                }
                else
                {
                    _messageBoxHelper.ShowWarningInfoBox("Nie wybrano zadania do usunięcia", "Brak wybranego projektu");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(DeleteIsuueFromProject), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Brak wybranego projektu");
            }
        }

        private bool DeleteNodeFromTree(TreeModel item, ObservableCollection<TreeModel> treeStructure)
        {
            if (treeStructure.Any(x => x.Id == item.Id && x.Type == item.Type))
            {
                return treeStructure.Remove(item);
            }
            else
            {
                foreach (var node in treeStructure)
                {
                    var result = DeleteNodeFromTree(item, node.Children);

                    if (result)
                    {
                        return result;
                    }
                }
            }

            return false;
        }

        private async Task SynchronizeNode(TreeModel node)
        {
            switch (node.Type)
            {
                case nameof(ObjectType.Comment):
                    await SynchronizeComment(node.Id);
                    break;
                case nameof(ObjectType.Issue):
                    await SynchronizeIssue(node.Id);
                    break;
            }

            if (SelectedNode?.Id == node.Id && SelectedNode?.Type == node.Type)
            {
                WeakReferenceMessenger.Default.Send(new NodeChangeMessage(node));
            }
        }

        private async Task SynchronizeIssue(int issueId)
        {
            try
            {
                var issue = await _issueService.GetIssueAsync(issueId);

                if (issue == null)
                {
                    _messageBoxHelper.ShowWarningInfoBox("Nie znaleziono zadania w bazie", "Błąd");
                    return;
                }

                var redmineIssue = await _integrationIssueService.GetIssue(issue.SourceId);
                if (redmineIssue == null)
                {
                    _messageBoxHelper.ShowWarningInfoBox("Nie znaleziono zadania w Redmine", "Błąd");
                    return;
                }

                redmineIssue.Comments = await _integrationJournalService.GetIssueJournals(redmineIssue);

                await _issueService.SynchronizeIssues(redmineIssue);

                await LoadIssuesForProject();

                _messageBoxHelper.ShowInformationBox("Pomyślnie zsynchronizowano", "Sukces");
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(SynchronizeIssue), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(String.Format("Nie udało się zsynchronizować zadania. {0}", ex.Message), "Błąd");
            }
        }

        private async Task SynchronizeComment(int commentId)
        {

            var comment = await _commentService.GetCommentAsync(commentId);

            if (comment == null)
            {
                _messageBoxHelper.ShowWarningInfoBox("Nie znaleziono komentarza w bazie", "Błąd");
                return;
            }

            await SynchronizeIssue(comment.IssueId);
        }
    }
}