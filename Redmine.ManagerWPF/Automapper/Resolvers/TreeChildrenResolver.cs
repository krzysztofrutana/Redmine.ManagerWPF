using AutoMapper;
using Redmine.ManagerWPF.Data.Models;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using System.Collections.ObjectModel;

namespace Redmine.ManagerWPF.Desktop.Automapper.Resolvers
{
    public class TreeChildrenResolver : IValueResolver<Issue, TreeModel, ObservableCollection<TreeModel>>
    {
        private readonly IMapper _mapper;

        public TreeChildrenResolver(IMapper mapper)
        {
            _mapper = mapper;
        }

        public ObservableCollection<TreeModel> Resolve(Issue source, TreeModel destination, ObservableCollection<TreeModel> destMember, ResolutionContext context)
        {
            var list = new ObservableCollection<TreeModel>();

            if (source.Issues != null)
            {
                foreach (var issue in source.Issues)
                {
                    list.Add(_mapper.Map<TreeModel>(issue));
                }
            }

            if (source.Comments != null)
            {
                foreach (var comment in source.Comments)
                {
                    list.Add(_mapper.Map<TreeModel>(comment));
                }
            }

            return list;
        }
    }
}