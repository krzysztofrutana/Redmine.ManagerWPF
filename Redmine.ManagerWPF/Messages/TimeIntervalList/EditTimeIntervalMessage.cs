using CommunityToolkit.Mvvm.Messaging.Messages;
using Redmine.ManagerWPF.Desktop.Models.TimeIntervals;

namespace Redmine.ManagerWPF.Desktop.Messages.TimeIntervalList
{
    public class EditTimeIntervalMessage : ValueChangedMessage<ListItemModel>
    {
        public EditTimeIntervalMessage(ListItemModel value) : base(value)
        {
        }
    }
}