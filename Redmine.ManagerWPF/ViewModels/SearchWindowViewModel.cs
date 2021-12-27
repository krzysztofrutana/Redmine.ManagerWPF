﻿using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Redmine.ManagerWPF.Desktop.Helpers;
using Redmine.ManagerWPF.Desktop.Messages;
using Redmine.ManagerWPF.Desktop.Models.Projects;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class SearchWindowViewModel : ObservableRecipient
    {
        public ObservableCollection<Models.Tree.TreeModel> Issues { get; private set; } = new AsyncObservableCollection<Models.Tree.TreeModel>();

        public Models.Projects.ListItemModel Project { get => _project; set => SetProperty(ref _project, value); }

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
                    }

                    if (value.Type == nameof(Data.Models.Comment))
                    {
                        ViewIssueDetails = false;
                        ViewCommentDetails = true;
                    }

                    WeakReferenceMessenger.Default.Send(new SearchNodeChangeMessage(value));
                }
            }
        }

        private string _searchText;

        public string SearchText
        {
            get { return _searchText; }
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
            get { return _viewIssueDetails; }
            set { SetProperty(ref _viewIssueDetails, value); }
        }

        private bool _viewCommentDetails;
        private ListItemModel _project;

        public bool ViewCommentDetails
        {
            get { return _viewCommentDetails; }
            set { SetProperty(ref _viewCommentDetails, value); }
        }

        private readonly IMapper _mapper;

        private readonly IssueService _issueService;
        private readonly IMessageBoxService _messageBoxService;

        public SearchWindowViewModel()
        {
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _issueService = Ioc.Default.GetRequiredService<IssueService>();
            _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();

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
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy pobieraniu zadań");
            }
        }
    }
}