using CommunityToolkit.Mvvm.Messaging.Messages;
using Redmine.ManagerWPF.Desktop.Models.Tree;

namespace Redmine.ManagerWPF.Desktop.Messages.Forms
{
    public class ChangeSelectedCommentDoneStatus : ValueChangedMessage<TreeModel>
    {
        public ChangeSelectedCommentDoneStatus(TreeModel value) : base(value)
        {
        }
    }
}
