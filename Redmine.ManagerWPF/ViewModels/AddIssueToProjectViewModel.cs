using System;
using System.Threading.Tasks;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Redmine.ManagerWPF.Data.Enums;
using Redmine.ManagerWPF.Data.Models;
using Redmine.ManagerWPF.Desktop.Extensions;
using Redmine.ManagerWPF.Desktop.Messages;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class AddIssueToProjectViewModel : ObservableObject
    {
        private Models.Projects.ListItemModel SelectedProject { get; set; }
        private Models.Issues.FormModel _issueFormModel;

        public Models.Issues.FormModel IssueFormModel
        {
            get => _issueFormModel;
            set => _issueFormModel = value;
        }

        public IAsyncRelayCommand SaveIssueCommand { get; }

        private readonly IssueService _issueService;
        private readonly ProjectService _projectService;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IMapper _mapper;
        private readonly ILogger<AddIssueToProjectViewModel> _logger;

        public AddIssueToProjectViewModel()
        {
            _issueService = Ioc.Default.GetRequiredService<IssueService>();
            _projectService = Ioc.Default.GetRequiredService<ProjectService>();
            _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _logger = Ioc.Default.GetLoggerForType<AddIssueToProjectViewModel>();

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
                        entity.ProjectId = project.Id;
                    }
                    else
                    {
                        _logger.LogDebug("{0} {1}", nameof(SaveIssueAsync), $"Nie znaleziono projektu o ID: {SelectedProject.Id}");
                        throw new Exception($"Nie znaleziono projektu o ID: {SelectedProject.Id}");
                    }

                    var result = await _issueService.Create(entity);

                    WeakReferenceMessenger.Default.Send(new AddIssueToProjectMessage(result));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(SaveIssueAsync), ex.Message);
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy zapisywaniu zadania");
            }
        }
    }
}