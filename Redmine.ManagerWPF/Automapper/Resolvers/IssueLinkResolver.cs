using AutoMapper;
using Redmine.ManagerWPF.Data.Models;
using Redmine.ManagerWPF.Helpers;
using Redmine.ManagerWPF.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redmine.ManagerWPF.Desktop.Automapper.Resolvers
{
    public class IssueLinkResolver : IValueResolver<Integration.Models.IssueDto, Data.Models.Issue, string>
    {
        public string Resolve(IssueDto source, Issue destination, string destMember, ResolutionContext context)
        {
            string url = SettingsHelper.GetUrl();
            return $"{url}issues/{source.Id}";
        }
    }
}
