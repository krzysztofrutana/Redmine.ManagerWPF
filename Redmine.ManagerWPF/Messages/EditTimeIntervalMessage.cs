using CommunityToolkit.Mvvm.Messaging.Messages;
using Redmine.ManagerWPF.Desktop.Models.TimeIntervals;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redmine.ManagerWPF.Desktop.Messages
{
    public class EditTimeIntervalMessage : ValueChangedMessage<ListItemModel>
    {
        public EditTimeIntervalMessage(ListItemModel value) : base(value)
        {
        }
    }
}