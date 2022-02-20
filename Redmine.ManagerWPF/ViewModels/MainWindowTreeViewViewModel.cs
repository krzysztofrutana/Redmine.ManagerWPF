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

            LoadIssuesForProjectAsyncCommand = new AsyncRelayCommand(LoadIssuesForProject);
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

            WeakReferenceMessenger.Default.Register<ProjectChangeMessage>(this, (r, m) =>
            {
                if (m.Value != null)
                {
                    SelectedProject = m.Value;
                    LoadIssuesForProject();
                }

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