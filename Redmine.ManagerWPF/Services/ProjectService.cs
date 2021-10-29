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
    public class ProjectService : IService
    {
        private readonly Context _context;
        private readonly Integration.Services.ProjectService _projectService;
        private readonly IMapper _mapper;

        public ProjectService(Context context, Integration.Services.ProjectService projectService, IMapper mapper)
        {
            _context = context;
            _projectService = projectService;
            _mapper = mapper;
        }

        public Task<List<Project>> GetProjectsAsync()
        {
            var result = _context.Projects.ToListAsync();

            return result;
        }

        public Task<Project> GetProjectAsync(int id)
        {
            return _context.Projects.Where(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task SynchronizeProjects(Integration.Models.ProjectDto redmineProject)
        {
            var redmineProjectIds = redmineProject.Id;

            var existingProject = _context.Projects.FirstOrDefault(x => x.SourceId == redmineProjectIds);

            if (existingProject == null)
            {
                var entity = _mapper.Map<Project>(redmineProject);
                entity.Status = Data.Enums.StatusType.New.ToString();
                _context.Add(entity);
            }
            else
            {
                _mapper.Map(redmineProject, existingProject);
                _context.Update(existingProject);
            }

            await _context.SaveChangesAsync();
        }
    }
}