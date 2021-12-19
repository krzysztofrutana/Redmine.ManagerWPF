using Redmine.ManagerWPF.Data.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redmine.ManagerWPF.Desktop.Models.DailyRaport
{
    public class ProjectListItemModel : ListItemModel
    {
        public long ProjectId { get; set; }
        public string Type { get; set; } = nameof(ObjectType.Project);
        public List<IssueListItemModel> Childrens { get; set; }

    }
}
