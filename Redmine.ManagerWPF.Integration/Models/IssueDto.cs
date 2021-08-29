using System;
using System.Collections.Generic;

namespace Redmine.ManagerWPF.Integration.Models
{
    public class IssueDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int? ParentIssueId { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public IEnumerable<JournalDto> Comments { get; set; }
        public string Status { get; set; }
    }
}