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
        private readonly ProjectService _projectService;

        public IssueService(
            Context context,
            IMapper mapper,
            CommentService commentService,
            ProjectService projectService)
        {
            _context = context;
            _mapper = mapper;
            _commentService = commentService;
            _projectService = projectService;
        }

        public async Task<List<Issue>> GetIssuesByProjectIdAsync(long projectId)
        {
            var issues = await _context.Issues.Where(x => x.Project.Id == projectId).ToListAsync();

            foreach (var issue in issues)
            {
                await _context.Entry<Issue>(issue).ReloadAsync();
            }

            issues = await GetCommentForIssues(issues);

            var mainIssues = issues.Where(x => x.MainTask == null).OrderBy(x => x.Name).ToList();

            CreateTree(mainIssues, issues);

            return mainIssues;
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

        public async Task SynchronizeIssues(Integration.Models.IssueDto redmineIssue)
        {
            try
            {
                var existingIssue = await _context.Issues.FirstOrDefaultAsync(x => x.SourceId == redmineIssue.Id);

                Issue addedOrUpdatedIssue = null;

                if (existingIssue == null)
                {
                    var entity = _mapper.Map<Issue>(redmineIssue);

                    var project = await _projectService.GetProjectBySourceIdAsync(redmineIssue.ProjectId);
                    if (project != null)
                    {
                        entity.Status = Data.Enums.StatusType.New.ToString();
                        await _context.Issues.AddAsync(entity);
                        await _context.SaveChangesAsync();

                        if (await _context.Issues.AnyAsync(x => x.SourceId == redmineIssue.ParentIssueId))
                        {
                            var parentIssue = await _context.Issues.FirstOrDefaultAsync(x => x.SourceId == redmineIssue.ParentIssueId);
                            if (parentIssue != null)
                            {
                                entity.MainTask = parentIssue;
                            }

                            _context.Issues.Update(entity);
                            await _context.SaveChangesAsync();
                        }
                        entity.Project = project;
                        _context.Issues.Update(entity);
                        await _context.SaveChangesAsync();

                        addedOrUpdatedIssue = entity;
                    }
                }
                else
                {
                    var project = await _projectService.GetProjectBySourceIdAsync(redmineIssue.ProjectId);
                    if (project != null)
                    {
                        _mapper.Map(redmineIssue, existingIssue);
                        existingIssue.Project = project;
                        _context.Issues.Update(existingIssue);
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
            catch (Exception ex)
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

        public async Task<Issue> Create(Issue issue)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            await _context.AddAsync(issue);
            await _context.SaveChangesAsync();

            return issue;
        }

        public async Task Delete(Issue issue)
        {
            _context.Remove(issue);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Issue>> SearchInIssuesAndComments(string searchPhrase, long projectId)
        {
            var issues = await _context.Issues
                .AsNoTracking()
                .Where(x => x.Project.Id == projectId)
                .Where(x => x.Name.Contains(searchPhrase) || x.Description.Contains(searchPhrase)
                            || x.Comments.Any(s => s.Text.Contains(searchPhrase)))
                .ToListAsync();

            issues = await GetCommentEWithPhreaseForIssues(issues, searchPhrase);

            return issues;
        }

        private async Task<List<Issue>> GetCommentEWithPhreaseForIssues(List<Issue> issues, string searchPhrase)
        {
            var issuesIds = issues.Select(x => x.Id).ToList();
            var comments = await _commentService.GetCommentByIssuesIdsWithPhraseAsync(issuesIds, searchPhrase);

            foreach (var item in issues)
            {
                item.Comments = comments.Where(x => x.Issue.Id == item.Id).OrderBy(x => x.Date).ToList();
            }

            return issues;
        }
    }
}