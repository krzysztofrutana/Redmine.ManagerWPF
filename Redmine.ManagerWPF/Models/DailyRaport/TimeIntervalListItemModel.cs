using Redmine.ManagerWPF.Data.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redmine.ManagerWPF.Desktop.Models.DailyRaport
{
    public class TimeIntervalListItemModel
    {
        public long TimeIntervalId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Time { get; set; }
        public string ToComment { get; set; }
        public string Note { get; set; }
        public string Type { get; set; } = nameof(ObjectType.TimeInterval);

    }
}
