using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Redmine.ManagerWPF.Data.Models;
using Redmine.ManagerWPF.Desktop.Helpers;
using Redmine.ManagerWPF.Desktop.Models.DailyRaport;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class DailyRaportViewModel : ObservableRecipient
    {
        private bool _dataLoading;

        public bool DataLoading
        {
            get => _dataLoading;
            set
            {
                SetProperty(ref _dataLoading, value);
            }
        }


        private DateTime _date;

        public DateTime Date
        {
            get { return _date; }
            set
            {
                SetProperty(ref _date, value);
                if (value != null)
                {
                    LoadRaportData();
                }
            }
        }


        public ObservableCollection<ProjectListItemModel> RaportData { get; set; } = new AsyncObservableCollection<ProjectListItemModel>();


        private readonly TimeIntervalsService _timeIntervalsService;
        private readonly IMessageBoxService _messageBoxService;

        public DailyRaportViewModel()
        {
            _timeIntervalsService = Ioc.Default.GetRequiredService<TimeIntervalsService>();
            _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();

            Date = DateTime.Now.Date;
        }

        private async void LoadRaportData()
        {
            DataLoading = true;
            try
            {
                await Task.Run(async () =>
                {
                    RaportData.Clear();

                    var timeIntervalForToday = await _timeIntervalsService.GetFinishedForCurrentDateAsync(Date);

                    var groupTimeIntervalForIssuesByProject = timeIntervalForToday.Where(x => x.Issue != null && x.Comment == null).GroupBy(x => x.Issue.Project);
                    var groupTimeIntervalForCommentByProject = timeIntervalForToday.Where(x => x.Issue == null && x.Comment != null).GroupBy(x => x.Comment.Issue.Project);
                    foreach (var projectGroup in groupTimeIntervalForIssuesByProject)
                    {
                        var timeIntervalsFromCommentsForProject = groupTimeIntervalForCommentByProject.Where(x => x.Key.Id == projectGroup.Key.Id).FirstOrDefault();

                        var projectToAdd = new ProjectListItemModel();

                        projectToAdd.TotalTimeTimeSpan = new TimeSpan();

                        projectToAdd.Name = projectGroup.Key.Name;
                        projectToAdd.ProjectId = projectGroup.Key.Id;
                        projectToAdd.Childrens = new List<IssueListItemModel>();

                        var groupByIssue = projectGroup.GroupBy(x => x.Issue);

                        foreach (var issueGroup in groupByIssue)
                        {
                            var issueToAdd = new IssueListItemModel();
                            issueToAdd.Name = issueGroup.Key.Name;
                            issueToAdd.IssueId = issueGroup.Key.Id;

                            issueToAdd.TotalTimeTimeSpan = new TimeSpan();

                            issueToAdd.Childrens = new List<TimeIntervalListItemModel>();

                            foreach (var timeInterval in issueGroup)
                            {
                                var (timeIntervalToAdd, timeDifference) = PrepareTimeInterval(timeInterval, false);

                                issueToAdd.Childrens.Add(timeIntervalToAdd);
                                issueToAdd.TotalTimeTimeSpan += timeDifference;
                            }

                            if (timeIntervalsFromCommentsForProject != null)
                            {
                                var fromCommentGroupByIssue = timeIntervalsFromCommentsForProject.GroupBy(x => x.Comment.Issue);

                                foreach (var issueGroupFromComment in fromCommentGroupByIssue)
                                {
                                    if (issueGroupFromComment.Key.Id == issueGroup.Key.Id)
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

                            var groupAllByIssue = timeIntervalsFromCommentsForProject.GroupBy(x => x.Comment.Issue);

                            var filteredIssues = groupAllByIssue.Where(x => !existingsIssuesIds.Contains(x.Key.Id));

                            foreach (var issueGroup in filteredIssues)
                            {
                                var issueToAdd = new IssueListItemModel();
                                issueToAdd.Name = issueGroup.Key.Name;
                                issueToAdd.IssueId = issueGroup.Key.Id;

                                issueToAdd.TotalTimeTimeSpan = new TimeSpan();

                                issueToAdd.Childrens = new List<TimeIntervalListItemModel>();

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

                        RaportData.Add(projectToAdd);
                    }

                    var noAddedProjects = groupTimeIntervalForCommentByProject.Where(x => !RaportData.Select(x => x.ProjectId).Contains(x.Key.Id)).ToList();

                    foreach (var projectGroup in noAddedProjects)
                    {
                        var projectToAdd = new ProjectListItemModel();

                        projectToAdd.TotalTimeTimeSpan = new TimeSpan();

                        projectToAdd.Name = projectGroup.Key.Name;
                        projectToAdd.ProjectId = projectGroup.Key.Id;
                        projectToAdd.Childrens = new List<IssueListItemModel>();

                        var groupByIssue = projectGroup.GroupBy(x => x.Comment.Issue);

                        foreach (var issueGroup in groupByIssue)
                        {
                            var issueToAdd = PrepareIssue(issueGroup);

                            SetTotalTime(issueToAdd);

                            projectToAdd.Childrens.Add(issueToAdd);
                            projectToAdd.TotalTimeTimeSpan += issueToAdd.TotalTimeTimeSpan;
                        }

                        SetTotalTime(projectToAdd);

                        RaportData.Add(projectToAdd);
                    }
                });
            }
            catch (ArgumentException)
            {
                _messageBoxService.ShowWarningInfoBox("Nie skonfigurowano połączenia do bazy danych", "Błąd");
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Nie udało się utworzyć raportu");
            }
            finally
            {
                DataLoading = false;
            }
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

        public (TimeIntervalListItemModel, TimeSpan) PrepareTimeInterval(TimeInterval timeInterval, bool fromComment)
        {
            var timeIntervalToAdd = new TimeIntervalListItemModel();
            timeIntervalToAdd.TimeIntervalId = timeInterval.Id;
            timeIntervalToAdd.StartDate = timeInterval.TimeIntervalStart.Value;
            timeIntervalToAdd.EndDate = timeInterval.TimeIntervalEnd.Value;
            timeIntervalToAdd.Note = timeInterval.Note;

            if (fromComment)
            {
                timeIntervalToAdd.ToComment = $"{timeInterval.Comment.CreatedBy} {timeInterval.Comment.Date.ToString("dd.MM.yyyy HH:mm")}";
            }

            var timeDifference = timeIntervalToAdd.EndDate - timeIntervalToAdd.StartDate;

            if (timeDifference.Days > 0)
            {
                timeIntervalToAdd.Time = $"{timeDifference.Days} d {timeDifference.Hours} h {timeDifference.Minutes} min {timeDifference.Seconds} s";
            }
            else
            {
                timeIntervalToAdd.Time = $"{timeDifference.Hours} h {timeDifference.Minutes} min {timeDifference.Seconds} s";
            }

            return (timeIntervalToAdd, timeDifference);
        }

        public IssueListItemModel PrepareIssue(IGrouping<Issue, TimeInterval> issueGroup)
        {
            var issueToAdd = new IssueListItemModel();
            issueToAdd.Name = issueGroup.Key.Name;
            issueToAdd.IssueId = issueGroup.Key.Id;

            issueToAdd.TotalTimeTimeSpan = new TimeSpan();

            issueToAdd.Childrens = new List<TimeIntervalListItemModel>();

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
