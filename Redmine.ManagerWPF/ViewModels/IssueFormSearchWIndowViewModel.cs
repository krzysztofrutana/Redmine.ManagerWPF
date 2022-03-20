using System.Diagnostics;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Redmine.ManagerWPF.Desktop.Extensions;
using Redmine.ManagerWPF.Desktop.Messages.SearchWindow;
using Redmine.ManagerWPF.Desktop.Models.Issues;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class IssueFormSearchWindowViewModel : ObservableRecipient
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
            set => SetProperty(ref _issueFormModel, value);
        }
        #endregion

        #region Injections
        private readonly IssueService _issueService;
        private readonly IMapper _mapper;
        private readonly IMessageBoxHelper _messageBoxHelper;
        private readonly ILogger<IssueFormSearchWindowViewModel> _logger;
        #endregion

        #region Commands
        public IRelayCommand OpenBrowserCommand { get; }
        #endregion

        public IssueFormSearchWindowViewModel()
        {
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _issueService = Ioc.Default.GetRequiredService<IssueService>();
            _messageBoxHelper = Ioc.Default.GetRequiredService<IMessageBoxHelper>();
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
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy pobieraniu zadania");
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