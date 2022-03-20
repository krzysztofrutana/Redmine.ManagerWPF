using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Redmine.ManagerWPF.Data.Enums;
using Redmine.ManagerWPF.Data.Models;
using Redmine.ManagerWPF.Desktop.Extensions;
using Redmine.ManagerWPF.Desktop.Helpers;
using Redmine.ManagerWPF.Desktop.Messages.Forms;
using Redmine.ManagerWPF.Desktop.Messages.TimeIntervalList;
using Redmine.ManagerWPF.Desktop.Models.TimeIntervals;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Desktop.Views.ContentDialogs;
using Redmine.ManagerWPF.Helpers.Interfaces;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class TimeIntervalsViewModel : ObservableRecipient
    {
        #region Properties
        private Timer CurrentNodeTimer { get; set; }

        private TreeModel _node;


        private TreeModel Node
        {
            get => _node;
            set => SetProperty(ref _node, value);
        }

        public ExtendedObservableCollection<ListItemModel> TimeIntervalsForNode { get; set; } = new ExtendedObservableCollection<ListItemModel>();
        #endregion

        #region Injections
        private readonly IMapper _mapper;
        private readonly TimeIntervalsService _timeIntervalsService;
        private readonly IMessageBoxHelper _messageBoxHelper;
        private readonly ILogger<TimeIntervalsViewModel> _logger;
        #endregion

        #region Commands
        public IAsyncRelayCommand AddTimeIntervalAsyncCommand { get; }
        public IAsyncRelayCommand RemoveTimeIntervalAsyncCommand { get; }
        public IAsyncRelayCommand StartTimeIntervalAsyncCommand { get; }
        public IAsyncRelayCommand EndTimeIntervalAsyncCommand { get; }
        public IAsyncRelayCommand<ListItemModel> SaveTimeIntervalNoteCommand { get; }
        public IRelayCommand<ListItemModel> EditStartDateCommand { get; }
        public IRelayCommand<ListItemModel> EditEndDateCommand { get; }
        #endregion

        public TimeIntervalsViewModel()
        {
            _timeIntervalsService = Ioc.Default.GetRequiredService<TimeIntervalsService>();
            _messageBoxHelper = Ioc.Default.GetRequiredService<IMessageBoxHelper>();
            _mapper = Ioc.Default.GetRequiredService<IMapper>();
            _logger = Ioc.Default.GetLoggerForType<TimeIntervalsViewModel>();

            AddTimeIntervalAsyncCommand = new AsyncRelayCommand(AddTimeIntervalAsync);
            RemoveTimeIntervalAsyncCommand = new AsyncRelayCommand<ListItemModel>(RemoveTimeIntervalAsync);
            StartTimeIntervalAsyncCommand = new AsyncRelayCommand<ListItemModel>(StartTimeIntervalAsync);
            EndTimeIntervalAsyncCommand = new AsyncRelayCommand<ListItemModel>(EndTimeIntervalAsync);
            SaveTimeIntervalNoteCommand = new AsyncRelayCommand<ListItemModel>(SaveTimeIntervalNodeAsync);
            EditStartDateCommand = new RelayCommand<ListItemModel>(EditStartDate);
            EditEndDateCommand = new RelayCommand<ListItemModel>(EditEndDate);

            TimeIntervalsForNode.Clear();

            WeakReferenceMessenger.Default.Register<InformationLoadedMessage>(this, (recipient, message) =>
            {
                Application.Current.Dispatcher.Invoke(() => ReceiveNode(message.Value));
            });

            WeakReferenceMessenger.Default.Register<TimeIntervalEditedMessage>(this, (recipient, message) =>
            {
                ReceiveEditedTimeInterval(message.Value);
            });
        }

        private void EditStartDate(ListItemModel item)
        {
            var tempItem = _mapper.Map<ListItemModel>(item);
            tempItem.EditType = TimeIntervalEditType.StartDate;
            var dialog = new EditTimeIntervalTime();
            dialog.ShowAsync();

            WeakReferenceMessenger.Default.Send(new EditTimeIntervalMessage(tempItem));
        }

        private void EditEndDate(ListItemModel item)
        {
            var tempItem = _mapper.Map<ListItemModel>(item);
            tempItem.EditType = TimeIntervalEditType.EndDate;
            var dialog = new EditTimeIntervalTime();
            dialog.ShowAsync();
            WeakReferenceMessenger.Default.Send(new EditTimeIntervalMessage(tempItem));
        }

        private async Task SaveTimeIntervalNodeAsync(ListItemModel timeInterval)
        {
            try
            {
                TimeInterval entity = await _timeIntervalsService.GetTimeIntervalAsync(timeInterval.Id);
                if (entity != null)
                {
                    entity.Note = timeInterval.Note;
                    await _timeIntervalsService.UpdateAsync(entity);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(SaveTimeIntervalNodeAsync), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy zapisywaniu notatki");
            }
        }

        private async void ReceiveNode(TreeModel message)
        {
            try
            {
                Node = message;

                TimeIntervalsForNode.Clear();

                var times = await GetTimeInteralForTypeAsync(Node.Type);

                if (!times.Any()) return;

                SetTimeIntervalsForView(times);
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(ReceiveNode), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy pobieraniu listy czasów");
            }
        }

        private async Task<List<ListItemModel>> GetTimeInteralForTypeAsync(string type)
        {
            if (type == nameof(Issue))
            {
                var timeIntervalsForIssue = await _timeIntervalsService.GetTimeIntervalsForIssueAsync(Node.Id);
                return _mapper.Map<List<ListItemModel>>(timeIntervalsForIssue);
            }

            if (type == nameof(Comment))
            {
                var timeIntervalsForComment = await _timeIntervalsService.GetTimeIntervalsForCommentAsync(Node.Id);
                return _mapper.Map<List<ListItemModel>>(timeIntervalsForComment);
            }

            return new List<ListItemModel>();
        }

        private void SetTimeIntervalsForView(List<ListItemModel> timeIntervals)
        {
            foreach (var item in timeIntervals)
            {
                TimeIntervalsForNode.Add(item);
            }

            var startedTimeInterval = TimeIntervalsForNode.FirstOrDefault(x => x.IsStarted);
            if (startedTimeInterval != null)
            {
                UpdateCountedTime(startedTimeInterval);
            }
        }

        private void ReceiveEditedTimeInterval(ListItemModel value)
        {
            var listItem = TimeIntervalsForNode.FirstOrDefault(x => x.Id == value.Id);
            if (listItem == null) return;
            if (value.StartDate.HasValue && value.EndDate.HasValue)
            {
                listItem.StartDate = value.StartDate;
                listItem.EndDate = value.EndDate;
                var totalTime = (listItem.EndDate.Value - listItem.StartDate.Value);
                listItem.CountedTime = $"{totalTime.Hours:00}:{totalTime.Minutes:00}:{totalTime.Seconds:00}";
            }
            else if (listItem.IsStarted)
            {
                listItem.StartDate = value.StartDate;
                UpdateCountedTime(listItem);
            }
        }

        private async Task AddTimeIntervalAsync()
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

                    Application.Current.Dispatcher.Invoke(() => TimeIntervalsForNode.Add(timeIntervalModel));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(AddTimeIntervalAsync), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy dodawaniu czasu");
            }
        }

        private async Task RemoveTimeIntervalAsync(ListItemModel item)
        {
            try
            {
                var timeInterval = await _timeIntervalsService.GetTimeIntervalAsync(item.Id);
                if (CurrentNodeTimer != null)
                {
                    await CurrentNodeTimer.DisposeAsync();
                }

                if (timeInterval != null)
                {
                    await _timeIntervalsService.DeleteAsync(timeInterval);
                }

                var deletedTimeInterval = TimeIntervalsForNode.SingleOrDefault(x => x.Id == item.Id);
                Application.Current.Dispatcher.Invoke(() => TimeIntervalsForNode.Remove(deletedTimeInterval));
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(RemoveTimeIntervalAsync), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy usuwaniu czasu");
            }
        }

        private async Task StartTimeIntervalAsync(ListItemModel item)
        {
            try
            {
                if (await _timeIntervalsService.CheckIfAnyStartedTimeIntervalExistAsync())
                {
                    _messageBoxHelper.ShowWarningInfoBox("Istnieje inne niezakończone zadanie!", "Uwaga");
                    return;
                }

                var listItem = TimeIntervalsForNode.FirstOrDefault(x => x.Id == item.Id);
                var timeInterval = await _timeIntervalsService.GetTimeIntervalAsync(item.Id);
                if (timeInterval != null)
                {
                    if (listItem != null)
                    {
                        listItem.StartDate = DateTime.Now;
                        listItem.IsStarted = true;
                    }

                    timeInterval.TimeIntervalStart = item.StartDate;
                    await _timeIntervalsService.UpdateAsync(timeInterval);
                }

                UpdateCountedTime(listItem);
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(StartTimeIntervalAsync), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy starcie czasu");
            }
        }

        private async Task EndTimeIntervalAsync(ListItemModel item)
        {
            var timeInterval = await _timeIntervalsService.GetTimeIntervalAsync(item.Id);
            try
            {
                var listItem = TimeIntervalsForNode.FirstOrDefault(x => x.Id == item.Id);
                if (timeInterval != null)
                {
                    if (listItem != null)
                    {
                        listItem.EndDate = DateTime.Now;
                        listItem.IsStarted = false;
                    }

                    timeInterval.TimeIntervalEnd = item.EndDate;
                    timeInterval.IsStarted = false;
                    await _timeIntervalsService.UpdateAsync(timeInterval);
                }

                CurrentNodeTimer.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(StartTimeIntervalAsync), ex.Message);
                _messageBoxHelper.ShowWarningInfoBox(ex.Message, "Wystąpił problem przy stopie czasu");
            }
        }

        private void UpdateCountedTime(ListItemModel item)
        {

            CurrentNodeTimer = new Timer(SetCountedTime, item, 0, 1000);
        }

        private void SetCountedTime(object state)
        {
            if (!(state is ListItemModel { IsStarted: true, StartDate: { } } timeInterval)) return;
            var totalTime = (DateTime.Now - timeInterval.StartDate.Value);
            timeInterval.CountedTime = totalTime.Days > 0 ? $"{totalTime.Days} d, {totalTime.Hours}:{totalTime.Minutes}:{totalTime.Seconds}" : $"{totalTime.Hours}:{totalTime.Minutes}:{totalTime.Seconds}";
        }
    }
}
