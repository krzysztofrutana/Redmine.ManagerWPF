using CommunityToolkit.Mvvm.Messaging.Messages;
using Redmine.ManagerWPF.Desktop.Models.Tree;

namespace Redmine.ManagerWPF.Desktop.Messages.MainWindowTreeView
{
    public class ReloadTreeMessage : ValueChangedMessage<TreeModel>
    {
        public ReloadTreeMessage(TreeModel node) : base(node)
        {
        }
    }
}
