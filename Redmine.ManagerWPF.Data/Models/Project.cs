using Dapper;
using System;
using System.Collections.Generic;

namespace Redmine.ManagerWPF.Data.Models
{
    [Table("Projects")]
    public class Project
    {
        [Key]
        public int Id { get; set; }
        public int SourceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public DateTime DataStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public ICollection<Issue> Issues { get; set; }
        public string Status { get; set; }
    }
}