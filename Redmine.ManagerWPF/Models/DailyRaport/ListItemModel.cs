using System;
using System.Collections.Generic;
using System.Text;

namespace Redmine.ManagerWPF.Desktop.Models.DailyRaport
{
    public class ListItemModel
    {
        public long IssueId { get; set; }
        public string IssueName { get; set; }
        public TimeSpan Time { get; set; }
        public string TimeFormatted { get; set; }

    }
}
