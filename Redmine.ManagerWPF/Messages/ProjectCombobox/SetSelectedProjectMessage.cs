using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Redmine.ManagerWPF.Desktop.Messages.ProjectCombobox
{
    public class SetSelectedProjectMessage : ValueChangedMessage<Models.Projects.ListItemModel>
    {
        public SetSelectedProjectMessage(Models.Projects.ListItemModel value) : base(value)
        {
        }
    }
}