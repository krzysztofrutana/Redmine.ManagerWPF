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
        private readonly IMessageBoxService _messageBoxService;

        public IRelayCommand OpenBrowserCommand { get; }
        public IAsyncRelayCommand SetAsDoneCommand { get; }
        public IAsyncRelayCommand SetAsUndoneCommand { get; }

        public IssueFormViewModel()
        {
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _issueService = Ioc.Default.GetRequiredService<IssueService>();
            _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();

            WeakReferenceMessenger.Default.Register<NodeChangeMessage>(this, (r, m) =>
            {
                ReceiveNode(m.Value);
            });

            OpenBrowserCommand = new RelayCommand(OpenBrowser);
            SetAsDoneCommand = new AsyncRelayCommand(SetAsDone);
            SetAsUndoneCommand = new AsyncRelayCommand(SetAsUndone);
        }

        public async void ReceiveNode(TreeModel message)
        {
            try
            {
                if (message.Type == nameof(Data.Models.Issue))
                {
                    Node = message;
                    var issue = await _issueService.GetIssueAsync(Node.Id).ConfigureAwait(false);
                    if (issue != null)
                    {
                        IssueFormModel = _mapper.Map<FormModel>(issue);
                        WeakReferenceMessenger.Default.Send(new InformationLoadedMessage(Node));
                    }

                }
            }
            catch (System.Exception ex)
            {
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy pobieraniu zadania");
            }
        }

        private async Task SetAsDone()
        {
            try
            {
                Node.Done = true;
                IssueFormModel.Done = true;
                var issue = await _issueService.GetIssueAsync(Node.Id);
                if (issue != null)
                {
                    issue.Done = true;
                    await _issueService.Update(issue);

                    WeakReferenceMessenger.Default.Send(new ChangeSelectedIssueDoneStatus(Node));
                }
            }
            catch (System.Exception ex)
            {
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy oznaczaniu jako zakończone");
            }
        }

        private async Task SetAsUndone()
        {
            try
            {
                Node.Done = false;
                IssueFormModel.Done = false;
                var issue = await _issueService.GetIssueAsync(Node.Id);
                if (issue != null)
                {
                    issue.Done = false;
                    await _issueService.Update(issue);

                    WeakReferenceMessenger.Default.Send(new ChangeSelectedIssueDoneStatus(Node));
                }
            }
            catch (System.Exception ex)
            {
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy oznaczaniu jako niezakończone");
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