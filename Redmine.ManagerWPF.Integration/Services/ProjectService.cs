using AutoMapper;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Helpers;
using Redmine.ManagerWPF.Integration.Models;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Redmine.ManagerWPF.Integration.Services
{
    public class ProjectService : IService
    {
        private readonly IMapper _mapper;

        public ProjectService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public IEnumerable<ProjectDto> GetProjects()
        {
            string url = SettingsHelper.GetUrl();
            string apiKey = SettingsHelper.GetApiKey();

            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentNullException();

            var manager = new RedmineManager(url, apiKey);

            var parameters = new NameValueCollection { { RedmineKeys.INCLUDE, RedmineKeys.ISSUE_CATEGORIES } };
            var result = manager.GetObjects<Project>(parameters);

            return _mapper.Map<IEnumerable<ProjectDto>>(result);
        }
    }
}