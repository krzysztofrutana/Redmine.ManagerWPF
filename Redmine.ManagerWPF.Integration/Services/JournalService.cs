using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using AutoMapper;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Helpers;
using Redmine.ManagerWPF.Integration.Models;
using Redmine.Net.Api;

namespace Redmine.ManagerWPF.Integration.Services
{
    public class JournalService : IService
    {
        private readonly IMapper _mapper;

        public JournalService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public Task<IEnumerable<JournalDto>> GetIssueJournals(IssueDto issue)
        {
            return Task.Run(() =>
            {
                string host = SettingsHelper.GetUrl();
                string apiKey = SettingsHelper.GetApiKey();

                var manager = new RedmineManager(host, apiKey);

                var parametersForIssue = new NameValueCollection { { RedmineKeys.INCLUDE, RedmineKeys.JOURNALS }, { RedmineKeys.INCLUDE, RedmineKeys.RELATIONS } };
                var redmineIssue = manager.GetObject<Redmine.Net.Api.Types.Issue>(issue.Id.ToString(), parametersForIssue);

                return _mapper.Map<IEnumerable<JournalDto>>(redmineIssue.Journals);
            });
        }

        public Task<IEnumerable<JournalDto>> GetIssueJournals(long issueOriginalId)
        {
            return Task.Run(() =>
            {
                string host = SettingsHelper.GetUrl();
                string apiKey = SettingsHelper.GetApiKey();

                var manager = new RedmineManager(host, apiKey);

                var parametersForIssue = new NameValueCollection { { RedmineKeys.INCLUDE, RedmineKeys.JOURNALS }, { RedmineKeys.INCLUDE, RedmineKeys.RELATIONS } };
                var redmineIssue = manager.GetObject<Redmine.Net.Api.Types.Issue>(issueOriginalId.ToString(), parametersForIssue);

                return _mapper.Map<IEnumerable<JournalDto>>(redmineIssue.Journals);
            });
        }
    }
}