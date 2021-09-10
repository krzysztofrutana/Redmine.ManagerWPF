using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Redmine.ManagerWPF.Data.Models;
using Redmine.ManagerWPF.Desktop.Messages;
using Redmine.ManagerWPF.Desktop.Models.Projects;
using Redmine.ManagerWPF.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class ProjectShortInfoViewModel : ObservableRecipient
    {
        private DetailsModel _selectedProject;

        public DetailsModel SelectedProject
        {
            get { return _selectedProject; }
            set { SetProperty(ref _selectedProject, value); }
        }

        private readonly ProjectService _projectService;
        private readonly IMapper _mapper;

        public IRelayCommand OpenBrowserCommand { get; }

        public ProjectShortInfoViewModel()
        {
            _projectService = Ioc.Default.GetRequiredService<ProjectService>();
            _mapper = Ioc.Default.GetRequiredService<IMapper>();

            WeakReferenceMessenger.Default.Register<ProjectChangeMessage>(this, (r, m) =>
            {
                ReceiveProject(m.Value);
            });

            OpenBrowserCommand = new RelayCommand(OpenBrowser);
        }

        private void ReceiveProject(ListItemModel value)
        {
            Task.Run(async () =>
            {
                var project = await _projectService.GetProjectAsync(value.Id);
                if (project != null)
                {
                    var formModel = _mapper.Map<DetailsModel>(project);
                    SelectedProject = formModel;
                }
            });
        }

        private void OpenBrowser()
        {
            var psi = new ProcessStartInfo
            {
                FileName = SelectedProject.Link,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}
