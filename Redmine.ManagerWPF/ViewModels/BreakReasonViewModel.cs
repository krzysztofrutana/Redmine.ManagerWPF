using System;
using System.Threading.Tasks;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Data.Models;
using Redmine.ManagerWPF.Desktop.Extensions;
using Redmine.ManagerWPF.Desktop.Messages.BreakReason;
using Redmine.ManagerWPF.Desktop.Messages.ControlButtonsMainWindow;
using Redmine.ManagerWPF.Desktop.Messages.ProjectCombobox;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class BreakReasonViewModel : ObservableObject
    {
        #region Properties
        private TimeSpan _elapsedTime;
        private bool _otherReasonCheckBox;
        private string _elapsedTimeString;

        public TimeSpan ElapsedTime { get => _elapsedTime; set => SetProperty(ref _elapsedTime, value); }
        public string ElapsedTimeString { get => _elapsedTimeString; set => SetProperty(ref _elapsedTimeString, value); }
        public bool OtherReasonCheckBox { get => _otherReasonCheckBox; set => SetProperty(ref _otherReasonCheckBox, value); }
        public string OtherReason { get; set; }
        public Models.Projects.ListItemModel SelectedProject { get; set; }
        #endregion

        #region Injections
        private readonly IssueService _issueService;
        private readonly ProjectService _projectService;
        private readonly TimeIntervalsService _timeIntervalService;
        private readonly IMessageBoxHelper _messageBoxHelper;
        private readonly IMapper _mapper;
        private readonly ILogger<BreakReasonViewModel> _logger;
        #endregion

        #region Commands
        public IAsyncRelayCommand<ICloseable> OtherReasonCommand { get; }
        public IAsyncRelayCommand<ICloseable> BreakCommand { get; }
        #endregion

        public BreakReasonViewModel()
        {
            _issueService = Ioc.Default.GetRequiredService<IssueService>();
            _projectService = Ioc.Default.GetRequiredService<ProjectService>();
            _timeIntervalService = Ioc.Default.GetRequiredService<TimeIntervalsService>();
            _messageBoxHelper = Ioc.Default.GetRequiredService<IMessageBoxHelper>();
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _logger = Ioc.Default.GetLoggerForType<BreakReasonViewModel>();


            OtherReasonCommand = new AsyncRelayCommand<ICloseable>(OtherReasonAsync);
            BreakCommand = new AsyncRelayCommand<ICloseable>(BreakAsync);

            WeakReferenceMessenger.Default.Register<BreakReasonTimeMessage>(this, (r, m) =>
            {
                if (m.Value != null)
                {
                    ElapsedTime = m.Value;
                    ElapsedTimeString = $"Czas: {ElapsedTime.Hours} h  {ElapsedTime.Minutes} m  {ElapsedTime.Seconds} s";
                }
            });

            WeakReferenceMessenger.Default.Register<ProjectChangeMessage>(this, (r, m) =>
            {
                if (m.Value != null)
                {
                    SelectedProject = m.Value;
                }

            });
        }
        private async Task BreakAsync(ICloseable window)
        {
            await AddTimeInterval(window, "Przerwa");
        }

        private async Task OtherReasonAsync(ICloseable window)
        {
            if (string.IsNullOrWhiteSpace(OtherReason))
            {
                OtherReason = "Inny powód";
            }

            await AddTimeInterval(window, OtherReason);

        }

        private async Task AddTimeInterval(ICloseable window, string note = "")
        {
            try
            {
                var actualTimeInterval = await _timeIntervalService.GetActualAsync();
                if (SelectedProject != null || actualTimeInterval != null)
                {
                    int? projectId = null;
                    if (actualTimeInterval != null)
                    {
                        if (actualTimeInterval.Comment != null)
                        {
                            var issueForComment = await _issueService.GetIssueAsync(actualTimeInterval.Comment.IssueId);
                            if (issueForComment != null)
                            {
                                projectId = issueForComment.ProjectId;
                            }
                        }
                        else if (actualTimeInterval.Issue != null)
                        {
                            projectId = actualTimeInterval.Issue.ProjectId;
                        }
                    }
                    else if (SelectedProject != null)
                    {
                        projectId = SelectedProject.Id;
                    }

                    if (projectId.HasValue && ElapsedTime != null)
                    {
                        var issueForBreak = await _issueService.GetIssueByProjectAndNameAsync(projectId.Value, "Przerwy");

                        if (issueForBreak == null)
                        {
                            var newIssueForBreak = new Issue();
                            newIssueForBreak.SourceId = -1;
                            newIssueForBreak.Name = "Przerwy";
                            newIssueForBreak.Description = "Przechwycone przerwy w wykonywaniu zadania";
                            newIssueForBreak.ProjectId = projectId.Value;
                            newIssueForBreak.Status = "New";

                            try
                            {
                                issueForBreak = await _issueService.Create(newIssueForBreak);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError("{0} {1}", nameof(AddTimeInterval), ex.Message);
                                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy dodawaniu zadania dla przerw");
                            }
                        }

                        if (issueForBreak != null)
                        {
                            var timeIntervalEnd = DateTime.Now;
                            var timeIntervalStart = timeIntervalEnd.AddMilliseconds(-ElapsedTime.TotalMilliseconds);
                            var newTimeInterval = new TimeInterval();
                            newTimeInterval.TimeIntervalStart = timeIntervalStart;
                            newTimeInterval.TimeIntervalEnd = timeIntervalEnd;
                            newTimeInterval.IssueId = issueForBreak.Id;
                            newTimeInterval.Note = note;

                            await _timeIntervalService.CreateAsync(newTimeInterval);

                            WeakReferenceMessenger.Default.Send(new AddIssueToProjectMessage(issueForBreak));
                            await CloseWindow(window);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(AddTimeInterval), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy dodawaniu wpisu przerwy");
            }
        }

        private Task CloseWindow(ICloseable window)
        {
            window?.Close();

            return Task.CompletedTask;
        }
    }
}