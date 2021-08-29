using System;
using System.Collections.Generic;

namespace Redmine.ManagerWPF.Data.Models
{
    public class Project
    {
        public int Id { get; set; }
        public int SourceId { get; set; }
        public string Name { get; set; }
        public string Discription { get; set; }
        public string Link { get; set; }
        public DateTime DataStart { get; set; }
        public DateTime DateEnd { get; set; }
        public ICollection<Issue> Issues { get; set; }
        public string Status { get; set; }
    }
}