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
        private ListItemModel _selectedProject;

        public ListItemModel SelectedProject
        {
            get { return _selectedProject; }
            set { SetProperty(ref _selectedProject, value); }
        }

        public IRelayCommand OpenBrowserCommand { get; }

        public ProjectShortInfoViewModel()
        {
            WeakReferenceMessenger.Default.Register<ProjectChangeMessage>(this, (r, m) =>
            {
                if (m.Value != null)
                {
                    SelectedProject = m.Value;
                }
            });

            OpenBrowserCommand = new RelayCommand(OpenBrowser);
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