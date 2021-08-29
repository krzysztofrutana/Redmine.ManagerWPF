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

        public async Task SynchronizeProjects()
        {
            var redmineProjects = _projectService.GetProjects();
            var redmineProjectsIds = redmineProjects.Select(x => x.Id);

            var existingProjects = _context.Projects.Where(x => redmineProjectsIds.Contains(x.SourceId)).ToList();
            var existingProjectsSourceId = existingProjects.Select(X => X.SourceId).ToList();

            var entities = _mapper.Map<IEnumerable<Project>>(redmineProjects);

            var entitiesToAdd = new List<Project>();
            var entitiesToUpdate = new List<Project>();
            foreach (var item in entities)
            {
                if (existingProjectsSourceId.Contains(item.SourceId))
                {
                    entitiesToUpdate.Add(item);
                }
                else
                {
                    item.Status = Data.Enums.StatusType.New.ToString();
                    entitiesToAdd.Add(item);
                }
            }

            _context.UpdateRange(entitiesToUpdate);
            _context.AddRange(entitiesToAdd);
            await _context.SaveChangesAsync();
        }
    }
}