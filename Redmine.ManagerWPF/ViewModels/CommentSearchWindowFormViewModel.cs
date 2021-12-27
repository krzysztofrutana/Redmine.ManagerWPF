using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Redmine.ManagerWPF.Desktop.Messages;
using Redmine.ManagerWPF.Desktop.Models.Comments;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class CommentSearchWindowFormViewModel : ObservableRecipient
    {
        private TreeModel _node;

        public TreeModel Node
        {
            get { return _node; }
            set { SetProperty(ref _node, value); }
        }

        private FormModel _commentFormModel;

        public FormModel CommentFormModel
        {
            get { return _commentFormModel; }
            set { SetProperty(ref _commentFormModel, value); }
        }

        private readonly CommentService _commentService;
        private readonly IMapper _mapper;
        private readonly IMessageBoxService _messageBoxService;

        public IRelayCommand OpenBrowserCommand { get; }

        public IAsyncRelayCommand SetAsDoneCommand { get; }
        public IAsyncRelayCommand SetAsUndoneCommand { get; }

        public CommentSearchWindowFormViewModel()
        {
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _commentService = Ioc.Default.GetRequiredService<CommentService>();
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
                if (message.Type == nameof(Data.Models.Comment))
                {
                    Node = message;
                    var comment = await _commentService.GetCommentAsync(Node.Id).ConfigureAwait(false);
                    if (comment != null)
                    {
                        CommentFormModel = _mapper.Map<FormModel>(comment);
                        if (comment.Done)
                        {
                            CommentFormModel.Status = "Wykonane";
                        }
                        else
                        {
                            CommentFormModel.Status = "Niewykonane";
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy pobieraniu komentarza");
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