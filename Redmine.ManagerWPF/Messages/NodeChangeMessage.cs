﻿using CommunityToolkit.Mvvm.Messaging.Messages;
using Redmine.ManagerWPF.Desktop.Models.Tree;

namespace Redmine.ManagerWPF.Desktop.Messages
{
    public class InformationLoadedMessage : ValueChangedMessage<TreeModel>
    {
        public InformationLoadedMessage(TreeModel node) : base(node)
        {
        }
    }
}