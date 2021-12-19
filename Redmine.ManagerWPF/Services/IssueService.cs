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
            var issues = await _context.Issues.Include(x => x.Issues).Where(x => x.Project.Id == projectId).ToListAsync();

            issues = await GetCommentForIssues(issues);

            var mainIssues = issues.Where(x => x.MainTask == null).OrderBy(x => x.Name).ToList();

            CreateTree(mainIssues, issues);

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

        public Issue GetIssue(int id)
        {
            return _context.Issues.Where(X => X.Id == id).SingleOrDefault();
        }

        public Task<Issue> GetIssueWithTimeIntervalAsync(int id)
        {
            return _context.Issues
            .Include(x => x.TimesForIssue)
            .Where(X => X.Id == id)
            .SingleOrDefaultAsync();
        }

        public Task<List<Issue>> GetAllIssueAsync()
        {
            return _context.Issues.ToListAsync();
        }

        public async Task Update(Issue entity)
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }

        private void CreateTree(List<Issue> parents, List<Issue> issues)
        {
            if (parents.Count > 0)
            {
                foreach (var item in parents)
                {
                    var currentParrent = issues.Where(x => x.MainTask != null && x.MainTask.Id == item.Id).OrderBy(x => x.Name).ToList();
                    item.Issues = currentParrent;
                    CreateTree(currentParrent, issues);
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
                var existingIssue = await _context.Issues.FirstOrDefaultAsync(x => x.SourceId == redmineIssue.Id);

                Issue addedOrUpdatedIssue = null;

                if (existingIssue == null)
                {
                    var entity = _mapper.Map<Issue>(redmineIssue);

                    var project = await _context.Projects.FirstOrDefaultAsync(x => x.SourceId == redmineIssue.ProjectId);
                    if (project != null)
                    {
                        entity.Project = project;

                        if (await _context.Issues.AnyAsync(x => x.SourceId == redmineIssue.ParentIssueId))
                        {
                            var parentIssue = await _context.Issues.FirstOrDefaultAsync(x => x.SourceId == redmineIssue.ParentIssueId);
                            if (parentIssue != null)
                            {
                                entity.MainTask = parentIssue;
                            }
                        }
                        entity.Status = Data.Enums.StatusType.New.ToString();
                        await _context.AddAsync(entity);
                        await _context.SaveChangesAsync();
                        addedOrUpdatedIssue = entity;
                    }
                }
                else
                {
                    var project = await _context.Projects.FirstOrDefaultAsync(x => x.SourceId == redmineIssue.ProjectId);
                    if (project != null)
                    {
                        existingIssue.Project = project;

                        _mapper.Map(redmineIssue, existingIssue);
                        _context.Update(existingIssue);
                        await _context.SaveChangesAsync();
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
            catch
            {
                throw;
            }
        }

        public async Task UpdateTreeStructure(Integration.Models.IssueDto redmineIssue, Issue issue)
        {
            var parentIssue = await _context.Issues.FirstOrDefaultAsync(x => x.SourceId == redmineIssue.ParentIssueId);
            if (parentIssue != null)
            {
                issue.MainTask = parentIssue;
                _context.Update(issue);
            }

            await _context.SaveChangesAsync();
        }
    }
}