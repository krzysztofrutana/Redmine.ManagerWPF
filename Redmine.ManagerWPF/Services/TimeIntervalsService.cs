using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Data.Dapper;
using Redmine.ManagerWPF.Data.Enums;
using Redmine.ManagerWPF.Data.Models;
using Redmine.ManagerWPF.Desktop.Models.Tree;

namespace Redmine.ManagerWPF.Desktop.Services
{
    public class TimeIntervalsService : IService
    {
        private readonly IContext _context;
        private readonly IssueService _issueService;
        private readonly CommentService _commentService;

        public TimeIntervalsService(IContext context, IssueService issueService, CommentService commentService)
        {
            _context = context;
            _issueService = issueService;
            _commentService = commentService;
        }

        public async Task<TimeInterval> CreateAsync(TimeInterval timeInterval)
        {
            if (timeInterval == null)
            {
                throw new ArgumentNullException(nameof(timeInterval));
            }

            using var context = await _context.GetConnectionAsync();

            var id = await context.InsertAsync(timeInterval);
            if (id.HasValue)
            {
                timeInterval.Id = id.Value;
            }

            return timeInterval;
        }
        public async Task CreateAsync(TreeModel treeModel, DateTime start, DateTime end)
        {
            using var context = await _context.GetConnectionAsync();
            if (treeModel.Type == nameof(Issue))
            {
                var issue = await _issueService.GetIssueWithTimeIntervalAsync(treeModel.Id);
                if (issue != null)
                {
                    var newTimeInterval = new TimeInterval();
                    newTimeInterval.TimeIntervalStart = start;
                    newTimeInterval.TimeIntervalEnd = end;
                    issue.TimesForIssue.Add(newTimeInterval);
                    await context.UpdateAsync(issue);
                }

            }
            else if (treeModel.Type == nameof(Comment))
            {
                var comment = await _commentService.GetCommentWithTimeIntervalAsync(treeModel.Id);
                if (comment != null)
                {
                    var newTimeInterval = new TimeInterval();
                    newTimeInterval.TimeIntervalStart = start;
                    newTimeInterval.TimeIntervalEnd = end;
                    comment.TimeForComment.Add(newTimeInterval);
                    await context.UpdateAsync(comment);
                }
            }
        }

        public async Task<List<TimeInterval>> GetTimeIntervalsForIssueAsync(int issueId)
        {
            using var context = await _context.GetConnectionAsync();

            var comments = await context.GetListAsync<TimeInterval>(new { IssueId = issueId });

            return comments.ToList();
        }

        public async Task<List<TimeInterval>> GetTimeIntervalsForCommentAsync(int commentId)
        {
            using var context = await _context.GetConnectionAsync();

            var comments = await context.GetListAsync<TimeInterval>(new { CommentId = commentId });

            return comments.ToList();
        }

        public async Task<TimeInterval> GetTimeIntervalAsync(long id)
        {
            using var context = await _context.GetConnectionAsync();

            return await context.GetAsync<TimeInterval>(id);
        }

        public async Task DeleteAsync(TimeInterval timeInterval)
        {
            using var context = await _context.GetConnectionAsync();

            await context.DeleteAsync(timeInterval);
        }

        public async Task UpdateAsync(TimeInterval timeInterval)
        {
            using var context = await _context.GetConnectionAsync();

            await context.UpdateAsync(timeInterval);
        }

        public async Task<TimeInterval> CreateEmptyAsync(int objectId, ObjectType type)
        {
            using var context = await _context.GetConnectionAsync();

            if (type == ObjectType.Issue)
            {
                var issue = await _issueService.GetIssueAsync(objectId);
                if (issue != null)
                {
                    var timeInterval = new TimeInterval();
                    timeInterval.Issue = issue;
                    timeInterval.IssueId = issue.Id;
                    var id = await context.InsertAsync(timeInterval);
                    if (id.HasValue)
                    {
                        timeInterval.Id = id.Value;
                    }

                    return timeInterval;
                }
            }

            if (type == ObjectType.Comment)
            {
                var comment = await _commentService.GetCommentAsync(objectId);
                if (comment != null)
                {
                    var timeInterval = new TimeInterval();
                    timeInterval.Comment = comment;
                    timeInterval.CommentId = comment.Id;
                    var id = await context.InsertAsync(timeInterval);
                    if (id.HasValue)
                    {
                        timeInterval.Id = id.Value;
                    }

                    return timeInterval;
                }
            }

            return null;
        }

        public bool CheckIfAnyStartedTimeIntervalExist()
        {
            using var context = _context.GetConnection();

            var comments = context.GetList<TimeInterval>("WHERE [TimeIntervalStart] IS NOT NULL AND [TimeIntervalEnd] IS NULL", new { });

            return comments.Any();
        }

        public async Task<bool> CheckIfAnyStartedTimeIntervalExistAsync()
        {
            using var context = await _context.GetConnectionAsync();

            var comments = await context.GetListAsync<TimeInterval>("WHERE TimeIntervalStart IS NOT NULL AND TimeIntervalEnd IS NULL", new { });

            return comments.Any();
        }

        public async Task<TimeInterval> GetActualAsync()
        {
            using var context = await _context.GetConnectionAsync();

            var query = @"SELECT * FROM [dbo].[TimeIntervals] timeIntervals
                          LEFT JOIN [dbo].[Issues] issues ON timeIntervals.IssueId = issues.Id
						  LEFT JOIN [dbo].[Comments] comments ON timeIntervals.CommentId = comments.Id
                          WHERE timeIntervals.[TimeIntervalStart] IS NOT NULL AND timeIntervals.[TimeIntervalEnd] IS NULL";

            var timeIntervals = await context.QueryAsync<TimeInterval, Issue, Comment, TimeInterval>(query, (timeInterval, issue, comment) =>
            {
                timeInterval.Issue = issue;
                timeInterval.Comment = comment;
                return timeInterval;
            },
             new { });

            return timeIntervals.FirstOrDefault();
        }

        public async Task<List<TimeInterval>> GetFinishedForCurrentDateAsync(DateTime date)
        {
            using var context = await _context.GetConnectionAsync();

            DateTime fromDate = date.Date;
            DateTime toDate = date.Date.AddDays(1);

            var query = @"SELECT * FROM [dbo].[TimeIntervals] timeIntervals
                          LEFT JOIN [dbo].[Issues] issues ON timeIntervals.IssueId = issues.Id
						  LEFT JOIN [dbo].[Comments] comments ON timeIntervals.CommentId = comments.Id
                          LEFT JOIN [dbo].[Issues] commentIssues ON comments.IssueId = commentIssues.Id
                          LEFT JOIN [dbo].[Projects] projects ON commentIssues.ProjectId = projects.Id OR issues.ProjectId = projects.Id
                          WHERE timeIntervals.[TimeIntervalStart] >= @fromDate AND timeIntervals.[TimeIntervalStart] < @toDate AND timeIntervals.[TimeIntervalEnd] >= @fromDate AND timeIntervals.[TimeIntervalEnd] < @toDate";

            var timeIntervals = await context.QueryAsync<TimeInterval, Issue, Comment, Issue, Project, TimeInterval>(query, (timeInterval, issue, comment, commentIssue, project) =>
            {
                timeInterval.Issue = issue;
                timeInterval.Comment = comment;
                if (timeInterval.Issue != null)
                {
                    timeInterval.Issue.Project = project;
                }

                if (timeInterval.Comment != null)
                {
                    timeInterval.Comment.Issue = commentIssue;
                    if (commentIssue != null)
                    {
                        timeInterval.Comment.Issue.Project = project;
                    }
                }
                return timeInterval;
            },
             new { fromDate = fromDate, toDate = toDate });

            return timeIntervals.ToList();
        }
    }
}