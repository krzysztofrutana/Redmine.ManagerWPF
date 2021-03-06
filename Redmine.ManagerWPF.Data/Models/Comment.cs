using Dapper;
using System;
using System.Collections.Generic;

namespace Redmine.ManagerWPF.Data.Models
{
    [Table("Comments")]
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        public int SourceId { get; set; }
        public string Text { get; set; }
        public string Link { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Date { get; set; }
        public ICollection<TimeInterval> TimeForComment { get; set; }
        public Issue Issue { get; set; }
        public int IssueId { get; set; }
        public bool Done { get; set; }
    }
}