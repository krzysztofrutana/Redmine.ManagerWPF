using System;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Redmine.ManagerWPF.Desktop.Messages.BreakReason
{
    public class BreakReasonTimeMessage : ValueChangedMessage<TimeSpan>
    {
        public BreakReasonTimeMessage(TimeSpan elapsedTime) : base(elapsedTime)
        {

        }
    }
}
