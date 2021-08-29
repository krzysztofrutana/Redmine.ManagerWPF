using Microsoft.EntityFrameworkCore;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Data;
using Redmine.ManagerWPF.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.Services
{
    public class CommentService : IService
    {
        private readonly Context _context;

        public CommentService(Context context)
        {
            _context = context;
        }

        public Task<List<Comment>> GetCommentByIssueId(long issueId)
        {
            var result = _context.Comments.AsQueryable();

            result = result.Where(x => x.Issue.Id == issueId);

            return result.ToListAsync();
        }

        public Task<List<Comment>> GetCommentByIssuesIds(List<int> issuesIds)
        {
            var result = _context.Comments.AsQueryable();

            result = result.Where(x => issuesIds.Contains(x.Issue.Id));

            return result.ToListAsync();
        }
    }
}