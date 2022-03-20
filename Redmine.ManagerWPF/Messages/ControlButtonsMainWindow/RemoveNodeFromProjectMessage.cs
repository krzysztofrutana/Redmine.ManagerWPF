using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Redmine.ManagerWPF.Desktop.Messages.ControlButtonsMainWindow
{
    public class RemoveNodeFromProjectMessage : ValueChangedMessage<Models.Tree.TreeModel>
    {
        public RemoveNodeFromProjectMessage(Models.Tree.TreeModel value) : base(value)
        {
        }
    }
}