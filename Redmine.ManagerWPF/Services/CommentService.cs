using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Data;
using Redmine.ManagerWPF.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.Services
{
    public class CommentService : IService
    {
        private readonly Context _context;
        private readonly IMapper _mapper;

        public CommentService(Context context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public Task<Comment> GetCommentAsync(int id)
        {
            return _context.Comments.FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<Comment> GetCommentWithTimeIntervalAsync(int id)
        {
            return _context.Comments
            .Include(x => x.TimeForComment)
            .SingleOrDefaultAsync(x => x.Id == id);
        }

        public Task<List<Comment>> GetCommentByIssueIdAsync(long issueId)
        {
            return _context.Comments
            .Where(x => x.Issue.Id == issueId)
            .ToListAsync();
        }

        public Task<List<Comment>> GetCommentByIssuesIdsAsync(List<int> issuesIds)
        {
            return _context.Comments
            .Include(x => x.Issue)
            .Where(x => issuesIds.Contains(x.Issue.Id))
            .ToListAsync();
        }

        public Task<List<Comment>> GetCommentByIssuesIdsWithPhraseAsync(List<int> issuesIds, string searchPhrase)
        {
            return _context.Comments
            .AsNoTracking()
            .Include(x => x.Issue)
            .Where(x => issuesIds.Contains(x.Issue.Id) && x.Text.Contains(searchPhrase))
            .ToListAsync();
        }

        public async Task Update(Comment entity)
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task SynchronizeCommentAsync(Integration.Models.JournalDto redmineComment, Issue issue)
        {
            try
            {
                var existingComment = await _context.Comments.FirstOrDefaultAsync(x => x.SourceId == redmineComment.Id);

                if (existingComment == null)
                {
                    if (!string.IsNullOrWhiteSpace(redmineComment.Text))
                    {
                        var entity = _mapper.Map<Comment>(redmineComment);
                        _context.Comments.Add(entity);
                        await _context.SaveChangesAsync();

                        entity.Issue = issue;
                        _context.Comments.Update(entity);
                        await _context.SaveChangesAsync();

                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(redmineComment.Text))
                    {
                        _mapper.Map(redmineComment, existingComment);
                        existingComment.Issue = issue;
                        _context.Comments.Update(existingComment);
                    }
                    else
                    {
                        _context.Comments.Remove(existingComment);
                    }

                    await _context.SaveChangesAsync();
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}