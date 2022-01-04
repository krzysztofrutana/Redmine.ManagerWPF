using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Redmine.ManagerWPF.Desktop.Messages;
using Redmine.ManagerWPF.Desktop.Models.Issues;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class IssueFormSearchWIndowViewModel : ObservableRecipient
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
        private readonly IMessageBoxService _messageBoxService;

        public IRelayCommand OpenBrowserCommand { get; }
        public IAsyncRelayCommand SetAsDoneCommand { get; }
        public IAsyncRelayCommand SetAsUndoneCommand { get; }

        public IssueFormSearchWIndowViewModel()
        {
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _issueService = Ioc.Default.GetRequiredService<IssueService>();
            _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();

            WeakReferenceMessenger.Default.Register<SearchNodeChangeMessage>(this, (r, m) =>
            {
                ReceiveNode(m.Value);
            });

            OpenBrowserCommand = new RelayCommand(OpenBrowser);
        }

        public async void ReceiveNode(TreeModel message)
        {
            try
            {
                if (message.Type == nameof(Data.Models.Issue))
                {
                    Node = message;
                    var issue = await _issueService.GetIssueAsync(Node.Id);
                    if (issue != null)
                    {
                        IssueFormModel = _mapper.Map<FormModel>(issue);
                    }
                }
            }
            catch (System.Exception ex)
            {
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy pobieraniu zadania");
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