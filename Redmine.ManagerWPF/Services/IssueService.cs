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
        private readonly Integration.Services.IssueService _integrationIssueService;
        private readonly IMapper _mapper;
        private readonly ProjectService _projectService;
        private readonly CommentService _commentService;

        public IssueService(
            Context context,
            Integration.Services.IssueService integrationIssueService,
            IMapper mapper,
            ProjectService projectService,
            CommentService commentService)
        {
            _context = context;
            _integrationIssueService = integrationIssueService;
            _mapper = mapper;
            _projectService = projectService;
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
            var comments = await _commentService.GetCommentByIssuesIds(issuesIds);

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

        public async Task SynchronizeIssues()
        {
            var allIssues = _integrationIssueService.GetIssues();

            if (allIssues.Any())
            {
                var allExistingIssues = _context.Issues.ToList();

                var resultSourceIds = allIssues.Select(x => x.Id).ToList();

                var sourceIdsToUpdate = allExistingIssues.Where(x => resultSourceIds.Contains(x.SourceId)).ToList();

                var allProjects = await _projectService.GetProjectsAsync();

                // first update parrent
                var parentResult = allIssues.Where(x => x.ParentIssueId == null).ToList();

                await AddIssue(allIssues, allProjects, parentResult);
            }
        }

        private async Task AddIssue(IEnumerable<Integration.Models.IssueDto> allIssues, List<Project> allProjects, List<Integration.Models.IssueDto> parentIssues)
        {
            if (allIssues.Count() > 0)
            {
                var addedSourceIds = new List<int>();
                foreach (var item in parentIssues)
                {
                    var itemToAdd = _mapper.Map<Issue>(item);
                    var project = allProjects.Where(x => x.SourceId == item.ProjectId).FirstOrDefault();
                    if (project != null)
                    {
                        itemToAdd.Project = project;
                    }

                    if (item.ParentIssueId != null)
                    {
                        var parent = _context.Issues.Where(x => x.SourceId == item.ParentIssueId).FirstOrDefault();
                        if (parent != null)
                        {
                            itemToAdd.MainTask = parent;
                        }
                    }

                    _context.Add(itemToAdd);
                    await _context.SaveChangesAsync();

                    foreach (var commentDto in item.Comments)
                    {
                        var comment = _mapper.Map<Comment>(commentDto);
                        comment.Issue = itemToAdd;
                        _context.Add(comment);
                    }

                    await _context.SaveChangesAsync();

                    addedSourceIds.Add(item.Id);
                }
                var issuesWithoutParent = allIssues.Where(x => !addedSourceIds.Contains(x.Id)).ToList();
                var newParent = issuesWithoutParent.Where(x => x.ParentIssueId.HasValue && addedSourceIds.Contains(x.ParentIssueId.Value)).ToList();

                await AddIssue(issuesWithoutParent, allProjects, newParent);
            }
            else
            {
                return;
            }
        }
    }
}