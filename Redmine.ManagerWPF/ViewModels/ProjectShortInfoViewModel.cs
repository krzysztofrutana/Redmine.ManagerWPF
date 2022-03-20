using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Redmine.ManagerWPF.Desktop.Messages.ProjectCombobox;
using Redmine.ManagerWPF.Desktop.Models.Projects;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class ProjectShortInfoViewModel : ObservableRecipient
    {
        #region Properties
        private ListItemModel _selectedProject;

        public ListItemModel SelectedProject
        {
            get => _selectedProject;
            private set => SetProperty(ref _selectedProject, value);
        }
        #endregion

        #region Commands
        public IRelayCommand OpenBrowserCommand { get; }
        #endregion

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