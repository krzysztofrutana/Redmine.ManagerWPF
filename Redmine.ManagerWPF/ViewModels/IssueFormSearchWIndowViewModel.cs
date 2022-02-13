using System.Diagnostics;
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
    public class IssueFormSearchWindowViewModel : ObservableRecipient
    {
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
            set => SetProperty(ref _issueFormModel, value);
        }

        private readonly IssueService _issueService;
        private readonly IMapper _mapper;
        private readonly IMessageBoxService _messageBoxService;
        private readonly ILogger<IssueFormSearchWindowViewModel> _logger;

        public IRelayCommand OpenBrowserCommand { get; }

        public IssueFormSearchWindowViewModel()
        {
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _issueService = Ioc.Default.GetRequiredService<IssueService>();
            _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();
            _logger = Ioc.Default.GetLoggerForType<IssueFormSearchWindowViewModel>();

            WeakReferenceMessenger.Default.Register<SearchNodeChangeMessage>(this, (r, m) =>
            {
                ReceiveNode(m.Value);
            });

            OpenBrowserCommand = new RelayCommand(OpenBrowser);
        }

        private async void ReceiveNode(TreeModel message)
        {
            try
            {
                if (message.Type != nameof(Data.Models.Issue)) return;
                Node = message;
                var issue = await _issueService.GetIssueAsync(Node.Id);
                if (issue != null)
                {
                    IssueFormModel = _mapper.Map<FormModel>(issue);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(ReceiveNode), ex.Message);
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