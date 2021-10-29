using System;
using System.Collections.Generic;
using System.Text;

namespace Redmine.ManagerWPF.Desktop.Models.TimeIntervals
{
    public class ListItemModel
    {
        public int Id { get; set; }
        public DateTime? StartDate { get; set; }
        public string CountedTime { get; set; }
        public DateTime? EndDate { get; set; }
        public string Note { get; set; }
        public bool IsStarted { get; set; }
    }
}
