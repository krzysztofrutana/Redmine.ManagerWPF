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
using Redmine.ManagerWPF.Desktop.Messages.Forms;
using Redmine.ManagerWPF.Desktop.Messages.MainWindowTreeView;
using Redmine.ManagerWPF.Desktop.Models.Comments;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class CommentFormViewModel : ObservableRecipient
    {
        #region Properties
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
        #endregion

        #region Injections
        private readonly CommentService _commentService;
        private readonly IMapper _mapper;
        private readonly IMessageBoxHelper _messageBoxHelper;
        private readonly ILogger<CommentFormViewModel> _logger;
        #endregion

        #region Commands
        public IRelayCommand OpenBrowserCommand { get; }
        public IAsyncRelayCommand SetAsDoneCommand { get; }
        public IAsyncRelayCommand SetAsUndoneCommand { get; }
        #endregion

        public CommentFormViewModel()
        {
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _commentService = Ioc.Default.GetRequiredService<CommentService>();
            _messageBoxHelper = Ioc.Default.GetRequiredService<IMessageBoxHelper>();
            _logger = Ioc.Default.GetLoggerForType<CommentFormViewModel>();

            WeakReferenceMessenger.Default.Register<NodeChangeMessage>(this, (r, m) =>
            {
                ReceiveNode(m.Value);
            });

            OpenBrowserCommand = new RelayCommand(OpenBrowser);

            SetAsDoneCommand = new AsyncRelayCommand(SetAsDoneAsync);
            SetAsUndoneCommand = new AsyncRelayCommand(SetAsUndoneAsync);
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
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy pobieraniu komentarza");
            }

        }

        private async Task SetAsDoneAsync()
        {
            try
            {
                Node.Done = true;
                CommentFormModel.Done = true;
                CommentFormModel.Status = "Wykonane";
                var comment = await _commentService.GetCommentAsync(Node.Id);
                if (comment != null)
                {
                    comment.Done = true;
                    await _commentService.Update(comment);

                    WeakReferenceMessenger.Default.Send(new ChangeSelectedCommentDoneStatus(Node));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(SetAsDoneAsync), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy oznaczaniu jako zakończone");
            }
        }

        private async Task SetAsUndoneAsync()
        {
            try
            {
                Node.Done = false;
                CommentFormModel.Done = false;
                CommentFormModel.Status = "Niewykonane";
                var comment = await _commentService.GetCommentAsync(Node.Id);
                if (comment != null)
                {
                    comment.Done = false;
                    await _commentService.Update(comment);

                    WeakReferenceMessenger.Default.Send(new ChangeSelectedCommentDoneStatus(Node));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(SetAsUndoneAsync), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy oznaczaniu jako niezakończone");
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