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
                    WeakReferenceMessenger.Default.Send(new NodeChangeMessage(value));
                }
            }
        }

        private readonly IMapper _mapper;

        private readonly ProjectService _projectService;
        private readonly IssueService _issueService;

        public IAsyncRelayCommand SynchronizeProjectsCommand { get; }
        public IAsyncRelayCommand SynchronizeIssuesCommand { get; }
        public IAsyncRelayCommand LoadProjectsAsyncCommand { get; }
        public IAsyncRelayCommand LoadIssuesForProjectAsyncCommand { get; }
        public IRelayCommand OpenSettingsDialogCommand { get; }

        public MainWindowViewModel()
        {
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _projectService = Ioc.Default.GetRequiredService<ProjectService>();
            _issueService = Ioc.Default.GetRequiredService<IssueService>();

            SynchronizeProjectsCommand = new AsyncRelayCommand(SynchronizeProjects);
            SynchronizeIssuesCommand = new AsyncRelayCommand(SynchronizeIssues);
            LoadProjectsAsyncCommand = new AsyncRelayCommand(LoadProjects);
            LoadIssuesForProjectAsyncCommand = new AsyncRelayCommand(LoadIssuesForProject);
            OpenSettingsDialogCommand = new RelayCommand(OpenSettingsDialog);
        }

        private async Task LoadProjects()
        {
            var result = await _projectService.GetProjectsAsync();
            foreach (var item in _mapper.Map<IEnumerable<Models.Projects.ListItemModel>>(result))
            {
                Projects.Add(item);
            }
        }

        private async Task SynchronizeProjects()
        {
            await _projectService.SynchronizeProjects();

            await LoadProjects();
        }

        private async Task SynchronizeIssues()
        {
            await _issueService.SynchronizeIssues();
        }

        private async Task LoadIssuesForProject()
        {
            if (SelectedProject.Id > 0)
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
            var asyncoperation = dialog.ShowAsync();
        }
    }
}