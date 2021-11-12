using AutoMapper;
using Redmine.ManagerWPF.Data.Models;
using Redmine.ManagerWPF.Desktop.Models.TimeIntervals;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redmine.ManagerWPF.Desktop.Automapper.Resolvers
{
    public class CountetTimeResolver : IValueResolver<TimeInterval, ListItemModel, string>
    {
        public string Resolve(TimeInterval source, ListItemModel destination, string destMember, ResolutionContext context)
        {
            if(source.IsEnd)
            {
                var totalTime = (source.TimeIntervalEnd.Value - source.TimeIntervalStart.Value);
                return $"{totalTime.Hours}:{totalTime.Minutes}:{totalTime.Seconds}";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
