using AutoMapper;
using Redmine.ManagerWPF.Data.Models;
using Redmine.ManagerWPF.Helpers;
using Redmine.ManagerWPF.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redmine.ManagerWPF.Desktop.Automapper.Resolvers
{
    public class ProjectLinkResolver : IValueResolver<Integration.Models.ProjectDto, Data.Models.Project, string>
    {
        public string Resolve(ProjectDto source, Project destination, string destMember, ResolutionContext context)
        {
            string url = SettingsHelper.GetUrl();
            return $"{url}projects/{source.Identifier}";
        }
    }
}
