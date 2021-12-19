using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redmine.ManagerWPF.Desktop.Models.DailyRaport
{
    public class ListItemModel : ObservableObject
    {
        public string TotalTime { get; set; }
        public TimeSpan TotalTimeTimeSpan { get; set; }
        public string Name { get; set; }
    }
}
