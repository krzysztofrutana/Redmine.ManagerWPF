using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Redmine.ManagerWPF.Data.Enums;
using Redmine.ManagerWPF.Data.Models;
using Redmine.ManagerWPF.Desktop.Helpers;
using Redmine.ManagerWPF.Desktop.Messages;
using Redmine.ManagerWPF.Desktop.Models.TimeIntervals;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;
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

        private TreeModel _node;


        public TreeModel Node
        {
            get { return _node; }
            set { SetProperty(ref _node, value); }
        }

        public ObservableCollection<ListItemModel> TimeIntervalsForNode { get; set; } = new AsyncObservableCollection<ListItemModel>();

        private readonly IMapper _mapper;
        private readonly TimeIntervalsService _timeIntervalsService;
        private readonly IMessageBoxService _messageBoxService;

        public IAsyncRelayCommand AddTimeIntervalAsyncCommand { get; }
        public IAsyncRelayCommand RemoveTimeIntervalAsyncCommand { get; }
        public IAsyncRelayCommand StartTimeIntervalAsyncCommand { get; }
        public IAsyncRelayCommand EndTimeIntervalAsyncCommand { get; }


        public TimeIntervalsViewModel()
        {
            _timeIntervalsService = Ioc.Default.GetRequiredService<TimeIntervalsService>();
            _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();
            _mapper = Ioc.Default.GetRequiredService<IMapper>();

            AddTimeIntervalAsyncCommand = new AsyncRelayCommand(AddTimeInterval);
            RemoveTimeIntervalAsyncCommand = new AsyncRelayCommand<ListItemModel>(RemoveTimeInterval);
            StartTimeIntervalAsyncCommand = new AsyncRelayCommand<ListItemModel>(StartTimeInterval);
            EndTimeIntervalAsyncCommand = new AsyncRelayCommand<ListItemModel>(EndTimeInterval);

            TimeIntervalsForNode.Clear();

            WeakReferenceMessenger.Default.Register<InformationLoadedMessage>(this, (recipient, mmessage) =>
            {
                ReceiveNode(mmessage.Value);
            });
        }

        public async void ReceiveNode(TreeModel message)
        {
            try
            {
                Node = message;
                TimeIntervalsForNode.Clear();

                List<ListItemModel> times = new List<ListItemModel>();
                if (Node.Type == nameof(Issue))
                {
                    var timeIntervalsForIssue = await _timeIntervalsService.GetTimeIntervalsForIssueAsync(Node.Id);
                    times = _mapper.Map<List<ListItemModel>>(timeIntervalsForIssue);
                }

                if (Node.Type == nameof(Comment))
                {
                    var timeIntervalsForComment = await _timeIntervalsService.GetTimeIntervalsForCommentAsync(Node.Id);
                    times = _mapper.Map<List<ListItemModel>>(timeIntervalsForComment);
                }

                if (times.Any())
                {

                    foreach (var item in times)
                    {
                        TimeIntervalsForNode.Add(item);
                    }

                    var startedTimeInterval = TimeIntervalsForNode.FirstOrDefault(x => x.IsStarted);
                    if (startedTimeInterval != null)
                    {
                        UpdateCountedTime(startedTimeInterval);
                    }
                }
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy pobieraniu listy czasów");
            }
        }

        public async Task AddTimeInterval()
        {
            try
            {
                TimeInterval timeInterval = null;
                if (Node.Type == nameof(ObjectType.Issue))
                {
                    timeInterval = await _timeIntervalsService.CreateEmptyAsync(Node.Id, ObjectType.Issue);

                }

                if (Node.Type == nameof(ObjectType.Comment))
                {
                    timeInterval = await _timeIntervalsService.CreateEmptyAsync(Node.Id, ObjectType.Comment);

                }

                if (timeInterval != null)
                {
                    var timeIntervalModel = _mapper.Map<ListItemModel>(timeInterval);
                    TimeIntervalsForNode.Add(timeIntervalModel);
                }
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy dodawaniu czasu");
            }
        }

        public async Task RemoveTimeInterval(ListItemModel item)
        {
            try
            {
                if (CurrentNodeTimer != null)
                {
                    CurrentNodeTimer.Dispose();
                }

                TimeInterval timeInterval = await _timeIntervalsService.GetTimeIntervalAsync(item.Id);
                if (timeInterval != null)
                {
                    await _timeIntervalsService.DeleteAsync(timeInterval);
                }

                var deletedTimeInterval = TimeIntervalsForNode.SingleOrDefault(x => x.Id == item.Id);
                TimeIntervalsForNode.Remove(deletedTimeInterval);
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy usuwaniu czasu");
            }
        }

        public async Task StartTimeInterval(ListItemModel item)
        {
            try
            {
                if (await _timeIntervalsService.CheckIfAnyStartedTimeIntervalExistAsync())
                {
                    _messageBoxService.ShowWarningInfoBox("Istnieje inne niezakończone zadanie!", "Uwaga");
                    return;
                }

                var listItem = TimeIntervalsForNode.FirstOrDefault(x => x.Id == item.Id);
                TimeInterval timeInterval = await _timeIntervalsService.GetTimeIntervalAsync(item.Id);
                if (timeInterval != null)
                {
                    listItem.StartDate = DateTime.Now;
                    listItem.IsStarted = true;

                    timeInterval.TimeIntervalStart = item.StartDate;
                    await _timeIntervalsService.UpdateAsync(timeInterval);
                }

                UpdateCountedTime(listItem);
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy starcie czasu");
            }
        }

        public async Task EndTimeInterval(ListItemModel item)
        {
            try
            {
                var listItem = TimeIntervalsForNode.FirstOrDefault(x => x.Id == item.Id);
                TimeInterval timeInterval = await _timeIntervalsService.GetTimeIntervalAsync(item.Id);
                if (timeInterval != null)
                {
                    listItem.EndDate = DateTime.Now;
                    listItem.IsStarted = false;

                    timeInterval.TimeIntervalEnd = item.EndDate;
                    timeInterval.IsStarted = false;
                    await _timeIntervalsService.UpdateAsync(timeInterval);
                }

                CurrentNodeTimer.Dispose();
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy stopie czasu");
            }
        }

        private void UpdateCountedTime(ListItemModel item)
        {

            CurrentNodeTimer = new Timer(SetCountedTime, item, 0, 1000);
        }

        private void SetCountedTime(object? state)
        {
            if (state is ListItemModel timeInterval)
            {
                if (timeInterval.IsStarted)
                {
                    if (timeInterval.StartDate.HasValue)
                    {
                        var totalTime = (DateTime.Now - timeInterval.StartDate.Value);
                        if (totalTime.Days > 0)
                        {
                            timeInterval.CountedTime = $"{totalTime.Days} d, {totalTime.Hours}:{totalTime.Minutes}:{totalTime.Seconds}";
                        }
                        else
                        {
                            timeInterval.CountedTime = $"{totalTime.Hours}:{totalTime.Minutes}:{totalTime.Seconds}";
                        }
                    }
                }
            }
        }
    }
}
