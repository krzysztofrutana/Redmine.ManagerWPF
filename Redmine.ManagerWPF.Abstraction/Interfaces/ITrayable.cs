using System;
using System.Collections.Generic;
using System.Text;

namespace Redmine.ManagerWPF.Abstraction.Interfaces
{
    public interface ITrayable
    {
        void OpenFromTray();
        void CloseFromTray();
    }
}
