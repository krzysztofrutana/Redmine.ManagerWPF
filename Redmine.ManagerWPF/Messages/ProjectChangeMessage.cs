using CommunityToolkit.Mvvm.Messaging.Messages;
using Redmine.ManagerWPF.Desktop.Models.Tree;

namespace Redmine.ManagerWPF.Desktop.Messages
{
    public class ProjectChangeMessage : ValueChangedMessage<Models.Projects.ListItemModel>
    {
        public ProjectChangeMessage(Models.Projects.ListItemModel node) : base(node)
        {
        }
    }
}