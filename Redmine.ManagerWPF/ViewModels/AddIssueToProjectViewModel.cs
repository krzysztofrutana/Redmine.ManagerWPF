using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Redmine.ManagerWPF.Data.Enums;
using Redmine.ManagerWPF.Data.Models;
using Redmine.ManagerWPF.Desktop.Messages;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class AddIssueToProjectViewModel : ObservableObject
    {
        public Models.Projects.ListItemModel SelectedProject { get; set; }
        private Models.Issues.FormModel _issueFormModel;

        public Models.Issues.FormModel IssueFormModel
        {
            get { return _issueFormModel; }
            set { _issueFormModel = value; }
        }

        public IAsyncRelayCommand SaveIssueCommand { get; }

        private readonly IssueService _issueService;
        private readonly ProjectService _projectService;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IMapper _mapper;

        public AddIssueToProjectViewModel()
        {
            _issueService = Ioc.Default.GetRequiredService<IssueService>();
            _projectService = Ioc.Default.GetRequiredService<ProjectService>();
            _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();
            _mapper = Ioc.Default.GetRequiredService<IMapper>();

            IssueFormModel = new Models.Issues.FormModel();

            SaveIssueCommand = new AsyncRelayCommand(SaveIssueAsync);

            WeakReferenceMessenger.Default.Register<SelectedProjectMessage>(this, (r, m) =>
            {
                if (m.Value != null)
                {
                    SelectedProject = m.Value;
                }
            });
        }

        private async Task SaveIssueAsync()
        {
            try
            {
                if (SelectedProject != null)
                {
                    var entity = _mapper.Map<Issue>(IssueFormModel);
                    entity.Status = nameof(IssueInternalStatus.Created);

                    var project = await _projectService.GetProjectAsync(SelectedProject.Id);
                    if (project != null)
                    {
                        entity.Project = project;
                    }
                    else
                    {
                        throw new Exception(String.Format("Nie znaleziono projektu o ID: {0}", SelectedProject.Id));
                    }

                    var result = await _issueService.Create(entity);

                    WeakReferenceMessenger.Default.Send(new AddIssueToProjectMessage(result));
                }
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy zapisywaniu zadania");
            }
        }
    }
}