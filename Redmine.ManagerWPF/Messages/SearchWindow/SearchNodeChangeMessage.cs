using CommunityToolkit.Mvvm.Messaging.Messages;
using Redmine.ManagerWPF.Desktop.Models.Tree;

namespace Redmine.ManagerWPF.Desktop.Messages.SearchWindow
{
    public class SearchNodeChangeMessage : ValueChangedMessage<TreeModel>
    {
        public SearchNodeChangeMessage(TreeModel node) : base(node)
        {
        }
    }
}