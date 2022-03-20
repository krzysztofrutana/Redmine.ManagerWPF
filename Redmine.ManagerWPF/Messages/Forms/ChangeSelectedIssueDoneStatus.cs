using CommunityToolkit.Mvvm.Messaging.Messages;
using Redmine.ManagerWPF.Desktop.Models.Tree;

namespace Redmine.ManagerWPF.Desktop.Messages.Forms
{
    public class ChangeSelectedIssueDoneStatus : ValueChangedMessage<TreeModel>
    {
        public ChangeSelectedIssueDoneStatus(TreeModel value) : base(value)
        {
        }
    }
}
