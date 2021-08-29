using AutoMapper;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Helpers;
using Redmine.ManagerWPF.Integration.Models;
using Redmine.Net.Api;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Redmine.ManagerWPF.Integration.Services
{
    public class IssueService : IService
    {
        private readonly IMapper _mapper;

        public IssueService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public IEnumerable<IssueDto> GetIssues()
        {
            List<IssueDto> issues = new List<IssueDto>();

            string host = SettingsHelper.GetUrl();
            string apiKey = SettingsHelper.GetApiKey();

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentNullException();

            var manager = new RedmineManager(host, apiKey);

            var parameters = new NameValueCollection { };
            var result = manager.GetObjects<Redmine.Net.Api.Types.Issue>(parameters);
            var allIssuesIds = result.Select(x => x.Id);

            foreach (var id in allIssuesIds)
            {
                var parametersForIssue = new NameValueCollection { { RedmineKeys.INCLUDE, RedmineKeys.JOURNALS }, { RedmineKeys.INCLUDE, RedmineKeys.RELATIONS } };
                var issue = manager.GetObject<Redmine.Net.Api.Types.Issue>(id.ToString(), parametersForIssue);

                var issueDto = _mapper.Map<IssueDto>(issue);

                issues.Add(issueDto);
            }

            return issues;
        }
    }
}