using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redmine.ManagerWPF.Desktop.Messages
{
    public class SelectedProjectMessage : ValueChangedMessage<Models.Projects.ListItemModel>
    {
        public SelectedProjectMessage(Models.Projects.ListItemModel value) : base(value)
        {
        }
    }
}