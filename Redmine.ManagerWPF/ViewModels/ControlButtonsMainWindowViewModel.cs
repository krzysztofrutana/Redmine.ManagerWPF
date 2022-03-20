using System;
using System.Threading.Tasks;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Redmine.ManagerWPF.Data.Enums;
using Redmine.ManagerWPF.Desktop.Extensions;
using Redmine.ManagerWPF.Desktop.Messages.ControlButtonsMainWindow;
using Redmine.ManagerWPF.Desktop.Messages.MainWindowTreeView;
using Redmine.ManagerWPF.Desktop.Messages.ProjectCombobox;
using Redmine.ManagerWPF.Desktop.Messages.RemoveNodeFromProjectMessage;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Desktop.Views.ContentDialogs;
using Redmine.ManagerWPF.Helpers.Interfaces;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class ControlButtonsMainWindowViewModel : ObservableRecipient
    {
        #region Properties

        private Models.Projects.ListItemModel _selectedProject;
        public Models.Projects.ListItemModel SelectedProject
        {
            get => _selectedProject;
            set
            {
                SetProperty(ref _selectedProject, value);
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
        #endregion

        #region Injections
        private readonly IMapper _mapper;
        private readonly IssueService _issueService;
        private readonly TimeIntervalsService _timeIntervalsService;
        private readonly IMessageBoxHelper _messageBoxHelper;
        private readonly ILogger<ControlButtonsMainWindowViewModel> _logger;
        private readonly CommentService _commentService;
        #endregion

        #region Commands
        public IRelayCommand AddIssueForProjectCommand { get; }
        public IAsyncRelayCommand DeleteIssueFromProjectCommand { get; }
        public IAsyncRelayCommand GoToActualCommand { get; }
        #endregion

        public ControlButtonsMainWindowViewModel()
        {
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _issueService = Ioc.Default.GetRequiredService<IssueService>();
            _timeIntervalsService = Ioc.Default.GetRequiredService<TimeIntervalsService>();
            _messageBoxHelper = Ioc.Default.GetRequiredService<IMessageBoxHelper>();
            _logger = Ioc.Default.GetLoggerForType<ControlButtonsMainWindowViewModel>();
            _commentService = Ioc.Default.GetRequiredService<CommentService>();


            AddIssueForProjectCommand = new RelayCommand(OpenAddIsuueToProjectDialog);
            DeleteIssueFromProjectCommand = new AsyncRelayCommand(DeleteIsuueFromProjectAsync);
            GoToActualCommand = new AsyncRelayCommand(GoToActualAsync);

            WeakReferenceMessenger.Default.Register<SetSelectedProjectMessage>(this, (r, m) =>
            {
                if (m.Value != null)
                {
                    SelectedProject = m.Value;
                }
            });

            WeakReferenceMessenger.Default.Register<ProjectChangeMessage>(this, (r, m) =>
            {
                if (m.Value != null)
                {
                    SelectedProject = m.Value;
                }
            });

            WeakReferenceMessenger.Default.Register<NodeChangeMessage>(this, (r, m) =>
            {
                if (m.Value != null)
                {
                    SelectedNode = m.Value;
                }
            });
        }

        private void OpenAddIsuueToProjectDialog()
        {
            if (SelectedProject != null)
            {
                var dialog = new AddIssueToProject();
                dialog.ShowAsync();
                WeakReferenceMessenger.Default.Send(new SetSelectedProjectMessage(SelectedProject));
            }
            else
            {
                _messageBoxHelper.ShowWarningInfoBox("By dodać zadanie pierwsze wybierz projekt", "Brak wybranego projektu");
            }
        }

        private async Task DeleteIsuueFromProjectAsync()
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
                        WeakReferenceMessenger.Default.Send(new RemoveNodeFromProjectMessage(nodeToSearch));
                        SelectedNode = null;
                    }
                }
                else
                {
                    _messageBoxHelper.ShowWarningInfoBox("Nie wybrano zadania do usunięcia", "Brak wybranego zadania");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(DeleteIsuueFromProjectAsync), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Brak wybranego zadania");
            }
        }

        private async Task GoToActualAsync()
        {
            var actual = await _timeIntervalsService.GetActualAsync();
            if (actual is null)
            {
                _messageBoxHelper.ShowInformationBox("Żadne zadanie nie zostało rozpoczęte", "Informacja");
                return;
            }
            else
            {
                if (actual.CommentId.HasValue && !actual.IssueId.HasValue)
                {
                    SelectedNode = _mapper.Map<TreeModel>(actual.Comment);
                }
                else if (!actual.CommentId.HasValue && actual.IssueId.HasValue)
                {
                    SelectedNode = _mapper.Map<TreeModel>(actual.Issue);
                }

                WeakReferenceMessenger.Default.Send(new SetToActuaMessage(SelectedNode));
            }
        }
    }
}