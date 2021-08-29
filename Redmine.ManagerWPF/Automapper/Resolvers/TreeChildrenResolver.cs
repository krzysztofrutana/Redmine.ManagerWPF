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
            if (source.Issues.Count == 0)
            {
                var list = _mapper.Map<ObservableCollection<TreeModel>>(source.Comments);
                return list;
            }
            else
            {
                var list = _mapper.Map<ObservableCollection<TreeModel>>(source.Issues);
                return list;
            }
        }
    }
}