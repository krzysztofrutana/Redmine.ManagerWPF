using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Redmine.ManagerWPF.Data.Enums;
using Redmine.ManagerWPF.Data.Models;
using Redmine.ManagerWPF.Desktop.Messages;
using Redmine.ManagerWPF.Desktop.Models.TimeIntervals;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using Redmine.ManagerWPF.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class TimeIntervalsViewModel : ObservableRecipient
    {

        private Timer CurrentNodeTimer { get; set; }

        private readonly object _timeIntervalsForNodeLock;

        private TreeModel _node;


        public TreeModel Node
        {
            get { return _node; }
            set { SetProperty(ref _node, value); }
        }

        private ObservableCollection<ListItemModel> _timeIntervalsForNode = new ObservableCollection<ListItemModel>();
        public ObservableCollection<ListItemModel> TimeIntervalsForNode
        {
            get => _timeIntervalsForNode;
            private set
            {
                _timeIntervalsForNode = value;
                BindingOperations.EnableCollectionSynchronization(_timeIntervalsForNode, _timeIntervalsForNodeLock);
            }
        }
        private readonly IMapper _mapper;
        private readonly TimeIntervalsService _timeIntervalsService;

        public IAsyncRelayCommand AddTimeIntervalAsyncCommand { get; }
        public IAsyncRelayCommand RemoveTimeIntervalAsyncCommand { get; }
        public IAsyncRelayCommand StartTimeIntervalAsyncCommand { get; }
        public IAsyncRelayCommand EndTimeIntervalAsyncCommand { get; }


        public TimeIntervalsViewModel()
        {
            _timeIntervalsService = Ioc.Default.GetRequiredService<TimeIntervalsService>();
            _mapper = Ioc.Default.GetRequiredService<IMapper>();

            AddTimeIntervalAsyncCommand = new AsyncRelayCommand(AddTimeInterval);
            RemoveTimeIntervalAsyncCommand = new AsyncRelayCommand<ListItemModel>(RemoveTimeInterval);
            StartTimeIntervalAsyncCommand = new AsyncRelayCommand<ListItemModel>(StartTimeInterval);
            EndTimeIntervalAsyncCommand = new AsyncRelayCommand<ListItemModel>(EndTimeInterval);


            WeakReferenceMessenger.Default.Register<NodeChangeMessage>(this, (r, m) =>
            {
                ReceiveNode(m.Value);
            });
        }

        public void ReceiveNode(TreeModel message)
        {

            Task.Run(async () =>
            {
                try
                {
                    Node = message;

                    List<ListItemModel> times = new List<ListItemModel>();
                    if (Node.Type == nameof(Issue))
                    {
                        var timeIntervalsForIssue = await _timeIntervalsService.GetTimeIntervalsForIssue(Node.Id);
                        times = _mapper.Map<List<ListItemModel>>(timeIntervalsForIssue);
                    }

                    if (Node.Type == nameof(Comment))
                    {
                        var timeIntervalsForComment = await _timeIntervalsService.GetTimeIntervalsForComment(Node.Id);
                        times = _mapper.Map<List<ListItemModel>>(timeIntervalsForComment);
                    }

                    if (times.Any())
                    {
                        TimeIntervalsForNode.Clear();
                        foreach (var item in times)
                        {
                            TimeIntervalsForNode.Add(item);
                        }
                    }

                    return new List<ListItemModel>();
                }
                catch (Exception ex)
                {

                    throw;
                }
            });


        }

        public async Task AddTimeInterval()
        {
            try
            {
                TimeInterval timeInterval = null;
                if (Node.Type == nameof(ObjectType.Issue))
                {
                    timeInterval = await _timeIntervalsService.CreateEmpty(Node.Id, ObjectType.Issue);

                }

                if (Node.Type == nameof(ObjectType.Comment))
                {
                    timeInterval = await _timeIntervalsService.CreateEmpty(Node.Id, ObjectType.Comment);

                }

                if (timeInterval != null)
                {
                    TimeIntervalsForNode.Add(_mapper.Map<ListItemModel>(timeInterval));
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task RemoveTimeInterval(ListItemModel item)
        {
            try
            {
                TimeInterval timeInterval = await _timeIntervalsService.GetTimeInterval(item.Id);
                if (timeInterval != null)
                {
                    await _timeIntervalsService.Delete(timeInterval);
                }

                var deletedTimeInterval = TimeIntervalsForNode.SingleOrDefault(x => x.Id == item.Id);
                TimeIntervalsForNode.Remove(deletedTimeInterval);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task StartTimeInterval(ListItemModel item)
        {
            try
            {
                item.StartDate = DateTime.Now;
                TimeInterval timeInterval = await _timeIntervalsService.GetTimeInterval(item.Id);
                if (timeInterval != null)
                {
                    timeInterval.TimeIntervalStart = item.StartDate;
                    await _timeIntervalsService.Update(timeInterval);
                }

                UpdateCountedTime(item);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task EndTimeInterval(ListItemModel item)
        {
            try
            {
                item.EndDate = DateTime.Now;
                TimeInterval timeInterval = await _timeIntervalsService.GetTimeInterval(item.Id);
                if (timeInterval != null)
                {
                    timeInterval.TimeIntervalEnd = item.EndDate;
                    await _timeIntervalsService.Update(timeInterval);
                }

                CurrentNodeTimer.Dispose();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void UpdateCountedTime(ListItemModel item)
        {
            CurrentNodeTimer = new Timer(SetCountedTime, item, 0, 1000);
        }

        private void SetCountedTime(object? state)
        {
            if (TimeIntervalsForNode.Any(x => x.IsStarted))
            {
                var startedTimeInterval = TimeIntervalsForNode.FirstOrDefault(x => x.IsStarted);
                startedTimeInterval.CountedTime = (DateTime.Now - startedTimeInterval.StartDate).ToString();
            }
        }
    }
}
