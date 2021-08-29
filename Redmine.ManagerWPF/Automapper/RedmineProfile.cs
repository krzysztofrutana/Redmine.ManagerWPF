using AutoMapper;
using Redmine.ManagerWPF.Integration.Models;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.Automapper
{
    public class RedmineProfile : Profile
    {
        public RedmineProfile()
        {
            Projects();
            Issues();
            Journals();
        }

        private void Projects()
        {
            CreateMap<Project, ProjectDto>();
            CreateMap<IdentifiableName, Integration.Models.ProjectDto>();
        }

        private void Issues()
        {
            CreateMap<Issue, Integration.Models.IssueDto>()
                .ForMember(x => x.Comments, m => m.MapFrom(s => s.Journals.Where(x => !string.IsNullOrWhiteSpace(x.Notes))))
                .ForMember(X => X.Status, m => m.MapFrom(s => s.Status.Name))
                .ForMember(X => X.ProjectId, m => m.MapFrom(s => s.Project.Id))
                .ForMember(x => x.ParentIssueId, m => m.MapFrom(s => s.ParentIssue.Id));
        }

        private void Journals()
        {
            CreateMap<Journal, Integration.Models.JournalDto>()
                .ForMember(X => X.Id, m => m.MapFrom(s => s.Id))
                .ForMember(x => x.CreatedBy, m => m.MapFrom(s => s.User.Name))
                .ForMember(X => X.Date, m => m.MapFrom(s => s.CreatedOn))
                .ForMember(x => x.Text, m => m.MapFrom(s => s.Notes));
        }
    }
}