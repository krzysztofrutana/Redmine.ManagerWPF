using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Redmine.ManagerWPF.Desktop.Messages;
using Redmine.ManagerWPF.Desktop.Models.Projects;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;
using System.Diagnostics;
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
        private readonly IMessageBoxService _messageBoxService;

        public IRelayCommand OpenBrowserCommand { get; }

        public ProjectShortInfoViewModel()
        {
            _projectService = Ioc.Default.GetRequiredService<ProjectService>();
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();

            WeakReferenceMessenger.Default.Register<ProjectChangeMessage>(this, (r, m) =>
            {
                ReceiveProject(m.Value);
            });

            OpenBrowserCommand = new RelayCommand(OpenBrowser);
        }

        private async void ReceiveProject(ListItemModel value)
        {
            try
            {
                var project = await _projectService.GetProjectAsync(value.Id).ConfigureAwait(false);
                if (project != null)
                {
                    var formModel = _mapper.Map<DetailsModel>(project);
                    SelectedProject = formModel;
                }
            }
            catch (System.Exception ex)
            {
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy pobieraniu projektu do szczegółów");
            }
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