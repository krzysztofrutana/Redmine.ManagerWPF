using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Data.Enums;
using Redmine.ManagerWPF.Data.Models;
using Redmine.ManagerWPF.Desktop.Messages;
using Redmine.ManagerWPF.Desktop.Models.TimeIntervals;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class EditTimeIntervalTimeViewModel : ObservableObject
    {
        private ListItemModel _selectedTimeInterval;

        public ListItemModel SelectedTimeInterval
        {
            get => _selectedTimeInterval;
            set => SetProperty(ref _selectedTimeInterval, value);
        }

        private DateTime? _selectedDate;

        public DateTime? DateTimeToEdit
        {
            get { return _selectedDate; }
            set { SetProperty(ref _selectedDate, value); }
        }

        private string _errorText;

        public string ErrorText
        {
            get { return _errorText; }
            set { SetProperty(ref _errorText, value); }
        }

        private bool _isError;

        public bool IsError
        {
            get { return _isError; }
            set { SetProperty(ref _isError, value); }
        }


        public IAsyncRelayCommand<ICloseable> SaveTimeIntervalCommand { get; }
        public IRelayCommand<ICloseable> CloseDialogCommand { get; }

        private readonly TimeIntervalsService _timeIntervalsService;
        private readonly IMessageBoxService _messageBoxService;

        public EditTimeIntervalTimeViewModel()
        {
            _timeIntervalsService = Ioc.Default.GetRequiredService<TimeIntervalsService>();
            _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();

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
                        if (SelectedTimeInterval.EditType == TimeIntervalEditType.StartDate)
                        {
                            if (DateTimeToEdit >= SelectedTimeInterval.EndDate)
                            {
                                ErrorText = "Czas startowy musi być mniejszy od końcowego";
                                IsError = true;
                                return;
                            }
                            else
                            {
                                IsError = false;
                            }

                            entity.TimeIntervalStart = DateTimeToEdit;
                            SelectedTimeInterval.StartDate = DateTimeToEdit;
                            await _timeIntervalsService.UpdateAsync(entity);

                            WeakReferenceMessenger.Default.Send(new TimeIntervalEditedMessage(SelectedTimeInterval));

                            dialog.Close();
                        }
                        else if (SelectedTimeInterval.EditType == TimeIntervalEditType.EndDate)
                        {
                            if (DateTimeToEdit <= SelectedTimeInterval.StartDate)
                            {
                                ErrorText = "Czas końca musi być większy od startowego";
                                IsError = true;
                                return;
                            }
                            else
                            {
                                IsError = false;
                            }
                            entity.TimeIntervalEnd = DateTimeToEdit;
                            SelectedTimeInterval.EndDate = DateTimeToEdit;
                            await _timeIntervalsService.UpdateAsync(entity);

                            WeakReferenceMessenger.Default.Send(new TimeIntervalEditedMessage(SelectedTimeInterval));
                            dialog.Close();
                        }
                    }
                    else
                    {
                        _messageBoxService.ShowWarningInfoBox("Nie znaleziono wpisu w bazie danych", "Błąd");
                    }
                }
                catch (Exception ex)
                {

                    _messageBoxService.ShowWarningInfoBox(ex.Message, "Edycja czasu nieudana");
                }

            }
        }

        private void CloseDialog(ICloseable dialog)
        {
            if (dialog != null)
            {
                dialog.Close();
            }
        }
    }
}