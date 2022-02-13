using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Desktop.Extensions;
using Redmine.ManagerWPF.Desktop.Messages;
using Redmine.ManagerWPF.Desktop.Models.TimeIntervals;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class EditTimeIntervalTimeViewModel : ObservableObject
    {
        private ListItemModel _selectedTimeInterval;

        private ListItemModel SelectedTimeInterval
        {
            get => _selectedTimeInterval;
            set => SetProperty(ref _selectedTimeInterval, value);
        }

        private DateTime? _selectedDate;

        public DateTime? DateTimeToEdit
        {
            get => _selectedDate;
            set => SetProperty(ref _selectedDate, value);
        }

        private string _errorText;

        public string ErrorText
        {
            get => _errorText;
            private set => SetProperty(ref _errorText, value);
        }

        private bool _isError;

        public bool IsError
        {
            get => _isError;
            set => SetProperty(ref _isError, value);
        }


        public IAsyncRelayCommand<ICloseable> SaveTimeIntervalCommand { get; }
        public IRelayCommand<ICloseable> CloseDialogCommand { get; }

        private readonly TimeIntervalsService _timeIntervalsService;
        private readonly IMessageBoxService _messageBoxService;
        private readonly ILogger<EditTimeIntervalTimeViewModel> _logger;

        public EditTimeIntervalTimeViewModel()
        {
            _timeIntervalsService = Ioc.Default.GetRequiredService<TimeIntervalsService>();
            _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();
            _logger = Ioc.Default.GetLoggerForType<EditTimeIntervalTimeViewModel>();

            SaveTimeIntervalCommand = new AsyncRelayCommand<ICloseable>(SaveTimeIntervalAsync);
            CloseDialogCommand = new RelayCommand<ICloseable>(CloseDialog);

            WeakReferenceMessenger.Default.Register<EditTimeIntervalMessage>(this, (r, m) =>
            {
                if (m.Value != null)
                {
                    SelectedTimeInterval = m.Value;
                    if (SelectedTimeInterval.EditType == TimeIntervalEditType.StartDate)
                    {
                        DateTimeToEdit = SelectedTimeInterval.StartDate;
                    }
                    else if (SelectedTimeInterval.EditType == TimeIntervalEditType.EndDate)
                    {
                        DateTimeToEdit = SelectedTimeInterval.EndDate;
                    }
                }
            });
        }

        private async Task SaveTimeIntervalAsync(ICloseable dialog)
        {
            if (SelectedTimeInterval != null)
            {
                try
                {
                    var entity = await _timeIntervalsService.GetTimeIntervalAsync(SelectedTimeInterval.Id);
                    if (entity != null)
                    {
                        switch (SelectedTimeInterval.EditType)
                        {
                            case TimeIntervalEditType.StartDate when DateTimeToEdit >= SelectedTimeInterval.EndDate:
                                ErrorText = "Czas startowy musi być mniejszy od końcowego";
                                IsError = true;
                                return;
                            case TimeIntervalEditType.EndDate when DateTimeToEdit <= SelectedTimeInterval.StartDate:
                                ErrorText = "Czas końca musi być większy od startowego";
                                IsError = true;
                                return;
                            case TimeIntervalEditType.StartDate:
                                {
                                    entity.TimeIntervalStart = DateTimeToEdit;
                                    SelectedTimeInterval.StartDate = DateTimeToEdit;
                                    await _timeIntervalsService.UpdateAsync(entity);

                                    WeakReferenceMessenger.Default.Send(new TimeIntervalEditedMessage(SelectedTimeInterval));

                                    dialog.Close();
                                    IsError = false;
                                    break;
                                }
                            case TimeIntervalEditType.EndDate:
                                {
                                    entity.TimeIntervalEnd = DateTimeToEdit;
                                    SelectedTimeInterval.EndDate = DateTimeToEdit;
                                    await _timeIntervalsService.UpdateAsync(entity);

                                    WeakReferenceMessenger.Default.Send(new TimeIntervalEditedMessage(SelectedTimeInterval));
                                    dialog.Close();
                                    IsError = false;
                                    break;
                                }
                        }
                    }
                    else
                    {
                        _messageBoxService.ShowWarningInfoBox("Nie znaleziono wpisu w bazie danych", "Błąd");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("{0} {1}", nameof(SaveTimeIntervalAsync), ex.Message);
                    _messageBoxService.ShowWarningInfoBox(ex.Message, "Edycja czasu nieudana");
                }

            }
        }

        private void CloseDialog(ICloseable dialog)
        {
            dialog?.Close();
        }
    }
}