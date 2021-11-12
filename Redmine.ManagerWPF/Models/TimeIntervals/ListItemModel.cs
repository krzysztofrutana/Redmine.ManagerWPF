using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redmine.ManagerWPF.Desktop.Models.TimeIntervals
{
    public class ListItemModel : ObservableObject
    {
        public int Id { get; set; }

        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get => _startDate; set => SetProperty(ref _startDate, value);

        }
        private string _countedTime;
        public string CountedTime
        {
            get => _countedTime;
            set
            {
                SetProperty(ref _countedTime, value);
            }
        }

        private DateTime? _endDate;
        public DateTime? EndDate { get => _endDate; set => SetProperty(ref _endDate, value); }

        public string Note { get; set; }

        private bool _isStarted;
        public bool IsStarted { get => _isStarted; set => SetProperty(ref _isStarted, value); }
    }
}
