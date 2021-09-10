using AutoMapper;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Helpers;
using Redmine.ManagerWPF.Integration.Models;
using Redmine.Net.Api;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Integration.Services
{
    public class IssueService : IService
    {
        private readonly IMapper _mapper;

        public IssueService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public Task<List<IssueDto>> GetIssues()
        {
            string host = SettingsHelper.GetUrl();
            string apiKey = SettingsHelper.GetApiKey();

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentNullException();

            var manager = new RedmineManager(host, apiKey);

            var parameters = new NameValueCollection { };
            var result = manager.GetObjects<Redmine.Net.Api.Types.Issue>(parameters);

            var issues = _mapper.Map<List<IssueDto>>(result);

            return Task.FromResult(issues);
        }

        public Task<IssueDto> GetIssueJournals(IssueDto issue)
        {
            string host = SettingsHelper.GetUrl();
            string apiKey = SettingsHelper.GetApiKey();

            var manager = new RedmineManager(host, apiKey);

            var parametersForIssue = new NameValueCollection { { RedmineKeys.INCLUDE, RedmineKeys.JOURNALS }, { RedmineKeys.INCLUDE, RedmineKeys.RELATIONS } };
            var redmineIssue = manager.GetObject<Redmine.Net.Api.Types.Issue>(issue.Id.ToString(), parametersForIssue);

            issue.Comments = _mapper.Map<IEnumerable<JournalDto>>(redmineIssue.Journals);

            return Task.FromResult(issue);
        }
    }
}