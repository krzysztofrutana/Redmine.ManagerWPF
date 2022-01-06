using AutoMapper;
using Dapper;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Data;
using Redmine.ManagerWPF.Data.Dapper;
using Redmine.ManagerWPF.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.Services
{
    public class IssueService : IService
    {
        private readonly IContext _context;
        private readonly IMapper _mapper;
        private readonly CommentService _commentService;
        private readonly ProjectService _projectService;

        public IssueService(
            IContext context,
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
            using var context = await _context.GetConnectionAsync();

            var query = @"SELECT * FROM [dbo].[Issues] issues
                          LEFT JOIN [dbo].[Issues] mainTasks ON issues.MainTaskId = mainTasks.Id
                          WHERE issues.ProjectId = @projectId";

            var issues = await context.QueryAsync<Issue, Issue, Issue>(query, (issue, mainTask) =>
            {
                if (mainTask != null)
                {
                    issue.MainTask = mainTask;
                }
                return issue;
            },
            new { projectId = projectId });

            issues = await GetCommentForIssues(issues.ToList());

            var mainIssues = issues.Where(x => x.MainTask == null).OrderBy(x => x.Name).ToList();

            CreateTree(mainIssues, issues.ToList());

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

        public async Task<Issue> GetIssueAsync(int id)
        {
            using var context = await _context.GetConnectionAsync();

            return await context.GetAsync<Issue>(id);
        }


        public async Task<Issue> GetIssueWithTimeIntervalAsync(int id)
        {
            using var context = await _context.GetConnectionAsync();

            var issue = await context.GetAsync<Issue>(id);

            if (issue != null)
            {
                var timeIntervals = await context.GetListAsync<TimeInterval>(new { IssueId = id });
                issue.TimesForIssue = timeIntervals.ToList();
            }

            return issue;
        }

        public async Task<IEnumerable<Issue>> GetAllIssueAsync()
        {
            using var context = await _context.GetConnectionAsync();

            return await context.GetListAsync<Issue>();
        }

        public async Task Update(Issue entity)
        {
            using var context = await _context.GetConnectionAsync();

            await context.UpdateAsync(entity);
        }

        public async Task SynchronizeIssues(Integration.Models.IssueDto redmineIssue)
        {
            try
            {
                using var context = await _context.GetConnectionAsync();

                var query = @"SELECT * FROM [dbo].[Issues] WHERE SourceId = @id";

                var existingIssue = await context.QueryFirstOrDefaultAsync<Issue>(query, new { id = redmineIssue.Id });

                Issue addedOrUpdatedIssue = null;

                if (existingIssue == null)
                {
                    var entity = _mapper.Map<Issue>(redmineIssue);

                    var project = await _projectService.GetProjectBySourceIdAsync(redmineIssue.ProjectId);
                    if (project != null)
                    {
                        entity.Status = Data.Enums.StatusType.New.ToString();
                        await context.InsertAsync(entity);

                        var checkMainIssue = await context.QueryFirstOrDefaultAsync<Issue>(query, new { id = redmineIssue.ParentIssueId });
                        if (checkMainIssue != null)
                        {
                            entity.MainTask = checkMainIssue;

                        }

                        entity.Project = project;
                        await context.UpdateAsync(entity);

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

                        await context.UpdateAsync(existingIssue);

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
            using var context = await _context.GetConnectionAsync();

            var query = @"SELECT * FROM [dbo].[Issues] WHERE SourceId = @id";

            var parentIssue = await context.QueryFirstOrDefaultAsync<Issue>(query, new { id = redmineIssue.ParentIssueId });
            if (parentIssue != null)
            {
                issue.MainTask = parentIssue;
                await context.UpdateAsync(issue);
            }
        }

        public async Task<Issue> Create(Issue issue)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            using var context = await _context.GetConnectionAsync();

            await context.InsertAsync(issue);

            return issue;
        }

        public async Task Delete(Issue issue)
        {
            using var context = await _context.GetConnectionAsync();
            await context.DeleteAsync(issue);
        }

        public async Task<List<Issue>> SearchInIssuesAndComments(string searchPhrase, long projectId)
        {
            using var context = await _context.GetConnectionAsync();

            var query = @$"SELECT * FROM [dbo].[Issues] issues 
                          LEFT JOIN [dbo].[Comments] comments ON comments.[IssueId] = issues.[Id] AND comments.[Text] LIKE '%{searchPhrase}%'
                          WHERE [ProjectId] = @projectId AND ([Name] LIKE '%{searchPhrase}%' OR [Description] LIKE '%{searchPhrase}%' OR comments.[Text] LIKE '%{searchPhrase}%')";

            var issues = await context.QueryAsync<Issue, Comment, Issue>(query, (issue, comment) =>
                {
                    if (comment != null)
                    {
                        if (issue.Comments == null)
                        {
                            issue.Comments = new List<Comment>();
                            issue.Comments.Add(comment);
                        }
                        if (issue.Comments != null && !issue.Comments.Any(s => s.Id == comment.Id))
                        {
                            issue.Comments.Add(comment);
                        }
                    }

                    return issue;
                },
                new { projectId = projectId });

            var groupedIssues = issues.GroupBy(x => x.Id);

            List<Issue> result = new List<Issue>();

            foreach (var group in groupedIssues)
            {
                var issue = group.First();

                var comments = issues.Where(x => x.Id == group.Key && x.Comments != null).SelectMany(x => x.Comments);
                if (comments != null && comments.Any())
                {
                    if (issue.Comments == null)
                    {
                        issue.Comments = new List<Comment>();
                    }

                    issue.Comments = comments.ToList();
                }


                result.Add(issue);
            }

            return result.ToList();
        }

        private async Task<List<Issue>> GetCommentWithPhreaseForIssues(List<Issue> issues, string searchPhrase)
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