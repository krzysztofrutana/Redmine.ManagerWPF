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
    public class ProjectService : IService
    {
        private readonly IContext _context;
        private readonly IMapper _mapper;

        public ProjectService(IContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Project>> GetProjectsAsync()
        {
            using var context = await _context.GetConnectionAsync();
            return await context.GetListAsync<Project>();
        }

        public async Task<Project> GetProjectAsync(int id)
        {
            using var context = await _context.GetConnectionAsync();
            return await context.GetAsync<Project>(id);
        }

        public async Task SynchronizeProjects(Integration.Models.ProjectDto redmineProject)
        {
            using var context = await _context.GetConnectionAsync();

            var query = @"SELECT * FROM [dbo].[Projects] WHERE SourceId = @id";

            var existingProject = await context.QueryFirstOrDefaultAsync<Project>(query, new { id = redmineProject.Id });

            if (existingProject == null)
            {
                var entity = _mapper.Map<Project>(redmineProject);
                entity.Status = Data.Enums.StatusType.New.ToString();
                await context.InsertAsync<long, Project>(entity);
            }
            else
            {
                _mapper.Map(redmineProject, existingProject);
                await context.UpdateAsync(existingProject);
            }
        }

        public async Task<Project> GetProjectBySourceIdAsync(int projectSourceId)
        {
            using var context = await _context.GetConnectionAsync();

            var query = @"SELECT * FROM [dbo].[Projects] WHERE SourceId = @id";

            return await context.QueryFirstOrDefaultAsync<Project>(query, new { id = projectSourceId });
        }
    }
}