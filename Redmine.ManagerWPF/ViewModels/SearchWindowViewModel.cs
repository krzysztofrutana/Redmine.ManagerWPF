using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Redmine.ManagerWPF.Desktop.Extensions;
using Redmine.ManagerWPF.Desktop.Helpers;
using Redmine.ManagerWPF.Desktop.Messages;
using Redmine.ManagerWPF.Desktop.Models.Projects;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class SearchWindowViewModel : ObservableRecipient
    {
        #region Properties
        public ObservableCollection<Models.Tree.TreeModel> Issues { get; private set; } = new ExtendedObservableCollection<Models.Tree.TreeModel>();

        public Models.Projects.ListItemModel Project { get => _project; set => SetProperty(ref _project, value); }

        private Models.Tree.TreeModel _selectedNode;

        public Models.Tree.TreeModel SelectedNode
        {
            get => _selectedNode;
            set
            {
                SetProperty(ref _selectedNode, value);
                if (value == null) return;
                switch (value.Type)
                {
                    case nameof(Data.Models.Issue):
                        ViewIssueDetails = true;
                        ViewCommentDetails = false;
                        break;
                    case nameof(Data.Models.Comment):
                        ViewIssueDetails = false;
                        ViewCommentDetails = true;
                        break;
                }

                WeakReferenceMessenger.Default.Send(new SearchNodeChangeMessage(value));
            }
        }

        private string _searchText;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    SetProperty(ref _searchText, value);
                    SearchIssues();
                }
                else
                {
                    Issues.Clear();
                }
            }
        }

        private bool _viewIssueDetails;

        public bool ViewIssueDetails
        {
            get => _viewIssueDetails;
            set => SetProperty(ref _viewIssueDetails, value);
        }

        private bool _viewCommentDetails;
        private ListItemModel _project;

        public bool ViewCommentDetails
        {
            get => _viewCommentDetails;
            set => SetProperty(ref _viewCommentDetails, value);
        }
        #endregion

        #region Injections
        private readonly IMapper _mapper;
        private readonly IssueService _issueService;
        private readonly IMessageBoxHelper _messageBoxHelper;
        private readonly ILogger<SearchWindowViewModel> _logger;
        #endregion

        public SearchWindowViewModel()
        {
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _issueService = Ioc.Default.GetRequiredService<IssueService>();
            _messageBoxHelper = Ioc.Default.GetRequiredService<IMessageBoxHelper>();
            _logger = Ioc.Default.GetLoggerForType<SearchWindowViewModel>();

            WeakReferenceMessenger.Default.Register<SelectedProjectMessage>(this, (r, m) =>
            {
                if (m.Value != null)
                {
                    Project = m.Value;
                }
            });
        }

        private async void SearchIssues()
        {
            try
            {
                Issues.Clear();

                var result = await _issueService.SearchInIssuesAndComments(SearchText, Project.Id);

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