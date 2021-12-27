using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Redmine.ManagerWPF.Data.Enums;
using Redmine.ManagerWPF.Desktop.Helpers;
using Redmine.ManagerWPF.Desktop.Messages;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Desktop.Views.ContentDialogs;
using Redmine.ManagerWPF.Desktop.Views.Windows;
using Redmine.ManagerWPF.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class MainWindowViewModel : ObservableRecipient
    {
        public ObservableCollection<Models.Projects.ListItemModel> Projects { get; private set; } = new ObservableCollection<Models.Projects.ListItemModel>();
        public ObservableCollection<Models.Tree.TreeModel> Issues { get; private set; } = new AsyncObservableCollection<Models.Tree.TreeModel>();

        private Models.Projects.ListItemModel _selectedProject;

        public Models.Projects.ListItemModel SelectedProject
        {
            get { return _selectedProject; }
            set
            {
                SetProperty(ref _selectedProject, value);

                if (value != null)
                {
                    WeakReferenceMessenger.Default.Send(new ProjectChangeMessage(value));
                }

                ViewProjectDetails = value != null;
            }
        }

        private Models.Tree.TreeModel _selectedNode;

        public Models.Tree.TreeModel SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                SetProperty(ref _selectedNode, value);
                if (value != null)
                {
                    if (value.Type == nameof(Data.Models.Issue))
                    {
                        ViewIssueDetails = true;
                        ViewCommentDetails = false;
                        ViewTimeIntervalList = true;
                    }

                    if (value.Type == nameof(Data.Models.Comment))
                    {
                        ViewIssueDetails = false;
                        ViewCommentDetails = true;
                        ViewTimeIntervalList = true;
                    }

                    WeakReferenceMessenger.Default.Send(new NodeChangeMessage(value));
                }
            }
        }

        private bool _viewProjectDetails;

        public bool ViewProjectDetails
        {
            get { return _viewProjectDetails; }
            set { SetProperty(ref _viewProjectDetails, value); }
        }

        private bool _viewIssueDetails;

        public bool ViewIssueDetails
        {
            get { return _viewIssueDetails; }
            set { SetProperty(ref _viewIssueDetails, value); }
        }

        private bool _viewCommentDetails;

        public bool ViewCommentDetails
        {
            get { return _viewCommentDetails; }
            set { SetProperty(ref _viewCommentDetails, value); }
        }

        private bool _viewTimeIntervalList;

        public bool ViewTimeIntervalList
        {
            get { return _viewTimeIntervalList; }
            set { SetProperty(ref _viewTimeIntervalList, value); }
        }

        private readonly IMapper _mapper;

        private readonly ProjectService _projectService;
        private readonly IssueService _issueService;
        private readonly TimeIntervalsService _timeIntervalsService;
        private readonly IMessageBoxService _messageBoxService;

        public IRelayCommand SynchronizeProjectsCommand { get; }
        public IRelayCommand SynchronizeIssuesCommand { get; }
        public IAsyncRelayCommand LoadProjectsAsyncCommand { get; }
        public IAsyncRelayCommand LoadIssuesForProjectAsyncCommand { get; }
        public IRelayCommand OpenSettingsDialogCommand { get; }
        public IRelayCommand OpenDailyRaportDialogCommand { get; }
        public IRelayCommand AddIssueForProjectCommand { get; }
        public IRelayCommand OpenSearchWindowCommand { get; }
        public IAsyncRelayCommand DeleteIssueFromProjectCommand { get; }

        public MainWindowViewModel()
        {
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _projectService = Ioc.Default.GetRequiredService<ProjectService>();
            _issueService = Ioc.Default.GetRequiredService<IssueService>();
            _timeIntervalsService = Ioc.Default.GetRequiredService<TimeIntervalsService>();
            _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();

            SynchronizeProjectsCommand = new RelayCommand(ShowSynchronizeProjectDialog);
            SynchronizeIssuesCommand = new RelayCommand(SynchronizeIssuesDialog);
            LoadProjectsAsyncCommand = new AsyncRelayCommand(LoadProjects);
            LoadIssuesForProjectAsyncCommand = new AsyncRelayCommand(LoadIssuesForProject);
            OpenSettingsDialogCommand = new RelayCommand(OpenSettingsDialog);
            OpenDailyRaportDialogCommand = new RelayCommand(OpenDailyRaportDialog);
            AddIssueForProjectCommand = new RelayCommand(OpenAddIsuueToProjectDialog);
            OpenSearchWindowCommand = new RelayCommand(OpenSearchWindow);
            DeleteIssueFromProjectCommand = new AsyncRelayCommand(DeleteIsuueFromProject);

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
        }

        private void AddToTreeView(Data.Models.Issue value)
        {
            if (value != null)
            {
                var issue = _mapper.Map<Models.Tree.TreeModel>(value);
                Issues.Add(issue);
            }
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
            catch (System.Exception ex)
            {
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy pobieraniu projektów");
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
            catch (System.Exception ex)
            {
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy pobieraniu listy zadań");
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
                _messageBoxService.ShowWarningInfoBox("Najpierw należy wybrać projekt w którym nastąpi wyszukiwanie", "Brak wybranego projektu");
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
                _messageBoxService.ShowWarningInfoBox("By dodać zadanie pierwsze wybierz projekt", "Brak wybranego projektu");
            }
        }

        private async Task DeleteIsuueFromProject()
        {
            try
            {
                if (SelectedNode != null && SelectedNode.Type == nameof(ObjectType.Issue))
                {
                    var result = _messageBoxService.ShowConfirmationBox("Czy na pewno chcesz usunąć zaznaczone zadanie?", "Uwaga");
                    if (result)
                    {
                        var timeIntervalsForSelectedIssue = await _timeIntervalsService.GetTimeIntervalsForIssueAsync(SelectedNode.Id);
                        foreach (var timeInterval in timeIntervalsForSelectedIssue)
                        {
                            await _timeIntervalsService.DeleteAsync(timeInterval);
                        }

                        var issue = _issueService.GetIssue(SelectedNode.Id);
                        if (issue != null)
                        {
                            await _issueService.Delete(issue);
                        }

                        var nodeToSearch = SelectedNode;
                        SelectedNode = null;

                        var isDeleted = DeleteNodeFromTree(nodeToSearch, Issues);
                        if (isDeleted)
                        {
                            _messageBoxService.ShowInformationBox("Pomyślnie usunięto zadanie", "Usuwanie zakończone powodzeniem");
                        }
                    }
                }
                else
                {
                    _messageBoxService.ShowWarningInfoBox("Nie wybrano zadania do usunięcia", "Brak wybranego projektu");
                }
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Brak wybranego projektu");
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
    }
}