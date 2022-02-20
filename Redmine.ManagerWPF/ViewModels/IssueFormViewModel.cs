using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Redmine.ManagerWPF.Desktop.Extensions;
using Redmine.ManagerWPF.Desktop.Messages;
using Redmine.ManagerWPF.Desktop.Models.Issues;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class IssueFormViewModel : ObservableRecipient
    {
        #region Properties
        private TreeModel _node;

        private TreeModel Node
        {
            get => _node;
            set => SetProperty(ref _node, value);
        }

        private FormModel _issueFormModel;

        public FormModel IssueFormModel
        {
            get => _issueFormModel;
            private set => SetProperty(ref _issueFormModel, value);
        }
        #endregion

        #region Injections
        private readonly IssueService _issueService;
        private readonly IMapper _mapper;
        private readonly IMessageBoxHelper _messageBoxHelper;
        private readonly ILogger<IssueFormViewModel> _logger;
        #endregion

        #region Commands
        public IRelayCommand OpenBrowserCommand { get; }
        public IAsyncRelayCommand SetAsDoneCommand { get; }
        public IAsyncRelayCommand SetAsUndoneCommand { get; }
        #endregion

        public IssueFormViewModel()
        {
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _issueService = Ioc.Default.GetRequiredService<IssueService>();
            _messageBoxHelper = Ioc.Default.GetRequiredService<IMessageBoxHelper>();
            _logger = Ioc.Default.GetLoggerForType<IssueFormViewModel>();

            WeakReferenceMessenger.Default.Register<NodeChangeMessage>(this, (r, m) =>
            {
                ReceiveNode(m.Value);
            });

            OpenBrowserCommand = new RelayCommand(OpenBrowser);
            SetAsDoneCommand = new AsyncRelayCommand(SetAsDone);
            SetAsUndoneCommand = new AsyncRelayCommand(SetAsUndone);
        }

        private async void ReceiveNode(TreeModel message)
        {
            try
            {
                if (message.Type != nameof(Data.Models.Issue)) return;
                Node = message;
                var issue = await _issueService.GetIssueAsync(Node.Id).ConfigureAwait(false);
                if (issue == null) return;
                IssueFormModel = _mapper.Map<FormModel>(issue);
                WeakReferenceMessenger.Default.Send(new InformationLoadedMessage(Node));
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(ReceiveNode), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy pobieraniu zadania");
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
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(SetAsDone), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy oznaczaniu jako zakończone");
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
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(SetAsUndone), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy oznaczaniu jako niezakończone");
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