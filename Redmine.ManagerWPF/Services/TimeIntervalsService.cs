using Microsoft.EntityFrameworkCore;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Data;
using Redmine.ManagerWPF.Data.Enums;
using Redmine.ManagerWPF.Data.Models;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.Services
{
    public class TimeIntervalsService : IService
    {
        private readonly Context _context;
        private readonly IssueService _issueService;
        private readonly CommentService _commentService;

        public TimeIntervalsService(Context context, IssueService issueService, CommentService commentService)
        {
            _context = context;
            _issueService = issueService;
            _commentService = commentService;
        }

        public async Task CreateAsync(TreeModel treeModel, DateTime start, DateTime end)
        {
            if (treeModel.Type == nameof(Issue))
            {
                var issue = await _issueService.GetIssueWithTimeIntervalAsync(treeModel.Id);
                if (issue != null)
                {
                    var newTimeInterval = new TimeInterval();
                    newTimeInterval.TimeIntervalStart = start;
                    newTimeInterval.TimeIntervalEnd = end;
                    issue.TimesForIssue.Add(newTimeInterval);
                    _context.Update(issue);
                    await _context.SaveChangesAsync();
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
                    _context.Update(comment);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public Task<List<TimeInterval>> GetTimeIntervalsForIssueAsync(int issueId)
        {
            return _context.TimeIntervals.Where(x => x.Issue.Id == issueId).ToListAsync();
        }

        public List<TimeInterval> GetTimeIntervalsForIssue(int issueId)
        {
            return _context.TimeIntervals.Where(x => x.Issue.Id == issueId).ToList();
        }

        public Task<List<TimeInterval>> GetTimeIntervalsForCommentAsync(int commentId)
        {
            return _context.TimeIntervals.Where(x => x.Comment.Id == commentId).ToListAsync();
        }

        public List<TimeInterval> GetTimeIntervalsForComment(int commentId)
        {
            return _context.TimeIntervals.Where(x => x.Comment.Id == commentId).ToList();
        }

        public Task<TimeInterval> GetTimeIntervalAsync(long id)
        {
            return _context.TimeIntervals.Where(x => x.Id == id).SingleOrDefaultAsync();
        }

        public async Task DeleteAsync(TimeInterval timeInterval)
        {
            _context.Remove(timeInterval);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TimeInterval timeInterval)
        {
            _context.Update(timeInterval);
            await _context.SaveChangesAsync();
        }

        public async Task<TimeInterval> CreateEmptyAsync(int objectId, ObjectType type)
        {
            if (type == ObjectType.Issue)
            {
                var issue = await _issueService.GetIssueAsync(objectId);
                if (issue != null)
                {
                    var timeInterval = new TimeInterval();
                    timeInterval.Issue = issue;
                    _context.TimeIntervals.Add(timeInterval);
                    await _context.SaveChangesAsync();

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
                    _context.TimeIntervals.Add(timeInterval);
                    await _context.SaveChangesAsync();

                    return timeInterval;
                }
            }

            return null;
        }

        public async Task<bool> CheckIfAnyStartedTimeIntervalExistAsync()
        {
            var check = await _context.TimeIntervals.FirstOrDefaultAsync(x => x.TimeIntervalStart.HasValue && !x.TimeIntervalEnd.HasValue);
            if (check != null)
            {
                return true;
            }

            return false;
        }

        public bool CheckIfAnyStartedTimeIntervalExist()
        {
            var check = _context.TimeIntervals.FirstOrDefault(x => x.TimeIntervalStart.HasValue && !x.TimeIntervalEnd.HasValue);
            if (check != null)
            {
                return true;
            }

            return false;
        }

        public Task<List<TimeInterval>> GetFinishedForCurrentDateAsync(DateTime date)
        {
            return _context.TimeIntervals
                .Include(x => x.Issue)
                    .ThenInclude(x => x.Project)
                .Include(x => x.Comment)
                    .ThenInclude(x => x.Issue)
                        .ThenInclude(x => x.Project)
                .Where(x => x.TimeIntervalStart.HasValue && x.TimeIntervalStart.Value.Date == date.Date && x.TimeIntervalEnd.HasValue && x.TimeIntervalEnd.Value.Date == date.Date)
                .ToListAsync();
        }
    }
}