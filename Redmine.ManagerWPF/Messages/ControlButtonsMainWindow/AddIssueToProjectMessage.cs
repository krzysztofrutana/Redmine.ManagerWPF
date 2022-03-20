using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Redmine.ManagerWPF.Desktop.Messages.ControlButtonsMainWindow
{
    public class AddIssueToProjectMessage : ValueChangedMessage<Data.Models.Issue>
    {
        public AddIssueToProjectMessage(Data.Models.Issue value) : base(value)
        {
        }
    }
}