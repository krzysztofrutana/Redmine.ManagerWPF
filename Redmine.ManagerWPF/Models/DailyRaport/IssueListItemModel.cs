using Redmine.ManagerWPF.Data.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redmine.ManagerWPF.Desktop.Models.DailyRaport
{
    public class IssueListItemModel : ListItemModel
    {
        public long IssueId { get; set; }
        public string Type { get; set; } = nameof(ObjectType.Issue);
        public List<TimeIntervalListItemModel> Childrens { get; set; }

    }
}
