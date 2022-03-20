using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Redmine.ManagerWPF.Desktop.Messages.ProjectCombobox
{
    public class ProjectChangeMessage : ValueChangedMessage<Models.Projects.ListItemModel>
    {
        public ProjectChangeMessage(Models.Projects.ListItemModel node) : base(node)
        {
        }
    }
}