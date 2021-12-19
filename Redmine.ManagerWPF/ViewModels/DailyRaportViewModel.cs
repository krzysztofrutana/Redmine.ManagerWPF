using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Redmine.ManagerWPF.Desktop.Helpers;
using Redmine.ManagerWPF.Desktop.Models.DailyRaport;
using Redmine.ManagerWPF.Desktop.Services;
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
        public ObservableCollection<ProjectListItemModel> RaportData { get; set; } = new AsyncObservableCollection<ProjectListItemModel>();


        private readonly TimeIntervalsService _timeIntervalsService;

        public DailyRaportViewModel()
        {
            _timeIntervalsService = Ioc.Default.GetRequiredService<TimeIntervalsService>();


            LoadRaportData();
        }

        private async void LoadRaportData()
        {
            DataLoading = true;
            await Task.Run(async () =>
            {
                RaportData.Clear();

                var timeIntervalForToday = await _timeIntervalsService.GetFinishedForCurrentDateAsync(DateTime.Now);

                var groupIssuesByProject = timeIntervalForToday.Where(x => x.Issue != null && x.Comment == null).GroupBy(x => x.Issue.Project);
                var groupCommentsByIssue = timeIntervalForToday.Where(x => x.Issue == null && x.Comment != null).GroupBy(x => x.Comment.Issue);
                foreach (var projectGroup in groupIssuesByProject)
                {
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
                            var timeIntervalToAdd = new TimeIntervalListItemModel();
                            timeIntervalToAdd.TimeIntervalId = timeInterval.Id;
                            timeIntervalToAdd.StartDate = timeInterval.TimeIntervalStart.Value;
                            timeIntervalToAdd.EndDate = timeInterval.TimeIntervalEnd.Value;
                            var timeDifference = timeIntervalToAdd.EndDate - timeIntervalToAdd.StartDate;

                            if (timeDifference.Days > 0)
                            {
                                timeIntervalToAdd.Time = $"{timeDifference.Days} d {timeDifference.Hours} h {timeDifference.Minutes} min {timeDifference.Seconds} s";
                            }
                            else
                            {
                                timeIntervalToAdd.Time = $"{timeDifference.Hours} h {timeDifference.Minutes} min {timeDifference.Seconds} s";
                            }

                            issueToAdd.Childrens.Add(timeIntervalToAdd);
                            issueToAdd.TotalTimeTimeSpan += timeDifference;
                        }

                        var timeIntervalsFromComments = groupCommentsByIssue.Where(x => x.Key.Id == issueGroup.Key.Id).FirstOrDefault();
                        if (timeIntervalsFromComments != null)
                        {
                            foreach (var timeInterval in timeIntervalsFromComments)
                            {
                                var timeIntervalToAdd = new TimeIntervalListItemModel();
                                timeIntervalToAdd.TimeIntervalId = timeInterval.Id;
                                timeIntervalToAdd.StartDate = timeInterval.TimeIntervalStart.Value;
                                timeIntervalToAdd.EndDate = timeInterval.TimeIntervalEnd.Value;
                                timeIntervalToAdd.ToComment = $"{timeInterval.Comment.CreatedBy} {timeInterval.Comment.Date.ToString("dd.MM.yyyy HH:mm")}";
                                var timeDifference = timeIntervalToAdd.EndDate - timeIntervalToAdd.StartDate;

                                if (timeDifference.Days > 0)
                                {
                                    timeIntervalToAdd.Time = $"{timeDifference.Days} d {timeDifference.Hours} h {timeDifference.Minutes} min {timeDifference.Seconds} s";
                                }
                                else
                                {
                                    timeIntervalToAdd.Time = $"{timeDifference.Hours} h {timeDifference.Minutes} min {timeDifference.Seconds} s";
                                }

                                issueToAdd.Childrens.Add(timeIntervalToAdd);
                                issueToAdd.TotalTimeTimeSpan += timeDifference;
                            }
                        }

                        if (issueToAdd.TotalTimeTimeSpan.Days > 0)
                        {
                            issueToAdd.TotalTime = $"{issueToAdd.TotalTimeTimeSpan.Days} d {issueToAdd.TotalTimeTimeSpan.Hours} h {issueToAdd.TotalTimeTimeSpan.Minutes} min {issueToAdd.TotalTimeTimeSpan.Seconds} s";
                        }
                        else
                        {
                            issueToAdd.TotalTime = $"{issueToAdd.TotalTimeTimeSpan.Hours} h {issueToAdd.TotalTimeTimeSpan.Minutes} min {issueToAdd.TotalTimeTimeSpan.Seconds} s";
                        }

                        projectToAdd.Childrens.Add(issueToAdd);
                        projectToAdd.TotalTimeTimeSpan += issueToAdd.TotalTimeTimeSpan;
                    }

                    var timeIntervalsForCommentsAndProjects = groupCommentsByIssue.Where(x => x.Key.Project.Id == projectToAdd.ProjectId).ToList();
                    foreach (var issueGroup in timeIntervalsForCommentsAndProjects)
                    {
                        if (!projectToAdd.Childrens.Any(x => x.IssueId == issueGroup.Key.Id))
                        {
                            var issueToAdd = new IssueListItemModel();
                            issueToAdd.Name = issueGroup.Key.Name;
                            issueToAdd.IssueId = issueGroup.Key.Id;

                            issueToAdd.TotalTimeTimeSpan = new TimeSpan();

                            issueToAdd.Childrens = new List<TimeIntervalListItemModel>();

                            foreach (var timeInterval in issueGroup)
                            {
                                var timeIntervalToAdd = new TimeIntervalListItemModel();
                                timeIntervalToAdd.TimeIntervalId = timeInterval.Id;
                                timeIntervalToAdd.StartDate = timeInterval.TimeIntervalStart.Value;
                                timeIntervalToAdd.EndDate = timeInterval.TimeIntervalEnd.Value;
                                timeIntervalToAdd.ToComment = $"{timeInterval.Comment.CreatedBy} {timeInterval.Comment.Date.ToString("dd.MM.yyyy HH:mm")}";
                                var timeDifference = timeIntervalToAdd.EndDate - timeIntervalToAdd.StartDate;

                                if (timeDifference.Days > 0)
                                {
                                    timeIntervalToAdd.Time = $"{timeDifference.Days} d {timeDifference.Hours} h {timeDifference.Minutes} min {timeDifference.Seconds} s";
                                }
                                else
                                {
                                    timeIntervalToAdd.Time = $"{timeDifference.Hours} h {timeDifference.Minutes} min {timeDifference.Seconds} s";
                                }

                                issueToAdd.Childrens.Add(timeIntervalToAdd);
                                issueToAdd.TotalTimeTimeSpan += timeDifference;

                                if (issueToAdd.TotalTimeTimeSpan.Days > 0)
                                {
                                    issueToAdd.TotalTime = $"{issueToAdd.TotalTimeTimeSpan.Days} d {issueToAdd.TotalTimeTimeSpan.Hours} h {issueToAdd.TotalTimeTimeSpan.Minutes} min {issueToAdd.TotalTimeTimeSpan.Seconds} s";
                                }
                                else
                                {
                                    issueToAdd.TotalTime = $"{issueToAdd.TotalTimeTimeSpan.Hours} h {issueToAdd.TotalTimeTimeSpan.Minutes} min {issueToAdd.TotalTimeTimeSpan.Seconds} s";
                                }
                            }

                            projectToAdd.Childrens.Add(issueToAdd);
                            projectToAdd.TotalTimeTimeSpan += issueToAdd.TotalTimeTimeSpan;
                        }
                    }

                    if (projectToAdd.TotalTimeTimeSpan.Days > 0)
                    {
                        projectToAdd.TotalTime = $"{projectToAdd.TotalTimeTimeSpan.Days} d  {projectToAdd.TotalTimeTimeSpan.Hours}  h  {projectToAdd.TotalTimeTimeSpan.Minutes}  min  {projectToAdd.TotalTimeTimeSpan.Seconds}";
                    }
                    else
                    {
                        projectToAdd.TotalTime = $"{projectToAdd.TotalTimeTimeSpan.Hours} h {projectToAdd.TotalTimeTimeSpan.Minutes} min {projectToAdd.TotalTimeTimeSpan.Seconds} s";
                    }

                    RaportData.Add(projectToAdd);
                }
            });

            DataLoading = false;
        }
    }
}
