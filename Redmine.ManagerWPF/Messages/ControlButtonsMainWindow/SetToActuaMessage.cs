using CommunityToolkit.Mvvm.Messaging.Messages;
using Redmine.ManagerWPF.Desktop.Models.Tree;

namespace Redmine.ManagerWPF.Desktop.Messages.RemoveNodeFromProjectMessage
{
    public class SetToActuaMessage : ValueChangedMessage<TreeModel>
    {
        public SetToActuaMessage(TreeModel node) : base(node)
        {
        }
    }
}