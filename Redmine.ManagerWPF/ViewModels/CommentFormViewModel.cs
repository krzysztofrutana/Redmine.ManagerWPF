using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Redmine.ManagerWPF.Desktop.Messages;
using Redmine.ManagerWPF.Desktop.Models.Comments;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using Redmine.ManagerWPF.Desktop.Services;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class CommentFormViewModel : ObservableRecipient
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

        public IRelayCommand OpenBrowserCommand { get; }

        public CommentFormViewModel()
        {
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _commentService = Ioc.Default.GetRequiredService<CommentService>();

            WeakReferenceMessenger.Default.Register<NodeChangeMessage>(this, (r, m) =>
            {
                ReceiveNode(m.Value);
            });

            OpenBrowserCommand = new RelayCommand(OpenBrowser);
        }

        public void ReceiveNode(TreeModel message)
        {
            if(message.Type == nameof(Data.Models.Comment))
            {
                Task.Run(async () =>
                {
                    Node = message;
                    var comment = await _commentService.GetCommentByIdAsync(Node.Id);
                    if (comment != null)
                    {
                        CommentFormModel = _mapper.Map<FormModel>(comment);
                    }
                });
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