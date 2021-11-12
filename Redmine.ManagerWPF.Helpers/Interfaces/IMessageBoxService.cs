using System;
using System.Collections.Generic;
using System.Text;

namespace Redmine.ManagerWPF.Helpers.Interfaces
{
    public interface IMessageBoxService
    {
        void ShowWarningInfoBox(string text, string caption);
    }
}
