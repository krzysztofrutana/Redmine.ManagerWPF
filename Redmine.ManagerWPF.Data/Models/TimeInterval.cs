using System;

namespace Redmine.ManagerWPF.Data.Models
{
    public class TimeInterval
    {
        public int Id { get; set; }
        public DateTime TimeIntervalStart { get; set; }
        public DateTime TimeIntervalEnd { get; set; }
        public Issue Issue { get; set; }
        public Comment Comment { get; set; }
        public string Note { get; set; }
    }
}