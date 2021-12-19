using CommunityToolkit.Mvvm.Messaging.Messages;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redmine.ManagerWPF.Desktop.Messages
{
    public class ChangeSelectedIssueDoneStatus : ValueChangedMessage<TreeModel>
    {
        public ChangeSelectedIssueDoneStatus(TreeModel value) : base(value)
        {
        }
    }
}
