using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Redmine.ManagerWPF.Data.Models;
using Redmine.ManagerWPF.Desktop.Messages;
using Redmine.ManagerWPF.Desktop.Models.TimeIntervals;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using Redmine.ManagerWPF.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class TimeIntervalsViewModel : ObservableRecipient
    {
        private TreeModel _node;

        public TreeModel Node
        {
            get { return _node; }
            set { SetProperty(ref _node, value); }
        }

        public ObservableCollection<ListItemModel> TimeIntervalsForNode = new ObservableCollection<ListItemModel>();

        private readonly IMapper _mapper;
        private readonly TimeIntervalsService _timeIntervalsService;

        public TimeIntervalsViewModel()
        {
            _timeIntervalsService = Ioc.Default.GetRequiredService<TimeIntervalsService>();
            _mapper = Ioc.Default.GetRequiredService<IMapper>();

            WeakReferenceMessenger.Default.Register<NodeChangeMessage>(this, (r, m) =>
            {
                ReceiveNode(m.Value);
            });
        }

        public void ReceiveNode(TreeModel message)
        {

            Task.Run(async () =>
            {
                Node = message;
                TimeIntervalsForNode.Clear();

                if (Node.Type == nameof(Issue))
                {
                    var timeIntervalsForIssue = await _timeIntervalsService.GetTimeIntervalsForIssue(Node.Id);
                    foreach (var timeInterval in _mapper.Map<List<ListItemModel>>(timeIntervalsForIssue))
                    {
                        TimeIntervalsForNode.Add(timeInterval);
                    }
                }

                if (Node.Type == nameof(Comment))
                {
                    var timeIntervalsForComment = await _timeIntervalsService.GetTimeIntervalsForComment(Node.Id);
                    foreach (var timeInterval in _mapper.Map<List<ListItemModel>>(timeIntervalsForComment))
                    {
                        TimeIntervalsForNode.Add(timeInterval);
                    }
                }

            });
        }
    }
}
