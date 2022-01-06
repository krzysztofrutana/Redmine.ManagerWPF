using Dapper;
using System;

namespace Redmine.ManagerWPF.Data.Models
{
    [Table("TimeIntervals")]
    public class TimeInterval
    {
        [Key]
        public int Id { get; set; }
        public DateTime? TimeIntervalStart { get; set; }
        public DateTime? TimeIntervalEnd { get; set; }
        public Issue Issue { get; set; }
        public Comment Comment { get; set; }
        public string Note { get; set; }

        [NotMapped]
        public bool IsStarted
        {
            get
            {
                return TimeIntervalStart.HasValue && !TimeIntervalEnd.HasValue;
            }
            set { }
        }

        [NotMapped]
        public bool IsEnd
        {
            get
            {
                return TimeIntervalStart.HasValue && TimeIntervalEnd.HasValue;
            }
            set { }
        }
    }
}