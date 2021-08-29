using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Redmine.ManagerWPF.Desktop.Messages;
using Redmine.ManagerWPF.Desktop.Models.Issues;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using Redmine.ManagerWPF.Desktop.Services;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class IssueFormViewModel : ObservableRecipient
    {
        private TreeModel _node;

        public TreeModel Node
        {
            get { return _node; }
            set { SetProperty(ref _node, value); }
        }

        private FormModel _issueFormModel;

        public FormModel IssueFormModel
        {
            get { return _issueFormModel; }
            set { SetProperty(ref _issueFormModel, value); }
        }

        private readonly IssueService _issueService;
        private readonly IMapper _mapper;

        public IssueFormViewModel()
        {
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _issueService = Ioc.Default.GetRequiredService<IssueService>();

            WeakReferenceMessenger.Default.Register<NodeChangeMessage>(this, async (r, m) =>
            {
                await ReceiveNode(m.Value);
            });
        }

        public async Task ReceiveNode(TreeModel message)
        {
            Node = message;
            var issue = await _issueService.GetIssueAsync(Node.Id);
            if (issue != null)
            {
                IssueFormModel = _mapper.Map<FormModel>(issue);
            }
        }
    }
}