using System;
using System.Diagnostics;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Redmine.ManagerWPF.Desktop.Extensions;
using Redmine.ManagerWPF.Desktop.Messages.SearchWindow;
using Redmine.ManagerWPF.Desktop.Models.Comments;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class CommentSearchWindowFormViewModel : ObservableRecipient
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
            private set => SetProperty(ref _commentFormModel, value);
        }
        #endregion

        #region Injections
        private readonly CommentService _commentService;
        private readonly IMapper _mapper;
        private readonly IMessageBoxHelper _messageBoxHelper;
        private readonly ILogger<CommentSearchWindowFormViewModel> _logger;
        #endregion

        #region Commands
        public IRelayCommand OpenBrowserCommand { get; }
        #endregion

        public CommentSearchWindowFormViewModel()
        {
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _commentService = Ioc.Default.GetRequiredService<CommentService>();
            _messageBoxHelper = Ioc.Default.GetRequiredService<IMessageBoxHelper>();
            _logger = Ioc.Default.GetLoggerForType<CommentSearchWindowFormViewModel>();

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
                if (message.Type != nameof(Data.Models.Comment)) return;
                Node = message;
                var comment = await _commentService.GetCommentAsync(Node.Id).ConfigureAwait(false);
                if (comment == null) return;
                CommentFormModel = _mapper.Map<FormModel>(comment);
                CommentFormModel.Status = comment.Done ? "Wykonane" : "Niewykonane";
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(ReceiveNode), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy pobieraniu komentarza");
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