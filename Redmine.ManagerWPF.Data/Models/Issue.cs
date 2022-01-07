using Dapper;
using System.Collections.Generic;

namespace Redmine.ManagerWPF.Data.Models
{
    [Table("Issues")]
    public class Issue
    {
        [Key]
        public int Id { get; set; }
        public int SourceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public Issue MainTask { get; set; }
        public int? MainTaskId { get; set; }
        public ICollection<TimeInterval> TimesForIssue { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Issue> Issues { get; set; }
        public Project Project { get; set; }
        public int ProjectId { get; set; }
        public string Status { get; set; }
        public bool Done { get; set; }
    }
}