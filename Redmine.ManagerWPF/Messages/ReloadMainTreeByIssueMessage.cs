using CommunityToolkit.Mvvm.Messaging.Messages;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redmine.ManagerWPF.Desktop.Messages
{
    public class ChangeSelectedCommentDoneStatus : ValueChangedMessage<TreeModel>
    {
        public ChangeSelectedCommentDoneStatus(TreeModel value) : base(value)
        {
        }
    }
}
