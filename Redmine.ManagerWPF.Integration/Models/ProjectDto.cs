using System;

namespace Redmine.ManagerWPF.Integration.Models
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Identifier { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}