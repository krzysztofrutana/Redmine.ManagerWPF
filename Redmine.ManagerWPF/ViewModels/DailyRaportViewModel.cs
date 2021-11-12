using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Redmine.ManagerWPF.Desktop.Helpers;
using Redmine.ManagerWPF.Desktop.Models.DailyRaport;
using Redmine.ManagerWPF.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class DailyRaportViewModel : ObservableRecipient
    {
        private bool _dataLoading;

        public bool DataLoading
        {
            get => _dataLoading;
            set
            {
                SetProperty(ref _dataLoading, value);
            }
        }
        public ObservableCollection<ListItemModel> RaportData { get; set; } = new ObservableCollection<ListItemModel>();


        private readonly TimeIntervalsService _timeIntervalsService;

        public DailyRaportViewModel()
        {
            _timeIntervalsService = Ioc.Default.GetRequiredService<TimeIntervalsService>();

            DataLoading = false;
            Task.Run(async () => await LoadRaportData());
        }

        private async Task LoadRaportData()
        {
            var timeIntervalForToday = await _timeIntervalsService.GetFinishedForCurrentDateAsync(DateTime.Now);
        }
    }
}
