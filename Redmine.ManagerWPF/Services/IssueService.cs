using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Data;
using Redmine.ManagerWPF.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.Services
{
    public class IssueService : IService
    {
        private readonly Context _context;
        private readonly IMapper _mapper;
        private readonly CommentService _commentService;

        public IssueService(
            Context context,
            IMapper mapper,
            CommentService commentService)
        {
            _context = context;
            _mapper = mapper;
            _commentService = commentService;
        }

        public async Task<List<Issue>> GetIssuesByProjectIdAsync(long projectId)
        {
            var result = _context.Issues.Include(x => x.Issues).AsQueryable();

            result = result.Where(x => x.Project.Id == projectId);

            var issues = await result.ToListAsync();

            issues = await GetCommentForIssues(issues);

            var mainIssues = issues.Where(x => x.MainTask == null).OrderBy(x => x.Name).ToList();

            await CreateTree(mainIssues, issues);

            return mainIssues;
        }

        private async Task<List<Issue>> GetCommentForIssues(List<Issue> issues)
        {
            var issuesIds = issues.Select(x => x.Id).ToList();
            var comments = await _commentService.GetCommentByIssuesIdsAsync(issuesIds);

            foreach (var item in issues)
            {
                item.Comments = comments.Where(x => x.Issue.Id == item.Id).OrderBy(x => x.Date).ToList();
            }

            return issues;
        }

        public Task<Issue> GetIssueAsync(int id)
        {
            return _context.Issues.Where(X => X.Id == id).SingleOrDefaultAsync();
        }

        public Task<List<Issue>> GetAllIssueAsync()
        {
            return _context.Issues.ToListAsync();
        }

        private async Task CreateTree(List<Issue> parents, List<Issue> issues)
        {
            if (parents.Count > 0)
            {
                foreach (var item in parents)
                {
                    var currentParrent = issues.Where(x => x.MainTask != null && x.MainTask.Id == item.Id).OrderBy(x => x.Name).ToList();
                    item.Issues = currentParrent;
                    await CreateTree(currentParrent, issues);
                }
            }
            else
            {
                return;
            }
        }

        public async Task SynchronizeIssues(Integration.Models.IssueDto redmineIssue)
        {
            try
            {
                var existingIssue = _context.Issues.FirstOrDefault(x => x.SourceId == redmineIssue.Id);

                Issue addedOrUpdatedIssue = null;

                if (existingIssue == null)
                {
                    var entity = _mapper.Map<Issue>(redmineIssue);

                    var project = _context.Projects.FirstOrDefault(x => x.SourceId == redmineIssue.ProjectId);
                    if(project != null)
                    {
                        entity.Project = project;

                        if (_context.Issues.Any(x => x.SourceId == redmineIssue.ParentIssueId))
                        {
                            var parentIssue = _context.Issues.FirstOrDefault(x => x.SourceId == redmineIssue.ParentIssueId);
                            if (parentIssue != null)
                            {
                                entity.MainTask = parentIssue;
                            }
                        }
                        entity.Status = Data.Enums.StatusType.New.ToString();
                        _context.Add(entity);
                        _context.SaveChanges();
                        addedOrUpdatedIssue = entity;
                    } 
                }
                else
                {
                    var project = _context.Projects.FirstOrDefault(x => x.SourceId == redmineIssue.ProjectId);
                    if (project != null)
                    {
                        existingIssue.Project = project;

                        _mapper.Map(redmineIssue, existingIssue);
                        _context.Update(existingIssue);
                        _context.SaveChanges();
                        addedOrUpdatedIssue = existingIssue;
                    }
                }

                if (addedOrUpdatedIssue != null)
                {
                    foreach (var commentDto in redmineIssue.Comments)
                    {
                        await _commentService.SynchronizeCommentAsync(commentDto, addedOrUpdatedIssue);
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Task UpdateTreeStructure(Integration.Models.IssueDto redmineIssue, Issue issue)
        {
            var parentIssue = _context.Issues.FirstOrDefault(x => x.SourceId == redmineIssue.ParentIssueId);
            if(parentIssue != null)
            {
                issue.MainTask = parentIssue;
                _context.Update(issue);
            }

            return Task.FromResult(_context.SaveChanges());
        }
    }
}