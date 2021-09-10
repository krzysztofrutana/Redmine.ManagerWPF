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

        public Task<Comment> GetCommentByIdAsync(int id)
        {
            return _context.Comments.FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<List<Comment>> GetCommentByIssueIdAsync(long issueId)
        {
            var result = _context.Comments.AsQueryable();

            result = result.Where(x => x.Issue.Id == issueId);

            return result.ToListAsync();
        }

        public Task<List<Comment>> GetCommentByIssuesIdsAsync(List<int> issuesIds)
        {
            var result = _context.Comments.AsQueryable();

            result = result.Where(x => issuesIds.Contains(x.Issue.Id));

            return result.ToListAsync();
        }

        public Task SynchronizeCommentAsync(Integration.Models.JournalDto redmineComment, Issue issue)
        {
            try
            {
                var existingComment = _context.Comments.FirstOrDefault(x => x.SourceId == redmineComment.Id);

                if (existingComment == null)
                {
                    if(!string.IsNullOrWhiteSpace(redmineComment.Text))
                    {
                        var entity = _mapper.Map<Comment>(redmineComment);
                        entity.Issue = issue;
                        _context.Add(entity);
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(redmineComment.Text))
                    {
                        _mapper.Map(redmineComment, existingComment);
                        existingComment.Issue = issue;
                        _context.Update(existingComment);
                    }
                    else
                    {
                        _context.Remove(existingComment);
                    }
                }
                return Task.FromResult(_context.SaveChanges());
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }
    }
}