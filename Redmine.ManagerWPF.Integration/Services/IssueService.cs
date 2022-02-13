using System;
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
    public class IssueService : IService
    {
        private readonly IMapper _mapper;

        public IssueService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public Task<List<IssueDto>> GetIssues()
        {
            return Task.Run(() =>
            {
                string host = SettingsHelper.GetUrl();
                string apiKey = SettingsHelper.GetApiKey();

                if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(apiKey))
                    throw new ArgumentNullException();

                var manager = new RedmineManager(host, apiKey);

                var parameters = new NameValueCollection { };
                var result = manager.GetObjects<Redmine.Net.Api.Types.Issue>(parameters);

                var issues = _mapper.Map<List<IssueDto>>(result);

                return issues;
            });
        }

        public Task<IssueDto> GetIssue(long issueOriginalId)
        {
            return Task.Run(() =>
            {
                string host = SettingsHelper.GetUrl();
                string apiKey = SettingsHelper.GetApiKey();

                if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(apiKey))
                    throw new ArgumentNullException();

                var manager = new RedmineManager(host, apiKey);

                var parameters = new NameValueCollection { };
                var result = manager.GetObject<Redmine.Net.Api.Types.Issue>(issueOriginalId.ToString(), parameters);

                var issues = _mapper.Map<IssueDto>(result);

                return issues;
            });
        }
    }
}