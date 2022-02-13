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
using Redmine.ManagerWPF.Desktop.Models.Comments;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class CommentFormViewModel : ObservableRecipient
    {
        private TreeModel _node;

        private TreeModel Node
        {
            get => _node;
            set => SetProperty(ref _node, value);
        }

        private FormModel _commentFormModel;

        public FormModel CommentFormModel
        {
            get => _commentFormModel;
            set => SetProperty(ref _commentFormModel, value);
        }

        private readonly CommentService _commentService;
        private readonly IMapper _mapper;
        private readonly IMessageBoxService _messageBoxService;
        private readonly ILogger<CommentFormViewModel> _logger;

        public IRelayCommand OpenBrowserCommand { get; }

        public IAsyncRelayCommand SetAsDoneCommand { get; }
        public IAsyncRelayCommand SetAsUndoneCommand { get; }

        public CommentFormViewModel()
        {
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _commentService = Ioc.Default.GetRequiredService<CommentService>();
            _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();
            _logger = Ioc.Default.GetLoggerForType<CommentFormViewModel>();

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
                if (message.Type != nameof(Data.Models.Comment)) return;
                Node = message;
                var comment = await _commentService.GetCommentAsync(Node.Id).ConfigureAwait(false);
                if (comment == null) return;
                CommentFormModel = _mapper.Map<FormModel>(comment);
                CommentFormModel.Status = comment.Done ? "Wykonane" : "Niewykonane";
                WeakReferenceMessenger.Default.Send(new InformationLoadedMessage(Node));
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(ReceiveNode), ex.Message);
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy pobieraniu komentarza");
            }

        }

        private async Task SetAsDone()
        {
            try
            {
                Node.Done = true;
                CommentFormModel.Done = true;
                CommentFormModel.Status = "Wykonane";
                var issue = await _commentService.GetCommentAsync(Node.Id);
                if (issue != null)
                {
                    issue.Done = true;
                    await _commentService.Update(issue);

                    WeakReferenceMessenger.Default.Send(new ChangeSelectedCommentDoneStatus(Node));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(SetAsDone), ex.Message);
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy oznaczaniu jako zakończone");
            }
        }

        private async Task SetAsUndone()
        {
            try
            {
                Node.Done = false;
                CommentFormModel.Done = false;
                CommentFormModel.Status = "Niewykonane";
                var issue = await _commentService.GetCommentAsync(Node.Id);
                if (issue != null)
                {
                    issue.Done = false;
                    await _commentService.Update(issue);

                    WeakReferenceMessenger.Default.Send(new ChangeSelectedCommentDoneStatus(Node));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(SetAsUndone), ex.Message);
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy oznaczaniu jako niezakończone");
            }
        }

        private void OpenBrowser()
        {
            var psi = new ProcessStartInfo
            {
                FileName = CommentFormModel.Link,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}