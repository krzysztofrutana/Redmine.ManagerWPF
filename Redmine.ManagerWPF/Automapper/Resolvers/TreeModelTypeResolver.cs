using AutoMapper;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using System;

namespace Redmine.ManagerWPF.Desktop.Automapper.Resolvers
{
    public class TreeModelTypeResolver : IValueResolver<object, TreeModel, string>
    {
        public string Resolve(object source, TreeModel destination, string destMember, ResolutionContext context)
        {
            if (source.GetType() == typeof(Data.Models.Issue))
            {
                return nameof(Data.Models.Issue);
            }
            if (source.GetType() == typeof(Data.Models.Comment))
            {
                return nameof(Data.Models.Comment);
            }

            return String.Empty;
        }
    }
}