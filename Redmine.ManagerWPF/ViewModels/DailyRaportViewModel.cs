using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Redmine.ManagerWPF.Data.Models;
using Redmine.ManagerWPF.Desktop.Extensions;
using Redmine.ManagerWPF.Desktop.Helpers;
using Redmine.ManagerWPF.Desktop.Models.DailyRaport;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class DailyRaportViewModel : ObservableRecipient
    {
        #region Properties
        private bool _dataLoading;

        public bool DataLoading
        {
            get => _dataLoading;
            set => SetProperty(ref _dataLoading, value);
        }


        private DateTime _date;

        public DateTime Date
        {
            get => _date;
            set
            {
                SetProperty(ref _date, value);
                LoadRaportData();
            }
        }

        public ExtendedObservableCollection<ProjectListItemModel> RaportData { get; set; } = new ExtendedObservableCollection<ProjectListItemModel>();
        #endregion

        #region Injections
        private readonly TimeIntervalsService _timeIntervalsService;
        private readonly IMessageBoxHelper _messageBoxHelper;
        private readonly ILogger<DailyRaportViewModel> _logger;
        #endregion

        #region Commands
        public IRelayCommand<object> CopyNoteToClipboardCommand { get; }
        public IRelayCommand<object> CopyTimeToClipboardCommand { get; }
        #endregion

        public DailyRaportViewModel()
        {
            _timeIntervalsService = Ioc.Default.GetRequiredService<TimeIntervalsService>();
            _messageBoxHelper = Ioc.Default.GetRequiredService<IMessageBoxHelper>();
            _logger = Ioc.Default.GetLoggerForType<DailyRaportViewModel>();

            CopyNoteToClipboardCommand = new RelayCommand<object>(CopyNoteToClipboard);
            CopyTimeToClipboardCommand = new RelayCommand<object>(CopyTimeToClipboard);

            Date = DateTime.Now.Date;
        }

        private void CopyNoteToClipboard(object node)
        {
            if (node is TimeIntervalListItemModel timeInterval)
            {
                Clipboard.SetText(timeInterval.Note);
            }
        }

        private void CopyTimeToClipboard(object node)
        {
            if (node is ListItemModel listItemModel)
            {
                var totalTime = listItemModel.TotalTimeTimeSpan;
                Clipboard.SetText(Math.Round(totalTime.TotalHours, 2).ToString());
            }

            if (node is TimeIntervalListItemModel timeInterval)
            {
                var totalTime = timeInterval.EndDate - timeInterval.StartDate;
                Clipboard.SetText(Math.Round(totalTime.TotalHours, 2).ToString());
            }
        }

        private async void LoadRaportData()
        {
            DataLoading = true;
            try
            {
                var raportData = await GetRaportDataAsync();
                RaportData.Clear();

                Application.Current.Dispatcher.Invoke(() => SetRaportData(raportData));

            }
            catch (ArgumentException ex)
            {
                _logger.LogError("{0} {1}", nameof(LoadRaportData), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox("Nie skonfigurowano połączenia do bazy danych", "Błąd");
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(LoadRaportData), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Nie udało się utworzyć raportu");
            }
            finally
            {
                DataLoading = false;
            }
        }

        private void SetRaportData(List<ProjectListItemModel> raportData)
        {
            foreach (var item in raportData)
            {
                RaportData.Add(item);
            }
        }

        private async Task<List<ProjectListItemModel>> GetRaportDataAsync()
        {
            var result = new List<ProjectListItemModel>();

            var timeIntervalForToday = await _timeIntervalsService.GetFinishedForCurrentDateAsync(Date);

            var groupTimeIntervalForIssuesByProject = timeIntervalForToday.Where(x => x.Issue != null && x.Comment == null).GroupBy(x => x.Issue.Project.Id);
            var groupTimeIntervalForCommentByProject = timeIntervalForToday.Where(x => x.Issue == null && x.Comment != null).GroupBy(x => x.Comment.Issue.Project.Id);
            foreach (var projectGroup in groupTimeIntervalForIssuesByProject)
            {
                var timeIntervalsFromCommentsForProject = groupTimeIntervalForCommentByProject.FirstOrDefault(x => x.Key == projectGroup.Key);

                var projectToAdd = new ProjectListItemModel();

                var project = projectGroup.FirstOrDefault().Issue.Project;

                projectToAdd.TotalTimeTimeSpan = new TimeSpan();

                projectToAdd.Name = project.Name;
                projectToAdd.ProjectId = projectGroup.Key;
                projectToAdd.Childrens = new List<IssueListItemModel>();

                var groupByIssue = projectGroup.GroupBy(x => x.Issue.Id);

                foreach (var issueGroup in groupByIssue)
                {
                    var issue = issueGroup.First().Issue;

                    var issueToAdd = new IssueListItemModel
                    {
                        Name = issue.Name,
                        IssueId = issueGroup.Key,
                        TotalTimeTimeSpan = new TimeSpan(),
                        Childrens = new List<TimeIntervalListItemModel>()
                    };

                    foreach (var timeInterval in issueGroup)
                    {
                        var (timeIntervalToAdd, timeDifference) = PrepareTimeInterval(timeInterval, false);

                        issueToAdd.Childrens.Add(timeIntervalToAdd);
                        issueToAdd.TotalTimeTimeSpan += timeDifference;
                    }

                    if (timeIntervalsFromCommentsForProject != null)
                    {
                        var fromCommentGroupByIssue = timeIntervalsFromCommentsForProject.GroupBy(x => x.Comment.Issue.Id);

                        foreach (var issueGroupFromComment in fromCommentGroupByIssue)
                        {
                            if (issueGroupFromComment.Key == issueGroup.Key)
                            {
                                foreach (var timeIntervalForComment in issueGroupFromComment)
                                {
                                    var (timeIntervalToAdd, timeDifference) = PrepareTimeInterval(timeIntervalForComment, true);

                                    issueToAdd.Childrens.Add(timeIntervalToAdd);
                                    issueToAdd.TotalTimeTimeSpan += timeDifference;
                                }
                            }
                        }
                    }

                    SetTotalTime(issueToAdd);

                    projectToAdd.Childrens.Add(issueToAdd);
                    projectToAdd.TotalTimeTimeSpan += issueToAdd.TotalTimeTimeSpan;
                }

                if (timeIntervalsFromCommentsForProject != null)
                {
                    var existingsIssuesIds = projectToAdd.Childrens.Select(x => x.IssueId).ToList();

                    var groupAllByIssue = timeIntervalsFromCommentsForProject.GroupBy(x => x.Comment.Issue.Id);

                    var filteredIssues = groupAllByIssue.Where(x => !existingsIssuesIds.Contains(x.Key));

                    foreach (var issueGroup in filteredIssues)
                    {
                        var issue = issueGroup.First().Comment.Issue;

                        var issueToAdd = new IssueListItemModel
                        {
                            Name = issue.Name,
                            IssueId = issueGroup.Key,
                            TotalTimeTimeSpan = new TimeSpan(),
                            Childrens = new List<TimeIntervalListItemModel>()
                        };

                        foreach (var timeInterval in issueGroup)
                        {
                            var (timeIntervalToAdd, timeDifference) = PrepareTimeInterval(timeInterval, true);

                            issueToAdd.Childrens.Add(timeIntervalToAdd);
                            issueToAdd.TotalTimeTimeSpan += timeDifference;
                        }

                        SetTotalTime(issueToAdd);

                        projectToAdd.Childrens.Add(issueToAdd);
                        projectToAdd.TotalTimeTimeSpan += issueToAdd.TotalTimeTimeSpan;
                    }

                }

                SetTotalTime(projectToAdd);

                result.Add(projectToAdd);
            }

            var noAddedProjects = groupTimeIntervalForCommentByProject.Where(x => !result.Select(x => x.ProjectId).Contains(x.Key)).ToList();

            foreach (var projectGroup in noAddedProjects)
            {
                var projectToAdd = new ProjectListItemModel();

                var project = projectGroup.FirstOrDefault().Comment.Issue.Project;

                projectToAdd.TotalTimeTimeSpan = new TimeSpan();

                projectToAdd.Name = project.Name;
                projectToAdd.ProjectId = projectGroup.Key;
                projectToAdd.Childrens = new List<IssueListItemModel>();

                var groupByIssue = projectGroup.GroupBy(x => x.Comment.Issue.Id);

                foreach (var issueGroup in groupByIssue)
                {
                    var issueToAdd = PrepareIssue(issueGroup);

                    SetTotalTime(issueToAdd);

                    projectToAdd.Childrens.Add(issueToAdd);
                    projectToAdd.TotalTimeTimeSpan += issueToAdd.TotalTimeTimeSpan;
                }

                SetTotalTime(projectToAdd);

                result.Add(projectToAdd);
            }

            return result;
        }

        private void SetTotalTime(ListItemModel item)
        {
            if (item.TotalTimeTimeSpan.Days > 0)
            {
                item.TotalTime = $"{item.TotalTimeTimeSpan.Days} d {item.TotalTimeTimeSpan.Hours} h {item.TotalTimeTimeSpan.Minutes} min {item.TotalTimeTimeSpan.Seconds} s";
            }
            else
            {
                item.TotalTime = $"{item.TotalTimeTimeSpan.Hours} h {item.TotalTimeTimeSpan.Minutes} min {item.TotalTimeTimeSpan.Seconds} s";
            }
        }

        private (TimeIntervalListItemModel, TimeSpan) PrepareTimeInterval(TimeInterval timeInterval, bool fromComment)
        {
            var timeIntervalToAdd = new TimeIntervalListItemModel
            {
                TimeIntervalId = timeInterval.Id,
                StartDate = timeInterval.TimeIntervalStart.Value,
                EndDate = timeInterval.TimeIntervalEnd.Value,
                Note = timeInterval.Note
            };

            if (fromComment)
            {
                timeIntervalToAdd.ToComment = $"{timeInterval.Comment.CreatedBy} {timeInterval.Comment.Date.ToString("dd.MM.yyyy HH:mm")}";
            }

            var timeDifference = timeIntervalToAdd.EndDate - timeIntervalToAdd.StartDate;

            timeIntervalToAdd.Time = timeDifference.Days > 0 ? $"{timeDifference.Days} d {timeDifference.Hours} h {timeDifference.Minutes} min {timeDifference.Seconds} s" : $"{timeDifference.Hours} h {timeDifference.Minutes} min {timeDifference.Seconds} s";

            return (timeIntervalToAdd, timeDifference);
        }

        private IssueListItemModel PrepareIssue(IGrouping<int, TimeInterval> issueGroup)
        {
            var issue = issueGroup.First().Comment.Issue;
            var issueToAdd = new IssueListItemModel
            {
                Name = issue.Name,
                IssueId = issueGroup.Key,
                TotalTimeTimeSpan = new TimeSpan(),
                Childrens = new List<TimeIntervalListItemModel>()
            };

            foreach (var timeInterval in issueGroup)
            {
                var (timeIntervalToAdd, timeDifference) = PrepareTimeInterval(timeInterval, true);

                issueToAdd.Childrens.Add(timeIntervalToAdd);
                issueToAdd.TotalTimeTimeSpan += timeDifference;
            }

            return issueToAdd;
        }
    }
}
