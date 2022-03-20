using System;
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
using Redmine.ManagerWPF.Data.Enums;
using Redmine.ManagerWPF.Desktop.Extensions;
using Redmine.ManagerWPF.Desktop.Helpers;
using Redmine.ManagerWPF.Desktop.Messages.ControlButtonsMainWindow;
using Redmine.ManagerWPF.Desktop.Messages.Forms;
using Redmine.ManagerWPF.Desktop.Messages.MainWindowTreeView;
using Redmine.ManagerWPF.Desktop.Messages.ProjectCombobox;
using Redmine.ManagerWPF.Desktop.Messages.RemoveNodeFromProjectMessage;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class MainWindowTreeViewViewModel : ObservableRecipient
    {
        #region Properties
        public ObservableCollection<Models.Tree.TreeModel> Issues { get; private set; } = new ExtendedObservableCollection<Models.Tree.TreeModel>();

        public Models.Projects.ListItemModel SelectedProject { get; set; }

        private TreeModel _selectedNode;

        public TreeModel SelectedNode
        {
            get => _selectedNode;
            set
            {
                SetProperty(ref _selectedNode, value);
                if (value == null) return;

                WeakReferenceMessenger.Default.Send(new NodeChangeMessage(value));
            }
        }

        private string _searchText;

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                if (!string.IsNullOrEmpty(value))
                {
                    if (SelectedProject != null)
                        SearchIssues();
                }
                else
                {
                    if (SelectedProject != null)
                        LoadIssuesForProjectAsync();
                }
            }
        }

        #endregion

        #region Injections
        private readonly IMapper _mapper;
        private readonly IssueService _issueService;
        private readonly CommentService _commentService;
        private readonly IMessageBoxHelper _messageBoxHelper;
        private readonly ILogger<MainWindowViewModel> _logger;
        private readonly TimeIntervalsService _timeIntervalsService;
        private readonly Integration.Services.IssueService _integrationIssueService;
        private readonly Integration.Services.JournalService _integrationJournalService;
        #endregion

        #region Commands
        public IAsyncRelayCommand LoadIssuesForProjectAsyncCommand { get; }
        public IAsyncRelayCommand<TreeModel> SynchronizeNodeCommand { get; }
        #endregion

        public MainWindowTreeViewViewModel()
        {
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _timeIntervalsService = Ioc.Default.GetRequiredService<TimeIntervalsService>();
            _issueService = Ioc.Default.GetRequiredService<IssueService>();
            _commentService = Ioc.Default.GetRequiredService<CommentService>();
            _messageBoxHelper = Ioc.Default.GetRequiredService<IMessageBoxHelper>();
            _logger = Ioc.Default.GetLoggerForType<MainWindowViewModel>();
            _integrationIssueService = Ioc.Default.GetRequiredService<Integration.Services.IssueService>();
            _integrationJournalService = Ioc.Default.GetRequiredService<Integration.Services.JournalService>();

            LoadIssuesForProjectAsyncCommand = new AsyncRelayCommand(LoadIssuesForProjectAsync);
            SynchronizeNodeCommand = new AsyncRelayCommand<TreeModel>(SynchronizeNodeAsync);

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

            WeakReferenceMessenger.Default.Register<RemoveNodeFromProjectMessage>(this, (r, m) =>
            {
                DeleteNodeFromProject(m.Value);
            });

            WeakReferenceMessenger.Default.Register<ProjectChangeMessage>(this, (r, m) =>
            {
                if (m.Value != null)
                {
                    SelectedProject = m.Value;
                    LoadIssuesForProjectAsync();
                }

            });

            WeakReferenceMessenger.Default.Register<SetToActuaMessage>(this, (r, m) =>
            {
                if (m.Value != null)
                {
                    SelectedNode = m.Value;
                }
            });

            WeakReferenceMessenger.Default.Register<ReloadTreeMessage>(this, (r, m) =>
            {
                LoadIssuesForProjectAsync();
            });
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

        private async Task LoadIssuesForProjectAsync()
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
                _logger.LogError("{0} {1}", nameof(LoadIssuesForProjectAsync), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy pobieraniu listy zadań");
            }
        }

        private void DeleteNodeFromProject(TreeModel value)
        {
            try
            {
                if (value.Id == SelectedNode.Id && value.Type == SelectedNode.Type)
                {
                    SelectedNode = null;
                }

                var isDeleted = DeleteNodeFromTree(value, Issues);
                if (isDeleted)
                {
                    _messageBoxHelper.ShowInformationBox("Pomyślnie usunięto zadanie", "Usuwanie zakończone powodzeniem");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(DeleteNodeFromProject), ex.Message);
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

        private async Task SynchronizeNodeAsync(TreeModel node)
        {
            switch (node.Type)
            {
                case nameof(ObjectType.Comment):
                    await SynchronizeCommentAsync(node.Id);
                    break;
                case nameof(ObjectType.Issue):
                    await SynchronizeIssueAsync(node.Id);
                    break;
            }

            if (SelectedNode?.Id == node.Id && SelectedNode?.Type == node.Type)
            {
                WeakReferenceMessenger.Default.Send(new NodeChangeMessage(node));
            }
        }

        private async Task SynchronizeIssueAsync(int issueId)
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

                await LoadIssuesForProjectAsync();

                _messageBoxHelper.ShowInformationBox("Pomyślnie zsynchronizowano", "Sukces");
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(SynchronizeIssueAsync), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(String.Format("Nie udało się zsynchronizować zadania. {0}", ex.Message), "Błąd");
            }
        }

        private async Task SynchronizeCommentAsync(int commentId)
        {

            var comment = await _commentService.GetCommentAsync(commentId);

            if (comment == null)
            {
                _messageBoxHelper.ShowWarningInfoBox("Nie znaleziono komentarza w bazie", "Błąd");
                return;
            }

            await SynchronizeIssueAsync(comment.IssueId);
        }

        private async void SearchIssues()
        {
            try
            {
                Issues.Clear();

                var result = await _issueService.SearchInIssuesAndComments(SearchText, SelectedProject.Id);

                foreach (var item in _mapper.Map<IEnumerable<Models.Tree.TreeModel>>(result))
                {
                    Issues.Add(item);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(SearchIssues), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy pobieraniu zadań");
            }
        }
    }
}