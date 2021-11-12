using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Redmine.ManagerWPF.Desktop.Messages;
using Redmine.ManagerWPF.Desktop.Models.Issues;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using Redmine.ManagerWPF.Desktop.Services;
using System.Diagnostics;
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

        public IRelayCommand OpenBrowserCommand { get; }

        public IssueFormViewModel()
        {
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _issueService = Ioc.Default.GetRequiredService<IssueService>();

            WeakReferenceMessenger.Default.Register<NodeChangeMessage>(this, (r, m) =>
            {
                ReceiveNode(m.Value);
            });

            OpenBrowserCommand = new RelayCommand(OpenBrowser);
        }

        public void ReceiveNode(TreeModel message)
        {
            if (message.Type == nameof(Data.Models.Issue))
            {
                Node = message;
                var issue = _issueService.GetIssue(Node.Id);
                if (issue != null)
                {
                    IssueFormModel = _mapper.Map<FormModel>(issue);
                }
            }
        }

        private void OpenBrowser()
        {
            var psi = new ProcessStartInfo
            {
                FileName = IssueFormModel.Link,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}