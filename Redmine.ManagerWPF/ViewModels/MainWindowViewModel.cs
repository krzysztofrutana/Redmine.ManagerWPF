using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Redmine.ManagerWPF.Desktop.Messages;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Desktop.Views.ContentDialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class MainWindowViewModel : ObservableRecipient
    {
        public ObservableCollection<Models.Projects.ListItemModel> Projects { get; private set; } = new ObservableCollection<Models.Projects.ListItemModel>();
        public ObservableCollection<Models.Tree.TreeModel> Issues { get; private set; } = new ObservableCollection<Models.Tree.TreeModel>();

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

        public IRelayCommand SynchronizeProjectsCommand { get; }
        public IRelayCommand SynchronizeIssuesCommand { get; }
        public IAsyncRelayCommand LoadProjectsAsyncCommand { get; }
        public IAsyncRelayCommand LoadIssuesForProjectAsyncCommand { get; }
        public IRelayCommand OpenSettingsDialogCommand { get; }
        public IRelayCommand OpenDailyRaportDialogCommand { get; }

        public MainWindowViewModel()
        {
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _projectService = Ioc.Default.GetRequiredService<ProjectService>();
            _issueService = Ioc.Default.GetRequiredService<IssueService>();

            SynchronizeProjectsCommand = new RelayCommand(ShowSynchronizeProjectDialog);
            SynchronizeIssuesCommand = new RelayCommand(SynchronizeIssuesDialog);
            LoadProjectsAsyncCommand = new AsyncRelayCommand(LoadProjects);
            LoadIssuesForProjectAsyncCommand = new AsyncRelayCommand(LoadIssuesForProject);
            OpenSettingsDialogCommand = new RelayCommand(OpenSettingsDialog);
            OpenDailyRaportDialogCommand = new RelayCommand(OpenDailyRaportDialog);
        }

        private async Task LoadProjects()
        {
            Projects.Clear();
            var result = await _projectService.GetProjectsAsync();
            foreach (var item in _mapper.Map<IEnumerable<Models.Projects.ListItemModel>>(result))
            {
                Projects.Add(item);
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
    }
}