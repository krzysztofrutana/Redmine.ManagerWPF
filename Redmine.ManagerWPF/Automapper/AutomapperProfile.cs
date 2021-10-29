using AutoMapper;
using Redmine.ManagerWPF.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.Automapper
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            Projects();
            Issues();
            Comment();
            TimeInterval();
        }

        private void Projects()
        {
            CreateMap<Project, Models.Projects.ListItemModel>()
                .ForMember(X => X.DataStart, m => m.MapFrom(s => s.DataStart.ToString("dd-MM-yyyy")));

            CreateMap<Integration.Models.ProjectDto, Project>()
                .ForMember(X => X.Id, m => m.Ignore())
                .ForMember(x => x.SourceId, m => m.MapFrom(s => s.Id))
                .ForMember(X => X.Name, m => m.MapFrom(s => s.Name))
                .ForMember(x => x.Discription, m => m.MapFrom(s => s.Description))
                .ForMember(x => x.DataStart, m => m.MapFrom(s => s.CreatedOn))
                .ForMember(x => x.Link, m => m.MapFrom<Resolvers.ProjectLinkResolver>());

            CreateMap<Project, Models.Projects.DetailsModel>();
        }

        private void Issues()
        {
            CreateMap<Integration.Models.IssueDto, Issue>()
                .ForMember(x => x.SourceId, m => m.MapFrom(s => s.Id))
                .ForMember(X => X.Name, m => m.MapFrom(s => s.Subject))
                .ForMember(x => x.Status, m => m.MapFrom(S => S.Status))
                .ForMember(x => x.Id, m => m.Ignore())
                .ForMember(x => x.Description, m => m.MapFrom(s => s.Description))
                .ForMember(x => x.Comments, m => m.Ignore())
                .ForMember(x => x.Link, m => m.MapFrom<Resolvers.IssueLinkResolver>());

            CreateMap<Issue, Models.Tree.TreeModel>()
                .ForMember(x => x.Type, m => m.MapFrom<Resolvers.TreeModelTypeResolver>())
                .ForMember(X => X.Children, m => m.MapFrom<Resolvers.TreeChildrenResolver>());

            CreateMap<Issue, Models.Issues.FormModel>();
        }

        private void Comment()
        {
            CreateMap<Integration.Models.JournalDto, Comment>()
                .ForMember(X => X.Id, m => m.Ignore())
                .ForMember(x => x.SourceId, m => m.MapFrom(s => s.Id));

            CreateMap<Comment, Models.Tree.TreeModel>()
                .ForMember(x => x.Name, m => m.MapFrom(s => s.CreatedBy + ' ' + s.Date.ToString("dd-MM-yyyy HH:mm")))
               .ForMember(x => x.Type, m => m.MapFrom<Resolvers.TreeModelTypeResolver>());

            CreateMap<Comment, Models.Comments.FormModel>();
        }

        private void TimeInterval()
        {
            CreateMap<TimeInterval, Models.TimeIntervals.ListItemModel>()
                .ForMember(x => x.StartDate, m => m.MapFrom(s => s.TimeIntervalStart))
                .ForMember(x => x.EndDate, m => m.MapFrom(s => s.TimeIntervalEnd))
                .ForMember(x => x.CountedTime, m => m.Ignore());
        }
    }
}