using System;

namespace Redmine.ManagerWPF.Integration.Models
{
    public class JournalDto
    {
        public int Id { get; set; }
        public string CreatedBy { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
    }
}