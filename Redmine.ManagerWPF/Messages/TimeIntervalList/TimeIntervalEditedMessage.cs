using CommunityToolkit.Mvvm.Messaging.Messages;
using Redmine.ManagerWPF.Desktop.Models.TimeIntervals;

namespace Redmine.ManagerWPF.Desktop.Messages.TimeIntervalList
{
    public class TimeIntervalEditedMessage : ValueChangedMessage<ListItemModel>
    {
        public TimeIntervalEditedMessage(ListItemModel value) : base(value)
        {
        }
    }
}