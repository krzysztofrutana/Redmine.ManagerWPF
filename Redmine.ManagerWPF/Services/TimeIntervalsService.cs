using Microsoft.EntityFrameworkCore;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Data;
using Redmine.ManagerWPF.Data.Models;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task Create(TreeModel treeModel, DateTime start, DateTime end)
        {
            if(treeModel.Type == nameof(Issue))
            {
                var issue = await _issueService.GetIssueWithTimeIntervalAsync(treeModel.Id);
                if(issue != null)
                {
                    var newTimeInterval = new TimeInterval();
                    newTimeInterval.TimeIntervalStart = start;
                    newTimeInterval.TimeIntervalEnd = end;
                    issue.TimesForIssue.Add(newTimeInterval);
                    _context.Update(issue);
                    _context.SaveChanges();
                }

            }
            else if(treeModel.Type == nameof(Comment))
            {
                var comment = await _commentService.GetCommentWithTimeIntervalAsync(treeModel.Id);
                if (comment != null)
                {
                    var newTimeInterval = new TimeInterval();
                    newTimeInterval.TimeIntervalStart = start;
                    newTimeInterval.TimeIntervalEnd = end;
                    comment.TimeForComment.Add(newTimeInterval);
                    _context.Update(comment);
                    _context.SaveChanges();
                }
            }
        }

        public Task<List<TimeInterval>> GetTimeIntervalsForIssue(int issueId)
        {
            return _context.TimeIntervals.Where(x => x.Issue.Id == issueId).ToListAsync();
        }

        public Task<List<TimeInterval>> GetTimeIntervalsForComment(int commentId)
        {
            return _context.TimeIntervals.Where(x => x.Comment.Id == commentId).ToListAsync();
        }
    }
}