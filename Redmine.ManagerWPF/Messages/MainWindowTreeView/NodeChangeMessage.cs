using CommunityToolkit.Mvvm.Messaging.Messages;
using Redmine.ManagerWPF.Desktop.Models.Tree;

namespace Redmine.ManagerWPF.Desktop.Messages.MainWindowTreeView
{
    public class NodeChangeMessage : ValueChangedMessage<TreeModel>
    {
        public NodeChangeMessage(TreeModel node) : base(node)
        {
        }
    }
}