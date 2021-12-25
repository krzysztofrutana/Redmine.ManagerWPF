using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redmine.ManagerWPF.Desktop.Messages
{
    public class AddIssueToProjectMessage : ValueChangedMessage<Data.Models.Issue>
    {
        public AddIssueToProjectMessage(Data.Models.Issue value) : base(value)
        {
        }
    }
}