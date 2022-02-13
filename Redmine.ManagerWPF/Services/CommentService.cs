using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Data.Dapper;
using Redmine.ManagerWPF.Data.Models;

namespace Redmine.ManagerWPF.Desktop.Services
{
    public class CommentService : IService
    {
        private readonly IContext _context;
        private readonly IMapper _mapper;

        public CommentService(IContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Comment> GetCommentAsync(int id)
        {
            using var context = await _context.GetConnectionAsync();

            return await context.GetAsync<Comment>(id);
        }

        public async Task<Comment> GetCommentWithTimeIntervalAsync(int id)
        {
            using var context = await _context.GetConnectionAsync();

            var comment = await context.GetAsync<Comment>(id);

            if (comment != null)
            {
                var timeIntervals = await context.GetListAsync<TimeInterval>(new { CommentId = id });
                comment.TimeForComment = timeIntervals.ToList();
            }

            return comment;
        }

        public async Task<List<Comment>> GetCommentByIssueIdAsync(long issueId)
        {
            using var context = await _context.GetConnectionAsync();

            var comments = await context.GetListAsync<Comment>(new { IssueId = issueId });

            return comments.ToList();
        }

        public async Task<List<Comment>> GetCommentByIssuesIdsAsync(List<int> issuesIds)
        {
            using var context = await _context.GetConnectionAsync();

            var query = @"SELECT comments.*, issues.* FROM [dbo].[Comments] comments
                          INNER JOIN issues ON comments.IssueId = issues.Id
                          WHERE comments.IssueId IN @ids";
            var comments = await context.QueryAsync<Comment, Issue, Comment>(query, (comment, issue) =>
            {
                comment.Issue = issue;
                return comment;
            },
            new { ids = issuesIds });

            return comments.ToList();
        }

        public async Task<List<Comment>> GetCommentByIssuesIdsWithPhraseAsync(List<int> issuesIds, string searchPhrase)
        {

            using var context = await _context.GetConnectionAsync();

            var comments = await context.GetListAsync<Comment>("WHERE IssueId IN @ids AND Text LIKE '%" + searchPhrase + "%'", new { ids = issuesIds });

            return comments.ToList();
        }

        public async Task Update(Comment entity)
        {
            using var context = await _context.GetConnectionAsync();

            var comments = await context.UpdateAsync(entity);
        }

        public async Task SynchronizeCommentAsync(Integration.Models.JournalDto redmineComment, Issue issue)
        {
            using var context = await _context.GetConnectionAsync();

            var query = @"SELECT * FROM [dbo].[Comments] WHERE SourceId = @id";

            var existingComment = await context.QueryFirstOrDefaultAsync<Comment>(query, new { id = redmineComment.Id });

            if (existingComment == null)
            {
                if (!string.IsNullOrWhiteSpace(redmineComment.Text))
                {
                    var entity = _mapper.Map<Comment>(redmineComment);
                    entity.IssueId = issue.Id;
                    await context.InsertAsync(entity);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(redmineComment.Text))
                {
                    _mapper.Map(redmineComment, existingComment);
                    existingComment.IssueId = issue.Id;
                    await context.UpdateAsync(existingComment);
                }
                else
                {
                    await context.DeleteAsync(existingComment);
                }
            }
        }

        public async Task SynchronizeCommentAsync(Integration.Models.JournalDto redmineComment, Comment comment)
        {
            using var context = await _context.GetConnectionAsync();

            var existingComment = await context.GetAsync<Comment>(comment.Id);

            if (existingComment == null)
            {
                return;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(redmineComment.Text))
                {
                    _mapper.Map(redmineComment, existingComment);
                    await context.UpdateAsync(existingComment);
                }
                else
                {
                    await context.DeleteAsync(existingComment);
                }
            }
        }
    }
}